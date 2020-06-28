namespace Zs.Common.Interfaces
{
    public interface IZsConfiguration
    {
        public string ConnectionString { get; }
        public string WorkPath         { get; }
        public string BotToken         { get; }
        public string ProxySocket      { get; }
        public string ProxyLogin       { get; }
        public string ProxyPassword    { get; }
        public int    DefaultChatId    { get; }
    }
}
