using System.Collections.Generic;
using System.Linq;
using Zs.Common.Abstractions;
using Zs.Common.Enums;

namespace Zs.App.Home.Web.Models
{
    public class ServiceResponseVM : IServiceResult
    {
        public bool IsSuccess => Messages.All(m => m.Type != InfoMessageType.Error);
        public IInfoMessage Message { set => Messages.Add(value); }
        public IList<IInfoMessage> Messages { get; } = new List<IInfoMessage>();

        public static ServiceResponseVM Success(string message)
            => new ServiceResponseVM
            {
                Message = InfoMessageVM.Success(message)
            };

        public static ServiceResponseVM Warning(string message)
            => new ServiceResponseVM
            {
                Message = InfoMessageVM.Warning(message)
            };

        public static ServiceResponseVM Error(string message)
            => new ServiceResponseVM
            {
                Message = InfoMessageVM.Error(message)
            };
    }

    public class ServiceResponseVM<TResult> : IServiceResult<TResult>
    {
        public bool IsSuccess => Messages.All(m => m.Type != InfoMessageType.Error);
        public IInfoMessage Message { set => Messages.Add(value); }
        public IList<IInfoMessage> Messages { get; } = new List<IInfoMessage>();
        public TResult Result { get; init; }
    }
}
