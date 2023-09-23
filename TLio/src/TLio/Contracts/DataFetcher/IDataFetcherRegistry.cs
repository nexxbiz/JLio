namespace TLio.Contracts.DataFetcher
{
    public interface IDataFetcherRegistry
    {
        void Register(Type fetcher, string name);

        public IDataFetcher GetFetcher(string fetcher);
    }
}