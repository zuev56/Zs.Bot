using System.Collections.Generic;
using System.Linq;
using Zs.Common.Abstractions;
using Zs.Common.Enums;

namespace Zs.Common.Models
{
    public class ServiceResult : IServiceResult
    {
        // TODO: remove  m != null  and fix
        public bool IsSuccess => Messages.All(m => m != null && m.Type != InfoMessageType.Error);
        public bool HasWarnings => Messages.All(m => m != null && m.Type != InfoMessageType.Warning);
        public IInfoMessage MessageToAdd { set => Messages.Add(value); }
        public IList<IInfoMessage> Messages { get; } = new List<IInfoMessage>();

        protected ServiceResult()
        {
        }
        // TODO: make base method for others
        public static ServiceResult Success(string message = null)
        {
            var result = new ServiceResult();

            if (!string.IsNullOrWhiteSpace(message))
                result.Messages.Add(InfoMessage.Success(message));

            return result;
        }

        public static ServiceResult Warning(string message)
        {
            var result = new ServiceResult();

            if (!string.IsNullOrWhiteSpace(message))
                result.Messages.Add(InfoMessage.Warning(message));

            return result;
        }

        public static ServiceResult Error(string message)
        {
            var result = new ServiceResult();

            if (!string.IsNullOrWhiteSpace(message))
                result.Messages.Add(InfoMessage.Error(message));

            return result;
        }
    }

    public class ServiceResult<TResult> : IServiceResult<TResult>
    {
        public bool IsSuccess => Messages.All(m => m.Type != InfoMessageType.Error);
        public bool HasWarnings => Messages.All(m => m.Type != InfoMessageType.Warning);
        public IInfoMessage Message { set => Messages.Add(value); }
        public IList<IInfoMessage> Messages { get; } = new List<IInfoMessage>();
        public TResult Result { get; init; }

        protected ServiceResult()
        {
        }

        // TODO: make base method for others
        public static ServiceResult<TResult> Success(TResult result, string message = null)
        {
            var res = new ServiceResult<TResult>()
            {
                Result = result
            };

            if (!string.IsNullOrWhiteSpace(message))
                res.Messages.Add(InfoMessage.Success(message));

            return res;
        }

        public static ServiceResult<TResult> Warning(TResult result, string message = null)
        {
            var res = new ServiceResult<TResult>()
            {
                Result = result
            };

            if (!string.IsNullOrWhiteSpace(message))
                res.Messages.Add(InfoMessage.Warning(message));

            return res;
        }

        public static ServiceResult<TResult> Error(TResult result = default, string message = null)
        {
            var res = new ServiceResult<TResult>()
            {
                Result = result
            };

            if (!string.IsNullOrWhiteSpace(message))
                res.Messages.Add(InfoMessage.Error(message));

            return res;
        }

        public static ServiceResult<TResult> Error(string message)
        {
            var res = new ServiceResult<TResult>();

            if (!string.IsNullOrWhiteSpace(message))
                res.Messages.Add(InfoMessage.Success(message));

            return res;
        }
    }
}
