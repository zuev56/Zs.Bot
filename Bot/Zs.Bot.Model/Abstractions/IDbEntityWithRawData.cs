using System;

namespace Zs.Bot.Data.Abstractions
{
    /// <summary>
    /// Database entity with properties containing raw data and raw data history
    /// </summary>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public interface IDbEntityWithRawData<TEntity, TKey> : IDbEntity<TEntity, TKey>, IEquatable<TEntity>
    {
        string RawData { get; set; }
        string RawDataHash { get; set; }
        string RawDataHistory { get; set; }
    }
}
