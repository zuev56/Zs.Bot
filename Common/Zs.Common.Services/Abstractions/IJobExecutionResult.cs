using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zs.Common.Services.Abstractions
{
    [Obsolete(" Вместо этого использовать IServiceResult")]
    public interface IJobExecutionResult
    {
        string TextValue { get; }
    }
}
