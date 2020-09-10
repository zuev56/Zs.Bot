using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Zs.Common.Abstractions
{
    //public interface IContextFactory<TContext, TOptions>
    //    where TContext : DbContext
    //    where TOptions : DbContextOptions<TContext>
    //{
    //    TContext GetContext();
    //    void Initialize(TOptions options);
    //}

    public interface IContextFactory<TContext>
       where TContext : DbContext
    {
        TContext GetContext();
    }
}
