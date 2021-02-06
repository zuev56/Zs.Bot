using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Abstractions;

namespace Zs.Bot.Data.Repositories
{
    /// <summary>
    /// Universal repository
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public class CommonRepository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IDbEntity<TEntity, TKey>
        where TContext : DbContext
    {
        protected IContextFactory<TContext> ContextFactory { get; }

        // TODO: Make specific delegate type
        /// <summary> Calls before items update. 
        /// First argument - saving item, second argument - existing item from database </summary>
        protected Action<TEntity, TEntity> BeforeUpdateItem { get; set; }
        
        public CommonRepository(IContextFactory<TContext> contextFactory)
        {
            ContextFactory = contextFactory;
        }

        public virtual async Task<TEntity> FindByKeyAsync(TKey id, CancellationToken cancellationToken = default)
        {
            using var context = ContextFactory.GetContext();
            return await context.Set<TEntity>().FirstOrDefaultAsync(i => i.Id.Equals(id));
        }

        public virtual async Task<TEntity> FindBySqlAsync(string query, CancellationToken cancellationToken = default)
        {
            using var context = ContextFactory.GetContext();
            return await context.Set<TEntity>().FromSqlRaw(query).FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> FindAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            CancellationToken cancellationToken = default)
        {
            using var context = ContextFactory.GetContext();

            IQueryable<TEntity> query = context.Set<TEntity>();

            if (orderBy != null)
                query = orderBy(query);

            return predicate is null
                ? await query.FirstOrDefaultAsync()
                : await query.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<List<TEntity>> FindAllAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            uint? skip = null,
            uint? take = null,
            CancellationToken cancellationToken = default)
        {
            using var context = ContextFactory.GetContext();

            IQueryable<TEntity> query = context.Set<TEntity>();

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            if (skip != null)
                query = query.Skip((int)skip);

            if (take != null)
                query = query.Take((int)take);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<bool> SaveAsync(TEntity item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            var itemToSave = item.GetItemToSave?.Invoke();

            using var context = ContextFactory.GetContext();

            var existingItem = await context.Set<TEntity>().FirstOrDefaultAsync(i => i.Id.Equals(itemToSave.Id), cancellationToken);
            if (existingItem != null)
            {
                // Make changes directly in the itemToSave
                itemToSave = itemToSave.GetItemToUpdate(existingItem);

                // Make changes in child repository
                BeforeUpdateItem?.Invoke(itemToSave, existingItem);

                if (!itemToSave.Equals(existingItem))
                {
                    context.Entry(existingItem).State = EntityState.Detached;
                    context.Entry(itemToSave).State = EntityState.Modified;
                    context.Set<TEntity>().Update(itemToSave);
                }                
            }
            else
            {
                context.Set<TEntity>().Add(itemToSave);
            }

            if (context.ChangeTracker.HasChanges())
            {
                int changes = await context.SaveChangesAsync(cancellationToken);
                item.Id = itemToSave.Id;
                return changes == 1;
            }

            return false;
        }

        public virtual async Task<bool> DeleteAsync(TEntity item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            using var context = ContextFactory.GetContext();

            var existingItem = await context.Set<TEntity>().FirstOrDefaultAsync(i => i.Id.Equals(item.Id), cancellationToken);
            if (existingItem != null)
            {
                context.Set<TEntity>().Remove(existingItem);
                return await context.SaveChangesAsync(cancellationToken) == 1;
            }
            else
            {
                return false;
            }
        }
    }
}
