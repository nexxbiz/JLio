using System;
using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using NewtonsoftJSchema = Newtonsoft.Json.Schema.JSchema;

namespace JLio.Extensions.JSchema
{
    public static class JSchemaExtensions
    {
        public static List<SchemaPropertyInfo> GetPaths(this NewtonsoftJSchema schema, string currentPath = "$",
            string parentPath = "$", List<SchemaPropertyInfo> paths = default)
        {
            if (paths == null)
            {
                paths = new List<SchemaPropertyInfo>();
            }

            if (schema.Type == JSchemaType.Object)
            {
                parentPath = currentPath;
                paths.Add(new SchemaPropertyInfo(SchemaPropertyType.Object, parentPath));
                foreach (var schemaProperty in schema.Properties)
                {
                    currentPath = $"{parentPath}.{schemaProperty.Key}";
                    GetPaths(schemaProperty.Value, currentPath, schemaProperty.Key, paths);
                }
            }
            else if (schema.Type == JSchemaType.Array)
            {
                parentPath = currentPath;
                paths.Add(new SchemaPropertyInfo(SchemaPropertyType.Array, parentPath));

                for (int i = 0; i < schema.Items.Count; i++)
                {
                    if (schema.Items.Count == 1)
                    {
                        currentPath = $"{parentPath}[*]";
                    }
                    else
                    {
                        currentPath = $"{parentPath}[{i}]";
                    }

                    GetPaths(schema.Items[i], currentPath, parentPath, paths);
                }

            }
            else
            {
                paths.Add(new SchemaPropertyInfo(SchemaPropertyType.Primitive, currentPath));
            }

            return paths;
        }
    }

    public class SchemaPropertyInfo
    {
        public SchemaPropertyType Type { get; }

        public string Path { get; }

        public SchemaPropertyInfo(SchemaPropertyType type, string path)
        {
            Type = type;
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }

    public enum SchemaPropertyType
    {
        Object,
        Array,
        Primitive
    }
}