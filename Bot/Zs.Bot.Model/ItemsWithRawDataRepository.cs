using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;

namespace Zs.Bot.Data
{
    /// <summary>
    /// Repository for items containing raw data
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public sealed class ItemsWithRawDataRepository<TEntity, TKey> : CommonRepository<TEntity, TKey>, IItemsWithRawDataRepository<TEntity, TKey>
        where TEntity : class, IDbEntityWithRawData<TEntity, TKey>
    {

        public ItemsWithRawDataRepository(IContextFactory<BotContext> contextFactory)
            : base(contextFactory)
        {
            BeforeUpdateItem = (item, existingItem) 
                => MergeRawDataFields(item, existingItem);
        }
                
        public async Task<TKey> GetActualIdByRawDataHashAsync(TEntity item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            using var context = ContextFactory.GetContext();

            TEntity dbItem = await context.Set<TEntity>().FirstOrDefaultAsync(i => i.RawDataHash == i.RawDataHash);

            return dbItem != default && !dbItem.Id.Equals(default(TKey))
                ? dbItem.Id
                : throw new ItemNotFoundException(item);
        }

        /// <summary>
        /// If items RawData properties are different, copies RawDataHistory and compement it with current
        /// </summary>
        /// <param name="existingItem"></param>
        /// <param name="newItem"></param>
        private void MergeRawDataFields(TEntity newItem, TEntity existingItem)
        {
            if (newItem.RawData != existingItem.RawData)
            {
                // Копируем историю из старого 
                newItem.RawDataHistory = existingItem.RawDataHistory;

                // Создаём историю из текущего значения RawData в БД, если её не было ранее
                if (newItem.RawData is not null
                    && newItem.RawDataHistory is null)
                {
                    newItem.RawDataHistory = $"[{existingItem.RawData}]".NormalizeJsonString();
                }
                // Иначе дополняем историю текущим значением RawData из БД
                else if (newItem.RawDataHistory is not null)
                {
                    JArray rawDataHistory = JArray.Parse(newItem.RawDataHistory);
                    rawDataHistory.Add(existingItem.RawData);
                    newItem.RawDataHistory = rawDataHistory.ToString().NormalizeJsonString();
                }

                // Пересчитываем хеш
                newItem.RawDataHash = newItem.RawData.GetMD5Hash();
            }
        }
    }
}
