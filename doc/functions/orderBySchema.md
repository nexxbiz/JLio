# orderBySchema

Orders the properties in a JSON structure to match the order defined in a schema.

## Arguments
- a JSON schema or a path to a schema.

## Description

The `orderBySchema` function reorders the properties in JSON objects to match the property order specified in a JSON schema. Properties that are defined in the schema will be ordered first according to their declaration order in the schema. Any properties that are not mentioned in the schema will be preserved but appended at the end of the properties list.

This function:
- Works recursively on nested objects and arrays
- Preserves all original data - no properties are removed
- Maintains the original values of all properties
- Only changes the order of properties to match the schema structure

## Examples

### Basic Usage

```json
{
  "schema": {
    "type": "object",
    "properties": {
      "lastName": {"type": "string"},
      "firstName": {"type": "string"},
      "age": {"type": "integer"}
    }
  },
  "person": {
    "age": 30,
    "firstName": "John",
    "country": "USA",
    "lastName": "Doe"
  }
}
```

Using `=orderBySchema($.schema)` on the `person` object would result in:

```json
{
  "lastName": "Doe",
  "firstName": "John", 
  "age": 30,
  "country": "USA"
}
```

Note that `lastName`, `firstName`, and `age` are now ordered according to the schema, while `country` (not in the schema) is appended at the end.

### Nested Objects

The function works recursively on nested objects:

```json
{
  "schema": {
    "type": "object",
    "properties": {
      "user": {
        "type": "object",
        "properties": {
          "lastName": {"type": "string"},
          "firstName": {"type": "string"}
        }
      },
      "timestamp": {"type": "string"}
    }
  },
  "data": {
    "timestamp": "2023-01-01",
    "user": {
      "age": 25,
      "firstName": "Alice",
      "lastName": "Smith"
    }
  }
}
```

Results in both the top-level and nested properties being reordered according to the schema.

### Array Items

When working with arrays, the function applies the schema ordering to each item in the array:

```json
{
  "schema": {
    "type": "object", 
    "properties": {
      "items": {
        "type": "array",
        "items": {
          "type": "object",
          "properties": {
            "id": {"type": "string"},
            "name": {"type": "string"}
          }
        }
      }
    }
  }
}
```

Each object in the `items` array will have its properties reordered to match the schema definition.

## Use Cases

- Standardizing the order of properties in API responses
- Preparing data for display where property order matters
- Ensuring consistent JSON structure for downstream processing
- Organizing configuration objects according to a schema template

## Fluent API

The function supports fluent API usage:

```csharp
var script = new JLioScript()
    .Set(OrderBySchemaBuilders.OrderBySchema(schema))
    .OnPath("$.data");
```

Or with a path to a schema:

```csharp
var script = new JLioScript()
    .Set(OrderBySchemaBuilders.OrderBySchema("$.schema"))
    .OnPath("$.data");
```