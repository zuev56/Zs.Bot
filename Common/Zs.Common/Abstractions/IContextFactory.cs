using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zs.Common.Abstractions
{
    public interface IContextFactory<TContext>
       where TContext : DbContext
    {
        TContext GetContext();
    }
}
