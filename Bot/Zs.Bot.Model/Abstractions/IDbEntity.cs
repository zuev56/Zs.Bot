using System;

namespace Zs.Bot.Data.Abstractions
{
    /// <summary>
    /// Database entity
    /// </summary>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public interface IDbEntity<TEntity, TKey>
    {
        TKey Id { get; set; }

        // TODO: Create specific delegete types

        /// <summary> Returns the entity prepared for saving to DB </summary>
        Func<TEntity> GetItemToSave { get; }
        
        /// <summary> Helps to update only fields you want and do not change others.
        /// Argument - item existing in database </summary>
        Func<TEntity, TEntity> GetItemToUpdate{ get; }
    }
}
