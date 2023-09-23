namespace TLio.Services.DataFetcher
{
    public class FetchedItem
    {
        public object Item { get; set; }
        public TargetTypes ItemType { get; set; }
        public string Path { get; set; }
    }
}