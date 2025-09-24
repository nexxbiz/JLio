using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NewtonsoftJSchema = Newtonsoft.Json.Schema.JSchema;

namespace JLio.Extensions.JSchema;

public class OrderBySchema : FunctionBase
{
    public OrderBySchema()
    {
    }

    public OrderBySchema(NewtonsoftJSchema schema)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(schema.ToString())));
    }

    public OrderBySchema(string path)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(path)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Count == 0 || Arguments.Count > 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires one argument: path of the schema or the schema itself");
            return JLioFunctionResult.Failed(currentToken);
        }

        var values = GetArguments(Arguments, currentToken, dataContext, context);

        var schema = JsonConvert.DeserializeObject<NewtonsoftJSchema>(values[0].ToString());
        var schemaPropertyOrder = GetSchemaPropertyOrder(schema);

        var currentObject = currentToken.DeepClone();
        var orderedObject = OrderTokenBySchema(currentObject, schemaPropertyOrder, "$");

        return new JLioFunctionResult(true, orderedObject);
    }

    private Dictionary<string, List<string>> GetSchemaPropertyOrder(NewtonsoftJSchema schema)
    {
        var propertyOrder = new Dictionary<string, List<string>>();
        BuildPropertyOrder(schema, "$", propertyOrder);
        return propertyOrder;
    }

    private void BuildPropertyOrder(NewtonsoftJSchema schema, string currentPath, Dictionary<string, List<string>> propertyOrder)
    {
        if (schema.Type == JSchemaType.Object && schema.Properties != null)
        {
            var properties = schema.Properties.Keys.ToList();
            propertyOrder[currentPath] = properties;

            foreach (var property in schema.Properties)
            {
                var childPath = currentPath == "$" ? $"$.{property.Key}" : $"{currentPath}.{property.Key}";
                BuildPropertyOrder(property.Value, childPath, propertyOrder);
            }
        }
        else if (schema.Type == JSchemaType.Array && schema.Items != null && schema.Items.Count > 0)
        {
            var arrayItemPath = currentPath + "[*]";
            BuildPropertyOrder(schema.Items.First(), arrayItemPath, propertyOrder);
        }
    }

    private JToken OrderTokenBySchema(JToken token, Dictionary<string, List<string>> schemaPropertyOrder, string currentPath)
    {
        if (token.Type == JTokenType.Object)
        {
            return OrderObject((JObject)token, schemaPropertyOrder, currentPath);
        }
        else if (token.Type == JTokenType.Array)
        {
            return OrderArray((JArray)token, schemaPropertyOrder, currentPath);
        }

        return token;
    }

    private JObject OrderObject(JObject obj, Dictionary<string, List<string>> schemaPropertyOrder, string currentPath)
    {
        var orderedObject = new JObject();

        // Get schema property order for this path
        if (schemaPropertyOrder.TryGetValue(currentPath, out var schemaOrder))
        {
            // First add properties in schema order
            foreach (var propName in schemaOrder)
            {
                if (obj.ContainsKey(propName))
                {
                    var childPath = currentPath == "$" ? $"$.{propName}" : $"{currentPath}.{propName}";
                    orderedObject[propName] = OrderTokenBySchema(obj[propName], schemaPropertyOrder, childPath);
                }
            }

            // Then add any remaining properties not in schema
            foreach (var property in obj.Properties())
            {
                if (!schemaOrder.Contains(property.Name))
                {
                    var childPath = currentPath == "$" ? $"$.{property.Name}" : $"{currentPath}.{property.Name}";
                    orderedObject[property.Name] = OrderTokenBySchema(property.Value, schemaPropertyOrder, childPath);
                }
            }
        }
        else
        {
            // No schema order defined, process all properties recursively but maintain original order
            foreach (var property in obj.Properties())
            {
                var childPath = currentPath == "$" ? $"$.{property.Name}" : $"{currentPath}.{property.Name}";
                orderedObject[property.Name] = OrderTokenBySchema(property.Value, schemaPropertyOrder, childPath);
            }
        }

        return orderedObject;
    }

    private JArray OrderArray(JArray array, Dictionary<string, List<string>> schemaPropertyOrder, string currentPath)
    {
        var orderedArray = new JArray();

        foreach (var item in array)
        {
            var arrayItemPath = currentPath + "[*]";
            orderedArray.Add(OrderTokenBySchema(item, schemaPropertyOrder, arrayItemPath));
        }

        return orderedArray;
    }
}