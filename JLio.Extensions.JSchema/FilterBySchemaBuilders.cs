using NewtonsoftJSchema = Newtonsoft.Json.Schema.JSchema;

namespace JLio.Extensions.JSchema;

public static class FilterBySchemaBuilders
{
    public static FilterBySchema FilterBySchema(NewtonsoftJSchema schema)
    {
        return new FilterBySchema(schema);
    }

    public static FilterBySchema FilterBySchema(string path)
    {
        return new FilterBySchema(path);
    }
}
