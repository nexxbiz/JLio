namespace TLio.Services.DataFetcher
{
    public class FetchedItem<T>
    {
        public T? Item { get; set; }
        public TargetTypes ItemType { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}