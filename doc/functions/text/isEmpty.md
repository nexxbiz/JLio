# IsEmpty Function Documentation

## Overview

The `IsEmpty` function checks whether a value is empty, supporting various data types including strings, arrays, objects, and null values. It's essential for data validation, conditional logic, and quality assurance workflows.

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
// Check if string is empty
"=isEmpty($.text)"

// Check if array is empty
"=isEmpty($.items)"

// Check if object is empty
"=isEmpty($.data)"

// Check literal values
"=isEmpty('')"
```

### Programmatic Usage
```csharp
// Check value
var isEmptyFunction = new IsEmpty("value");

// Empty constructor for dynamic arguments
var isEmptyFunction = new IsEmpty();
```

### Builder Pattern
```csharp
var isEmptyFunction = IsEmptyBuilders.IsEmpty("$.field");
var arrayCheck = IsEmptyBuilders.IsEmpty("$.items[*]");
```

## Parameters

- **value** (required): The value to check for emptiness
  - **Type**: Any value, JSONPath expression
  - **Supported Types**: String, Array, Object, Null, and other primitives

## Return Value

- **Type**: Boolean (JValue)
- **Value**: true if empty, false if not empty

## Emptiness Rules

### String Values
- **Empty**: `""` (empty string)
- **Not Empty**: Any string with content, including whitespace

### Arrays
- **Empty**: `[]` (no elements)
- **Not Empty**: Arrays with any elements, including null elements

### Objects  
- **Empty**: `{}` (no properties)
- **Not Empty**: Objects with any properties, including null values

### Null Values
- **Empty**: `null` values are considered empty
- **Not Empty**: All other primitive values (including `false`, `0`)

### Other Types
- **Empty**: Only null values
- **Not Empty**: All other primitive types (numbers, booleans)

## Examples

### String Emptiness Check
```json
{
  "path": "$.validation.nameEmpty",
  "value": "=isEmpty($.userName)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "userName": ""
}
```

**Result**:
```json
{
  "userName": "",
  "validation": {
    "nameEmpty": true
  }
}
```

### Array Emptiness Check
```json
{
  "path": "$.hasItems",
  "value": "=!isEmpty($.items)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "items": []
}
```

**Result**:
```json
{
  "items": [],
  "hasItems": false
}
```

### Object Emptiness Check
```json
{
  "path": "$.hasConfig",
  "value": "=!isEmpty($.configuration)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "configuration": {}
}
```

**Result**:
```json
{
  "configuration": {},
  "hasConfig": false
}
```

### Form Validation
```json
{
  "path": "$.validation",
  "value": {
    "firstNameRequired": "=isEmpty($.firstName)",
    "lastNameRequired": "=isEmpty($.lastName)", 
    "emailRequired": "=isEmpty($.email)",
    "phoneRequired": "=isEmpty($.phone)"
  },
  "command": "add"
}
```

**Input Data**:
```json
{
  "firstName": "John",
  "lastName": "",
  "email": "john@example.com",
  "phone": null
}
```

**Result**:
```json
{
  "firstName": "John",
  "lastName": "",
  "email": "john@example.com", 
  "phone": null,
  "validation": {
    "firstNameRequired": false,
    "lastNameRequired": true,
    "lastNameRequired": true,
    "phoneRequired": true
  }
}
```

### Conditional Processing
```json
{
  "path": "$.message",
  "value": "=if(isEmpty($.title), 'No title provided', $.title)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "title": ""
}
```

**Result**:
```json
{
  "title": "",
  "message": "No title provided"
}
```

### Array Processing with Validation
```json
{
  "path": "$.users[*].isValid",
  "value": "=!isEmpty(@.name) && !isEmpty(@.email)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "Alice", "email": "alice@example.com"},
    {"name": "", "email": "bob@example.com"},
    {"name": "Charlie", "email": ""}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"name": "Alice", "email": "alice@example.com", "isValid": true},
    {"name": "", "email": "bob@example.com", "isValid": false},
    {"name": "Charlie", "email": "", "isValid": false}
  ]
}
```

### Data Quality Assessment
```json
[
  {
    "path": "$.quality.emptyFields",
    "value": "=sum(if(isEmpty($.name), 1, 0), if(isEmpty($.email), 1, 0), if(isEmpty($.phone), 1, 0))",
    "command": "add"
  },
  {
    "path": "$.quality.completeness",
    "value": "=round((3 - $.quality.emptyFields) / 3 * 100, 1)",
    "command": "add"
  }
]
```

### Missing Data Report
```json
{
  "path": "$.report.missingData",
  "value": {
    "missingNames": "=count($.users[?isEmpty(@.name)])",
    "missingEmails": "=count($.users[?isEmpty(@.email)])",
    "emptyProfiles": "=count($.users[?isEmpty(@.profile)])"
  },
  "command": "add"
}
```

## Advanced Usage

### Multi-field Validation
```csharp
var script = new JLioScript()
    .Add(IsEmptyBuilders.IsEmpty("@.firstName"))
    .OnPath("$.users[*].validation.firstNameEmpty")
    .Add(IsEmptyBuilders.IsEmpty("@.lastName"))
    .OnPath("$.users[*].validation.lastNameEmpty")
    .Add(IsEmptyBuilders.IsEmpty("@.email"))
    .OnPath("$.users[*].validation.emailEmpty");
```

### Conditional Data Processing
```csharp
var script = new JLioScript()
    .Add("=if(isEmpty($.optionalField), 'N/A', $.optionalField)")
    .OnPath("$.processedField")
    .Add("=if(isEmpty($.items), [], $.items)")
    .OnPath("$.safeItems");
```

### Data Completeness Scoring
```json
{
  "path": "$.completenessScore",
  "value": "=avg(!isEmpty($.field1), !isEmpty($.field2), !isEmpty($.field3), !isEmpty($.field4)) * 100",
  "command": "add"
}
```

## Data Type Examples

### String Tests
```json
"=isEmpty('')"           // true (empty string)
"=isEmpty(' ')"          // false (whitespace string)
"=isEmpty('hello')"      // false (content string)
"=isEmpty('null')"       // false (string containing "null")
```

### Array Tests
```json
"=isEmpty([])"           // true (empty array)
"=isEmpty([null])"       // false (array with null element)
"=isEmpty([''])"         // false (array with empty string)
"=isEmpty([1,2,3])"      // false (array with elements)
```

### Object Tests
```json
"=isEmpty({})"           // true (empty object)
"=isEmpty({\"a\": null})" // false (object with null property)
"=isEmpty({\"a\": \"\"})" // false (object with empty string property)
```

### Primitive Tests
```json
"=isEmpty(null)"         // true (null value)
"=isEmpty(false)"        // false (boolean false)
"=isEmpty(0)"            // false (number zero)
"=isEmpty(undefined)"    // true (undefined/missing)
```

## Fluent API Usage

### Basic Emptiness Check
```csharp
var script = new JLioScript()
    .Add(IsEmptyBuilders.IsEmpty("$.requiredField"))
    .OnPath("$.validation.fieldMissing");
```

### Multiple Validation Checks
```csharp
var script = new JLioScript()
    .Add("=!isEmpty($.name)")
    .OnPath("$.validation.hasName")
    .Add("=!isEmpty($.email)")
    .OnPath("$.validation.hasEmail")
    .Add("=!isEmpty($.phone)")
    .OnPath("$.validation.hasPhone");
```

### Conditional Default Values
```csharp
var script = new JLioScript()
    .Add("=if(isEmpty($.title), 'Untitled', $.title)")
    .OnPath("$.displayTitle")
    .Add("=if(isEmpty($.description), 'No description', $.description)")
    .OnPath("$.displayDescription");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=isEmpty()"  // Logs error: "IsEmpty requires exactly one argument"

// Too many arguments
"=isEmpty($.field, 'extra')"  // Logs error: "IsEmpty requires exactly one argument"
```

## Performance Considerations

- **String Check**: Fast string length comparison
- **Array Check**: Fast array length comparison  
- **Object Check**: Property enumeration may be slower for large objects
- **Memory Usage**: Minimal overhead for emptiness checking

## Best Practices

1. **Validation Strategy**: Use isEmpty for comprehensive data validation
2. **Default Values**: Combine with conditional logic for default value assignment
3. **Data Quality**: Use for data completeness assessment
4. **Performance**: Consider object property count for large objects
5. **Type Awareness**: Understand emptiness rules for different data types
6. **Testing**: Test with various data scenarios including edge cases

## Common Patterns

### Required Field Validation Pattern
```json
"=isEmpty($.requiredField)"
```

### Safe Default Value Pattern
```json
"=if(isEmpty($.optionalField), 'default', $.optionalField)"
```

### Data Completeness Pattern
```json
"=!isEmpty($.field1) && !isEmpty($.field2) && !isEmpty($.field3)"
```

### Filtering Pattern
```json
"$.items[?!isEmpty(@.importantField)]"
```

### Quality Score Pattern
```json
"=avg(!isEmpty($.field1), !isEmpty($.field2), !isEmpty($.field3)) * 100"
```

## Integration Examples

### With Conditional Logic
```json
"=if(isEmpty($.data), 'No data available', concat('Found ', length($.data), ' items'))"
```

### With String Functions
```json
"=isEmpty(trim($.userInput))"  // Check if trimmed input is empty
```

### With Array Functions
```json
"=if(isEmpty($.tags), [], split($.tagString, ','))"
```

### With Math Functions
```json
"=sum(if(isEmpty(@.value), 0, @.value))"  // Sum with empty value handling
```

## Use Cases Summary

### Data Validation
- Required field checking
- Form validation
- Input sanitization
- Data quality assessment

### Conditional Processing
- Default value assignment
- Optional field handling
- Branching logic
- Error prevention

### Reporting
- Data completeness metrics
- Missing data identification
- Quality scoring
- Validation summaries

### Data Filtering
- Remove empty records
- Find incomplete data
- Quality-based filtering
- Required field enforcement