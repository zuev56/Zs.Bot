using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zs.Bot.Data.Abstractions
{
    public interface IItemsWithRawDataRepository<TEntity, TKey> : IRepository<TEntity, TKey> 
    {
        //Task<bool> UpdateRawDataAsync(TEntity item);
        Task<TKey> GetActualIdByRawDataHashAsync(TEntity item);
    }
}
