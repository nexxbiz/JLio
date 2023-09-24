namespace TLio.Contracts.DataFetcher
{
    public interface IDataFetcherRegistry<T>
    {
        void Register(Type fetcher, string name);

        public IDataFetcher<T> GetFetcher(string fetcher);
    }
}