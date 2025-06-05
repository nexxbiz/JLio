using Newtonsoft.Json.Schema;

namespace JLio.Extensions.JSchema;

public static class FilterBySchemaBuilders
{
    public static FilterBySchema FilterBySchema(JSchema schema)
    {
        return new FilterBySchema(schema);
    }

    public static FilterBySchema FilterBySchema(string path)
    {
        return new FilterBySchema(path);
    }
}
