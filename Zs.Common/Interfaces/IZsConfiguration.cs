namespace Zs.Common.Interfaces
{
    public interface IZsConfiguration
    {
        string ConnectionString { get; }

        string WorkPath { get; }
    }
}
