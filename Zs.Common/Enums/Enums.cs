namespace Zs.Common.Enums
{
    public enum QueryResultType : short
    {
        Undefined = -1,
        NoResult = 0,
        Json,
        String,
        Double
    }

    public enum ConnectionStatus : short
    {
        Undefined = -1,
        Ok,
        NoProxyConnection,
        NoInternetConnection
    }

    public enum LogType : short
    {
        Info = 0,
        Warning,
        Error
    }

    public enum ChatType : short
    {
        Unknown = -1,
        Undefined = -1,
        Private = 0,
        Group,
        Channel
    }

    public enum MessageAction : short
    {
        Undefined = -1,
        Received = 0,
        Sent,
        Deleted
    }

    public enum OperationResult : short
    {
        Undefined = -1,
        Success,
        Failure,
        Retry
    }
}
