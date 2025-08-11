# Join Function Documentation

## Overview

The `Join` function combines array elements or multiple values into a single string using a specified separator. It's the counterpart to the split function and essential for creating formatted output, CSV generation, and string concatenation workflows.

## Installation

### Extension Pack Registration
```csharp
// Register text functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterText();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Formats
```json
// Join array elements with separator
"=join(',', $.tags)"

// Join multiple values
"=join('-', 'year', 'month', 'day')"

// Join with space separator
"=join(' ', $.firstName, $.lastName)"

// Join with empty separator (concatenate)
"=join('', $.parts)"
```

### Programmatic Usage
```csharp
// Join array with separator
var joinFunction = new Join("separator", "array");

// Join multiple values
var joinFunction = new Join("separator", "value1", "value2", "value3");

// Empty constructor for dynamic arguments
var joinFunction = new Join();
```

### Builder Pattern
```csharp
var joinFunction = JoinBuilders.Join(",", "$.tags");
var multiValue = JoinBuilders.Join(" ", "$.first", "$.middle", "$.last");
```

## Parameters

- **separator** (required): The string to use between elements
  - **Type**: String, JSONPath expression, or any value
  - **Empty Allowed**: Empty string creates concatenation without separators
- **array** (when 2nd argument is array): Array to join
  - **Type**: JArray or JSONPath expression that resolves to array
  - **Element Conversion**: All elements converted to strings
- **values** (when multiple arguments): Individual values to join
  - **Type**: String, JSONPath expression, or any value
  - **Minimum**: At least one value after separator

## Return Value

- **Type**: String (JValue)
- **Value**: Combined string with separators between elements

## Examples

### Basic Array Joining
```json
{
  "path": "$.tagString",
  "value": "=join(',', $.tags)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "tags": ["javascript", "web", "frontend", "react"]
}
```

**Result**:
```json
{
  "tags": ["javascript", "web", "frontend", "react"],
  "tagString": "javascript,web,frontend,react"
}
```

### Multiple Value Joining
```json
{
  "path": "$.fullName",
  "value": "=join(' ', $.firstName, $.middleName, $.lastName)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "firstName": "John",
  "middleName": "Michael",
  "lastName": "Doe"
}
```

**Result**:
```json
{
  "firstName": "John",
  "middleName": "Michael",
  "lastName": "Doe",
  "fullName": "John Michael Doe"
}
```

### CSV Generation
```json
{
  "path": "$.csvRow",
  "value": "=join(',', $.name, $.age, $.city)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "name": "Alice",
  "age": 30,
  "city": "New York"
}
```

**Result**:
```json
{
  "name": "Alice",
  "age": 30,
  "city": "New York",
  "csvRow": "Alice,30,New York"
}
```

### Path Construction
```json
{
  "path": "$.filePath",
  "value": "=join('/', $.directory, $.filename)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "directory": "documents",
  "filename": "report.pdf"
}
```

**Result**:
```json
{
  "directory": "documents",
  "filename": "report.pdf",
  "filePath": "documents/report.pdf"
}
```

### Array Processing
```json
{
  "path": "$.users[*].skillsText",
  "value": "=join(', ', @.skills)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "Alice", "skills": ["Java", "Python", "SQL"]},
    {"name": "Bob", "skills": ["JavaScript", "React", "Node.js"]}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"name": "Alice", "skills": ["Java", "Python", "SQL"], "skillsText": "Java, Python, SQL"},
    {"name": "Bob", "skills": ["JavaScript", "React", "Node.js"], "skillsText": "JavaScript, React, Node.js"}
  ]
}
```

### Email Generation
```json
{
  "path": "$.email",
  "value": "=join('@', $.username, $.domain)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "username": "john.doe",
  "domain": "company.com"
}
```

**Result**:
```json
{
  "username": "john.doe",
  "domain": "company.com",
  "email": "john.doe@company.com"
}
```

### URL Construction
```json
{
  "path": "$.apiUrl",
  "value": "=join('/', $.baseUrl, 'api', $.version, $.endpoint)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "baseUrl": "https://api.example.com",
  "version": "v1",
  "endpoint": "users"
}
```

**Result**:
```json
{
  "baseUrl": "https://api.example.com",
  "version": "v1",
  "endpoint": "users",
  "apiUrl": "https://api.example.com/api/v1/users"
}
```

### Concatenation Without Separator
```json
{
  "path": "$.code",
  "value": "=join('', $.prefix, $.number, $.suffix)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "prefix": "CODE",
  "number": 12345,
  "suffix": "X"
}
```

**Result**:
```json
{
  "prefix": "CODE",
  "number": 12345,
  "suffix": "X",
  "code": "CODE12345X"
}
```

## Advanced Usage

### Dynamic Separator
```csharp
var script = new JLioScript()
    .Add(JoinBuilders.Join("@.separator", "@.values"))
    .OnPath("$.data[*].joined");
```

### Conditional Joining
```json
{
  "path": "$.address",
  "value": "=join(', ', $.street, if(isEmpty($.apartment), '', $.apartment), $.city, $.state)",
  "command": "add"
}
```

### Multi-level Processing
```csharp
var script = new JLioScript()
    .Add(JoinBuilders.Join(" ", "@.firstName", "@.lastName"))
    .OnPath("$.team[*].fullName")
    .Add(JoinBuilders.Join(", ", "$.team[*].fullName"))
    .OnPath("$.teamRoster");
```

### Report Generation
```json
[
  {
    "path": "$.header",
    "value": "=join('\t', 'Name', 'Age', 'Department', 'Salary')",
    "command": "add"
  },
  {
    "path": "$.employees[*].row",
    "value": "=join('\t', @.name, @.age, @.department, @.salary)",
    "command": "add"
  }
]
```

## Data Type Handling

### Array Elements
```json
"=join(',', [1, 2, 3])"  // Result: "1,2,3" (numbers converted to strings)
```

### Mixed Types
```json
"=join('-', 'prefix', 123, true, 'suffix')"  // Result: "prefix-123-true-suffix"
```

### Null and Empty Values
```json
"=join(',', ['a', null, 'b', '', 'c'])"  // Result: "a,,b,,c" (null becomes empty)
```

### Single Element
```json
"=join(',', ['single'])"  // Result: "single" (no separator needed)
```

## Fluent API Usage

### Basic Array Join
```csharp
var script = new JLioScript()
    .Add(JoinBuilders.Join(",", "$.categories"))
    .OnPath("$.categoryList");
```

### Multiple Value Join
```csharp
var script = new JLioScript()
    .Add(JoinBuilders.Join(" - ", "$.title", "$.subtitle"))
    .OnPath("$.heading");
```

### Bulk Processing
```csharp
var script = new JLioScript()
    .Add(JoinBuilders.Join(", ", "@.tags"))
    .OnPath("$.posts[*].tagString")
    .Add(JoinBuilders.Join(" | ", "@.categories"))
    .OnPath("$.posts[*].categoryString");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=join(',')"  // Logs error: "Join requires at least 2 arguments"

// Single argument
"=join('')"  // Logs error: "Join requires at least 2 arguments"
```

### Empty Array Handling
```json
"=join(',', [])"  // Result: "" (empty string for empty array)
```

## Performance Considerations

- **Array Size**: Performance scales with number of elements to join
- **Element Conversion**: Converting non-strings to strings adds overhead
- **Memory Usage**: Creates new string objects for results
- **Separator Length**: Longer separators slightly impact performance

## Best Practices

1. **Separator Choice**: Choose appropriate separators for your output format
2. **Null Handling**: Be aware of how null values are handled (become empty strings)
3. **Type Conversion**: Remember that all values are converted to strings
4. **Empty Arrays**: Handle empty arrays gracefully (result is empty string)
5. **Performance**: Consider memory usage with large arrays
6. **Validation**: Validate array contents when joining user data

## Common Patterns

### CSV Generation Pattern
```json
"=join(',', $.fields)"
```

### Full Name Pattern
```json
"=join(' ', $.firstName, $.lastName)"
```

### Path Construction Pattern
```json
"=join('/', $.pathComponents)"
```

### Tag String Pattern
```json
"=join(', ', $.tags)"
```

### URL Building Pattern
```json
"=join('/', $.baseUrl, $.path, $.resource)"
```

## Integration Examples

### With Split Function
```json
"=join('-', split($.spaceText, ' '))"  // Convert spaces to dashes
```

### With Array Processing
```json
"=join(', ', $.items[*].name)"  // Join array of object names
```

### With Conditional Logic
```json
"=join(' ', $.title, if(isEmpty($.subtitle), '', concat('(', $.subtitle, ')')))"
```

### With Filtering
```json
"=join(', ', $.tags[?length(@) > 0])"  // Join non-empty tags only
```

## Comparison with Concat

| Function | Purpose | Input | Output |
|----------|---------|--------|--------|
| **Join** | Combine array/values with separator | Array or multiple values | Single string |
| **Concat** | Simple concatenation | Multiple values | Single string (no separator) |

### Usage Decision
- Use **Join** when you have an array or want separators between values
- Use **Concat** for simple string concatenation without separators