# Fetch Function Documentation

## Overview

The `Fetch` function retrieves values from JSON data using JSONPath expressions. It's essential for accessing nested data, referencing other parts of the document, and extracting values for use in transformations and calculations.

## Syntax

### Function Expression Format
```json
// Basic fetch from JSONPath
"=fetch($.user.name)"

// Fetch from current context
"=fetch(@.localProperty)"

// Fetch array elements
"=fetch($.items[0])"

// Fetch with filters
"=fetch($.users[?(@.active == true)])"
```

### Programmatic Usage
```csharp
// With JSONPath argument
var fetchFunction = new Fetch("$.user.name");

// Empty constructor for dynamic arguments
var fetchFunction = new Fetch();
```

### Builder Pattern
```csharp
var fetchFunction = FetchBuilders.Fetch("$.data.value");
```

## Parameters

- **Path (First Argument)**: JSONPath expression specifying what to fetch
- **Return Behavior**: Returns the first matching token or null if not found
- **Context**: Evaluates paths relative to the data context and current token

## Examples

### Basic Value Fetching
```json
{
  "path": "$.displayValue",
  "value": "=fetch($.user.name)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {
    "name": "John Doe",
    "email": "john@example.com"
  }
}
```

**Result**:
```json
{
  "user": {
    "name": "John Doe",
    "email": "john@example.com"
  },
  "displayValue": "John Doe"
}
```

### Fetching Nested Objects
```json
{
  "path": "$.userProfile",
  "value": "=fetch($.user)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {
    "id": 123,
    "name": "John Doe",
    "preferences": {
      "theme": "dark",
      "language": "en"
    }
  }
}
```

**Result**:
```json
{
  "user": {
    "id": 123,
    "name": "John Doe",
    "preferences": {
      "theme": "dark",
      "language": "en"
    }
  },
  "userProfile": {
    "id": 123,
    "name": "John Doe",
    "preferences": {
      "theme": "dark",
      "language": "en"
    }
  }
}
```

### Array Element Fetching
```json
{
  "path": "$.firstItem",
  "value": "=fetch($.items[0])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "items": [
    {"id": 1, "name": "First"},
    {"id": 2, "name": "Second"}
  ]
}
```

**Result**:
```json
{
  "items": [
    {"id": 1, "name": "First"},
    {"id": 2, "name": "Second"}
  ],
  "firstItem": {"id": 1, "name": "First"}
}
```

### Fetching with Filters
```json
{
  "path": "$.activeUser",
  "value": "=fetch($.users[?(@.active == true)])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"id": 1, "name": "John", "active": false},
    {"id": 2, "name": "Jane", "active": true}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"id": 1, "name": "John", "active": false},
    {"id": 2, "name": "Jane", "active": true}
  ],
  "activeUser": {"id": 2, "name": "Jane", "active": true}
}
```

### Cross-Reference Data
```json
{
  "path": "$.order.customerName",
  "value": "=fetch($.customers[?(@.id == $.order.customerId)].name)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "customers": [
    {"id": 1, "name": "Alice"},
    {"id": 2, "name": "Bob"}
  ],
  "order": {
    "id": 100,
    "customerId": 2,
    "amount": 250
  }
}
```

**Result**:
```json
{
  "customers": [
    {"id": 1, "name": "Alice"},
    {"id": 2, "name": "Bob"}
  ],
  "order": {
    "id": 100,
    "customerId": 2,
    "amount": 250,
    "customerName": "Bob"
  }
}
```

### Fetch from Current Context
```json
{
  "path": "$.summary",
  "value": "=concat('Item: ', fetch(@.name), ' - Price: ', fetch(@.price))",
  "command": "add"
}
```

**Applied to each item in an array**:
```json
{
  "name": "Product A",
  "price": 29.99,
  "summary": "Item: Product A - Price: 29.99"
}
```

## Advanced Usage

### Multi-level Fetching
```json
{
  "path": "$.userTheme",
  "value": "=fetch($.user.preferences.theme)",
  "command": "add"
}
```

### Combining with Other Functions
```json
{
  "path": "$.userSummary",
  "value": "=concat('User: ', fetch($.user.name), ' (ID: ', toString(fetch($.user.id)), ')')",
  "command": "add"
}
```

### Conditional Fetching
```json
{
  "path": "$.primaryContact",
  "value": "=fetch($.contacts[?(@.isPrimary == true)])",
  "command": "add"
}
```

### Recursive Descent
```json
{
  "path": "$.allNames",
  "value": "=fetch($..name)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {"name": "John"},
  "company": {"name": "ACME Corp"},
  "project": {"name": "Project X"}
}
```

**Result**: Returns the first name found at any level

## Fluent API Usage

### Basic Fetch
```csharp
var script = new JLioScript()
    .Add(FetchBuilders.Fetch("$.user.email"))
    .OnPath("$.contactEmail");
```

### Complex Data Retrieval
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat(
        "Welcome ",
        "=fetch($.user.name)",
        "! Last login: ",
        "=fetch($.user.lastLogin)"
    ))
    .OnPath("$.welcomeMessage");
```

### Array Processing
```csharp
var script = new JLioScript()
    .Add(FetchBuilders.Fetch("$.orders[?(@.status == 'pending')]"))
    .OnPath("$.pendingOrder")
    .Add(FetchBuilders.Fetch("$.orders[?(@.status == 'completed')].length"))
    .OnPath("$.completedCount");
```

## Integration Examples

### User Profile Enrichment
```csharp
var script = new JLioScript()
    .Add(FetchBuilders.Fetch("$.users[?(@.id == $.currentUserId)]"))
    .OnPath("$.currentUser")
    .Add(FetchBuilders.Fetch("$.currentUser.preferences.language"))
    .OnPath("$.userLanguage")
    .Add(FetchBuilders.Fetch("$.currentUser.profile.avatar"))
    .OnPath("$.userAvatar");
```

### Order Processing
```csharp
var script = new JLioScript()
    .Add(FetchBuilders.Fetch("$.products[?(@.id == $.order.productId)]"))
    .OnPath("$.order.product")
    .Add(FetchBuilders.Fetch("$.order.product.price"))
    .OnPath("$.order.unitPrice")
    .Add(ConcatBuilders.Concat(
        "=fetch($.order.quantity)",
        " x ",
        "=fetch($.order.product.name)"
    ))
    .OnPath("$.order.description");
```

### Configuration Resolution
```csharp
var script = new JLioScript()
    .Add(FetchBuilders.Fetch("$.config.environments[?(@.name == $.targetEnvironment)]"))
    .OnPath("$.activeConfig")
    .Add(FetchBuilders.Fetch("$.activeConfig.database.connectionString"))
    .OnPath("$.dbConnection")
    .Add(FetchBuilders.Fetch("$.activeConfig.api.baseUrl"))
    .OnPath("$.apiEndpoint");
```

## Return Value Handling

### Successful Fetch
```json
// Returns the actual value found
"=fetch($.user.name)"  // Returns: "John Doe"
```

### No Match Found
```json
// Returns null when path doesn't exist
"=fetch($.nonExistent.path)"  // Returns: null
```

### Multiple Matches
```json
// Returns first match when multiple items found
"=fetch($.users[*].name)"  // Returns: First user's name only
```

### Array Results
```json
// When fetching arrays, returns the array object
"=fetch($.items)"  // Returns: [item1, item2, item3]
```

## Error Handling

### Invalid JSONPath
If the JSONPath expression is malformed, the function returns null and may log warnings.

### Missing Context
If the required data context is not available, the function returns null.

### Type Mismatches
The function returns the actual value type found, which may be string, number, object, array, boolean, or null.

## Performance Considerations

- **JSONPath Complexity**: Complex paths with filters require more processing
- **Recursive Descent**: `$..` patterns can be expensive on large documents
- **Multiple Calls**: Frequent fetch operations may impact performance
- **Data Size**: Large objects/arrays take more time to traverse

## Best Practices

1. **Use Specific Paths**: Prefer specific paths over recursive descent when possible
2. **Cache Results**: Store fetched values in variables if used multiple times
3. **Validate Paths**: Ensure JSONPath expressions are correct and efficient
4. **Handle Nulls**: Always consider null return values in your logic
5. **Test Edge Cases**: Verify behavior with missing data and empty arrays
6. **Document Dependencies**: Clearly document what data the fetch depends on
7. **Avoid Deep Nesting**: Minimize the depth of JSONPath expressions

## Common Patterns

### Property Copy Pattern
```json
"=fetch($.source.property)"
```

### Array First Element Pattern
```json
"=fetch($.items[0])"
```

### Filtered Search Pattern
```json
"=fetch($.collection[?(@.criteria == 'value')])"
```

### Cross-Reference Pattern
```json
"=fetch($.lookup[?(@.id == $.current.lookupId)])"
```

### Default Value Pattern
```json
"=fetch($.optional.property) || 'default'"
```

## JSONPath Expression Examples

### Basic Paths
```json
"$.user"                    // Root level object
"$.user.name"              // Nested property
"$.items[0]"               // First array element
"$.items[-1]"              // Last array element
```

### Array Operations
```json
"$.items[*]"               // All array elements
"$.items[1:3]"             // Array slice
"$.items.length"           // Array length
```

### Filters
```json
"$.users[?(@.age > 18)]"           // Age filter
"$.products[?(@.price < 100)]"     // Price filter
"$.items[?(@.category == 'tech')]" // Category filter
```

### Recursive Descent
```json
"$..name"                  // All 'name' properties at any level
"$..items[*]"             // All 'items' arrays at any level
```