namespace SystemEvents.Utils.Interfaces
{
    public interface IElasticsearchIndexFactory
    {
        string GetIndexName();
        string GetPreviousIndexName();
    }
}