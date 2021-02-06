using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Bot.Data.Abstractions
{
    /// <summary>  </summary>
    /// <typeparam name="TEntity">Search value type</typeparam>
    public interface IRepository<TEntity, TKey>
        where TEntity : class, IDbEntity<TEntity, TKey>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity> FindByKeyAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity> FindBySqlAsync(string query, CancellationToken cancellationToken = default);


        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified
        /// condition or a default value if no such element is found
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="orderBy"> Sorting rules before executing predicate</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity> FindAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously returns the list of elements of a sequence that satisfies a specified condition
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="orderBy"> Sorting rules before executing predicate</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> FindAllAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            uint? skip = null,
            uint? take = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Save new item or update existing item in database
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>TRUE if saved or updated, otherwise FALSE</returns>
        Task<bool> SaveAsync(TEntity item, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete existing item from database
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>TRUE if deleted, otherwise FALSE</returns>
        Task<bool> DeleteAsync(TEntity item, CancellationToken cancellationToken = default);
    }
}
