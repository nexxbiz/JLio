namespace JLio.Extensions.JSchema;

public static class FilterBySchemaBuilders
{
    public static FilterBySchema FilterBySchema(Newtonsoft.Json.Schema.JSchema schema)
    {
        return new FilterBySchema(schema);
    }

    public static FilterBySchema FilterBySchema(string path)
    {
        return new FilterBySchema(path);
    }
}
