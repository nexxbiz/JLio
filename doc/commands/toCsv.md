# ToCsv Command Documentation

## Overview

The `ToCsv` command converts JSON objects or arrays of objects into CSV (Comma-Separated Values) format. It follows RFC 4180 standards and includes comprehensive options for formatting, escaping, and data type handling.

## Syntax

### JSON Script Format
```json
{
  "path": "$.target.path",
  "command": "toCsv",
  "csvSettings": {
    "delimiter": ",",
    "includeHeaders": true,
    "includeTypeColumns": false,
    "quoteAllFields": false,
    "nullValueRepresentation": "",
    "booleanFormat": "true,false"
  }
}
```

### Required Properties
- **path**: JSONPath expression targeting objects or arrays to convert
- **command**: Must be "toCsv"
- **csvSettings**: Configuration object for CSV formatting

## CSV Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `delimiter` | string | "," | Field delimiter (comma, semicolon, tab, etc.) |
| `includeHeaders` | boolean | true | Whether to include column headers |
| `includeTypeColumns` | boolean | false | Whether to include type information columns |
| `typeColumnSuffix` | string | "_type" | Suffix for type columns |
| `quoteAllFields` | boolean | false | Whether to quote all fields regardless of content |
| `escapeQuoteChar` | string | "\"" | Character used for quoting fields |
| `lineEnding` | string | "\r\n" | Line ending format (CRLF, LF, etc.) |
| `encoding` | string | "UTF-8" | Character encoding |
| `includeMetadata` | boolean | false | Whether to include metadata columns |
| `nullValueRepresentation` | string | "" | How to represent null values |
| `booleanFormat` | string | "true,false" | Format for boolean values (true,false or YES,NO, etc.) |

## Examples

### Basic CSV Generation

**Input:**
```json
{
  "companies": [
    {
      "id": 1,
      "name": "TechCorp",
      "address.street": "123 Tech Ave",
      "address.city": "Silicon Valley",
      "address.country": "USA",
      "departments.0.name": "Engineering",
      "departments.0.employees.0.name": "John Doe",
      "departments.0.employees.0.skills.0": "JavaScript"
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.companies[*]",
  "command": "toCsv",
  "csvSettings": {
    "delimiter": ",",
    "includeHeaders": true,
    "includeTypeColumns": false
  }
}]
```

**Output:**
```csv
address.city,address.country,address.street,departments.0.employees.0.name,departments.0.employees.0.skills.0,departments.0.name,id,name
Silicon Valley,USA,123 Tech Ave,John Doe,JavaScript,Engineering,1,TechCorp
```

### Handling Special Characters and Escaping

**Input:**
```json
{
  "products": [
    {
      "id": 1,
      "name": "Laptop with \"special\" features",
      "description": "A laptop\nwith multiple lines\rand commas, quotes",
      "price": 1299.99,
      "inStock": true,
      "category": null
    },
    {
      "id": 2,
      "name": "Mouse",
      "description": "Simple mouse",
      "price": 29.99,
      "inStock": false,
      "category": "Accessories"
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.products",
  "command": "toCsv",
  "csvSettings": {
    "delimiter": ",",
    "includeHeaders": true,
    "quoteAllFields": false,
    "nullValueRepresentation": "NULL",
    "booleanFormat": "TRUE,FALSE"
  }
}]
```

**Output:**
```csv
category,description,id,inStock,name,price
NULL,"A laptop
with multiple lines
and commas, quotes",1,TRUE,"Laptop with ""special"" features",1299.99
Accessories,Simple mouse,2,FALSE,Mouse,29.99
```

### Single Object Conversion

**Input:**
```json
{
  "user": {
    "id": 123,
    "name": "John Doe",
    "email": "john@example.com",
    "active": true
  }
}
```

**Script:**
```json
[{
  "path": "$.user",
  "command": "toCsv",
  "csvSettings": {
    "delimiter": ",",
    "includeHeaders": true
  }
}]
```

**Output:**
```csv
active,email,id,name
true,john@example.com,123,John Doe
```

### Including Type Information and Metadata

**Input:**
```json
{
  "report": {
    "data.field1": "value1",
    "data.field1_type": "String",
    "data.field2": 42,
    "data.field2_type": "Integer",
    "_metadata": {
      "generated": "2024-01-01"
    },
    "_timestamp": "2024-01-01T10:00:00Z"
  }
}
```

**Script:**
```json
[{
  "path": "$.report",
  "command": "toCsv",
  "csvSettings": {
    "delimiter": ",",
    "includeHeaders": true,
    "includeTypeColumns": true,
    "includeMetadata": true
  }
}]
```

**Output:**
```csv
_metadata,_timestamp,data.field1,data.field1_type,data.field2,data.field2_type
"{""generated"":""2024-01-01""}",2024-01-01T10:00:00Z,value1,String,42,Integer
```

### Advanced Configuration

**Script with Custom Settings:**
```json
[{
  "path": "$.data",
  "command": "toCsv",
  "csvSettings": {
    "delimiter": ";",
    "quoteAllFields": true,
    "nullValueRepresentation": "NULL",
    "booleanFormat": "YES,NO",
    "lineEnding": "\n",
    "escapeQuoteChar": "\"",
    "includeTypeColumns": true,
    "includeMetadata": false
  }
}]
```

This configuration:
- Uses semicolon as field delimiter
- Quotes all fields
- Represents nulls as "NULL"
- Uses YES/NO for booleans
- Uses LF line endings
- Includes type columns but excludes metadata

## Integration with Flatten Command

The ToCsv command works exceptionally well with flattened data:

**Complete Pipeline:**
```json
[
  {
    "path": "$.products[*]",
    "command": "flatten",
    "flattenSettings": {
      "delimiter": ".",
      "includeArrayIndices": false,
      "preserveTypes": false,
      "metadataPath": "$",
      "metadataKey": "_metadata"
    }
  },
  {
    "path": "$.products[*]",
    "command": "toCsv",
    "csvSettings": {
      "delimiter": ",",
      "includeHeaders": true,
      "includeTypeColumns": false,
      "includeMetadata": false
    }
  }
]
```

**Original Nested JSON:**
```json
{
  "products": [
    {
      "id": 1,
      "info": {
        "name": "Laptop",
        "specs": {
          "cpu": "Intel i7",
          "ram": "16GB"
        }
      },
      "tags": ["electronics", "computers"]
    }
  ]
}
```

**Final CSV Output:**
```csv
id,info.name,info.specs.cpu,info.specs.ram,tags.0,tags.1
1,Laptop,Intel i7,16GB,electronics,computers
```

## CSV Standards Compliance

### RFC 4180 Features

1. **Field Separation**: Configurable delimiter (comma, semicolon, tab, etc.)
2. **Line Termination**: Configurable line endings (CRLF, LF)
3. **Quote Handling**: Automatic quoting when fields contain:
   - Delimiter characters
   - Quote characters
   - Line breaks
   - Leading/trailing spaces
4. **Quote Escaping**: Doubles quote characters (`"` becomes `""`)
5. **Header Row**: Optional column headers
6. **UTF-8 Encoding**: Full Unicode support

### Data Type Handling

| JSON Type | CSV Representation |
|-----------|-------------------|
| String | Direct value (quoted if necessary) |
| Number | Invariant culture formatting (no locale-specific decimals) |
| Boolean | Configurable format (true/false, YES/NO, 1/0) |
| Null | Configurable representation (empty, NULL, N/A) |
| Object | JSON serialization (quoted) |
| Array | JSON serialization (quoted) |
| Date | ISO format (yyyy-MM-ddTHH:mm:ssZ) |

## Use Cases

### 1. Data Export
Export JSON data to CSV for Excel, database import, or analysis tools:
```json
[
  { "path": "$.records[*]", "command": "flatten" },
  { "path": "$.records[*]", "command": "toCsv" }
]
```

### 2. Report Generation
Create CSV reports from complex JSON structures:
```json
[
  { "path": "$.analytics.users[*]", "command": "flatten" },
  { "path": "$.analytics.users[*]", "command": "toCsv", "csvSettings": { "includeHeaders": true } }
]
```

### 3. Database Import Preparation
Prepare JSON data for database bulk import:
```json
[
  { "path": "$.entities[*]", "command": "flatten" },
  { "path": "$.entities[*]", "command": "toCsv", "csvSettings": { "delimiter": "|", "nullValueRepresentation": "\\N" } }
]
```

### 4. API Response Conversion
Convert API responses to CSV format:
```json
[
  { "path": "$.apiResponse.data[*]", "command": "toCsv" }
]
```

## Programmatic Usage

### Constructor with Settings
```csharp
var toCsvCommand = new ToCsv
{
    Path = "$.data[*]",
    CsvSettings = new CsvSettings
    {
        Delimiter = ",",
        IncludeHeaders = true,
        IncludeTypeColumns = false,
        QuoteAllFields = false,
        NullValueRepresentation = "NULL",
        BooleanFormat = "YES,NO"
    }
};
```

### Validation

The command validates:
- Path property is not empty
- CsvSettings is not null
- Delimiter is not empty
- EscapeQuoteChar is not empty
- BooleanFormat contains comma-separated values

## Performance Considerations

- **Large Datasets**: Memory usage scales with dataset size
- **Complex Objects**: Nested objects require JSON serialization
- **Column Count**: Many columns can impact processing time
- **Quoting Strategy**: `quoteAllFields: true` adds processing overhead
- **Type Processing**: Including type columns doubles memory usage

## Error Handling

### Invalid Targets
Warnings are logged for unsupported data types:
```csharp
context.LogWarning("ToCsv command can only be applied to objects or arrays of objects");
```

### Validation Failures
Multiple validation errors are collected and reported:
```csharp
result.ValidationMessages.Add("Delimiter cannot be empty in CsvSettings");
result.ValidationMessages.Add("BooleanFormat must contain comma-separated true,false values");
```

## Best Practices

1. **Use with Flatten**: Combine with flatten for complex nested data
2. **Test with Sample Data**: Verify output format with representative data
3. **Configure Appropriately**: Set proper delimiters for your target system
4. **Handle Special Characters**: Test with data containing quotes, commas, newlines
5. **Consider Encoding**: Ensure UTF-8 compatibility for international characters
6. **Monitor Memory Usage**: Large datasets may require memory optimization
7. **Validate Configuration**: Always validate CSV settings before processing

## Limitations

- **Memory Constraints**: Large datasets are processed in memory
- **Column Ordering**: Columns are ordered alphabetically by key name
- **Complex Nesting**: Very deep objects may produce large serialized strings

## Related Commands

- [Flatten](flatten.md) - Prepare nested data for CSV conversion
- [Restore](restore.md) - Reconstruct original structure from flattened data
- [Resolve](resolve.md) - Cross-reference data before CSV export