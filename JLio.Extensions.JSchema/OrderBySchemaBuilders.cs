using NewtonsoftJSchema = Newtonsoft.Json.Schema.JSchema;

namespace JLio.Extensions.JSchema;

public static class OrderBySchemaBuilders
{
    public static OrderBySchema OrderBySchema(NewtonsoftJSchema schema)
    {
        return new OrderBySchema(schema);
    }

    public static OrderBySchema OrderBySchema(string path)
    {
        return new OrderBySchema(path);
    }
}