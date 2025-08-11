# StartsWith Function Documentation

## Overview

The `StartsWith` function checks whether a string starts with a specified prefix. It supports both case-sensitive and case-insensitive matching, making it essential for text validation, filtering, URL processing, and conditional logic workflows.

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
// Basic starts-with check (case-sensitive)
"=startsWith('Hello World', 'Hello')"

// Case-insensitive check
"=startsWith('Hello World', 'HELLO', true)"

// JSONPath source
"=startsWith($.url, 'https://')"
```

### Programmatic Usage
```csharp
// Case-sensitive check
var startsWithFunction = new StartsWith("text", "prefix");

// Case-insensitive check
var startsWithFunction = new StartsWith("text", "prefix", "true");

// Empty constructor for dynamic arguments
var startsWithFunction = new StartsWith();
```

### Builder Pattern
```csharp
var startsWithFunction = StartsWithBuilders.StartsWith("$.text", "prefix");
var caseInsensitive = StartsWithBuilders.StartsWith("$.text", "PREFIX", "true");
```

## Parameters

- **text** (required): The source string to check
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **prefix** (required): The prefix string to check for
  - **Type**: String, JSONPath expression, or any value
  - **Empty String**: Always returns true (empty string is prefix of any string)
- **ignoreCase** (optional): Whether to perform case-insensitive matching
  - **Type**: Boolean, string representation, or integer (0/1)
  - **Default**: false (case-sensitive)

## Return Value

- **Type**: Boolean (JValue)
- **Value**: true if string starts with prefix, false otherwise

## Examples

### Basic Prefix Check
```json
{
  "path": "$.startsWithHello",
  "value": "=startsWith('Hello World', 'Hello')",
  "command": "add"
}
```

**Result**:
```json
{
  "startsWithHello": true
}
```

### Case-Insensitive Check
```json
{
  "path": "$.startsWithHello",
  "value": "=startsWith('Hello World', 'HELLO', true)",
  "command": "add"
}
```

**Result**:
```json
{
  "startsWithHello": true
}
```

### URL Protocol Validation
```json
{
  "path": "$.isSecure",
  "value": "=startsWith($.url, 'https://')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "url": "https://www.example.com"
}
```

**Result**:
```json
{
  "url": "https://www.example.com",
  "isSecure": true
}
```

### Email Domain Check
```json
{
  "path": "$.isCompanyEmail",
  "value": "=startsWith($.email, '@company.com', false)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "email": "user@company.com"
}
```

**Result**:
```json
{
  "email": "user@company.com",
  "isCompanyEmail": false
}
```

### File Extension Validation
```json
{
  "path": "$.isImageFile",
  "value": "=startsWith(toLower($.filename), 'img_') || startsWith(toLower($.filename), 'photo_')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "IMG_1234.jpg"
}
```

**Result**:
```json
{
  "filename": "IMG_1234.jpg",
  "isImageFile": true
}
```

### Array Filtering with StartsWith
```json
{
  "path": "$.adminUsers",
  "value": "$.users[?startsWith(@.username, 'admin_')]",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"username": "admin_john", "role": "admin"},
    {"username": "user_jane", "role": "user"},
    {"username": "admin_bob", "role": "admin"}
  ]
}
```

**Result**:
```json
{
  "users": [...],
  "adminUsers": [
    {"username": "admin_john", "role": "admin"},
    {"username": "admin_bob", "role": "admin"}
  ]
}
```

### Command Processing
```json
{
  "path": "$.commandType",
  "value": "=if(startsWith($.input, '/'), 'command', 'message')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "input": "/help user"
}
```

**Result**:
```json
{
  "input": "/help user",
  "commandType": "command"
}
```

### Path Validation
```json
{
  "path": "$.isAbsolutePath",
  "value": "=startsWith($.path, '/') || startsWith($.path, 'C:')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "path": "/home/user/documents"
}
```

**Result**:
```json
{
  "path": "/home/user/documents",
  "isAbsolutePath": true
}
```

### Multiple Prefix Validation
```json
{
  "path": "$.validPrefix",
  "value": "=startsWith($.code, 'PRE_') || startsWith($.code, 'POST_') || startsWith($.code, 'MID_')",
  "command": "add"
}
```

## Advanced Usage

### Protocol Classification
```csharp
var script = new JLioScript()
    .Add(StartsWithBuilders.StartsWith("@.url", "https://"))
    .OnPath("$.urls[*].isSecure")
    .Add(StartsWithBuilders.StartsWith("@.url", "ftp://"))
    .OnPath("$.urls[*].isFtp")
    .Add(StartsWithBuilders.StartsWith("@.url", "mailto:"))
    .OnPath("$.urls[*].isEmail");
```

### File Type Detection
```csharp
var script = new JLioScript()
    .Add(StartsWithBuilders.StartsWith("@.filename", "temp_"))
    .OnPath("$.files[*].isTemporary")
    .Add(StartsWithBuilders.StartsWith("@.filename", "backup_"))
    .OnPath("$.files[*].isBackup")
    .Add(StartsWithBuilders.StartsWith("@.filename", "log_"))
    .OnPath("$.files[*].isLogFile");
```

### User Role Validation
```json
{
  "path": "$.permissions",
  "value": {
    "canAdmin": "=startsWith($.user.role, 'admin')",
    "canModerate": "=startsWith($.user.role, 'moderator')",
    "canEdit": "=startsWith($.user.role, 'editor')"
  },
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=startsWith('hello world', 'hello')"  // Result: true
"=startsWith('hello world', 'HELLO')"  // Result: false (case-sensitive)
"=startsWith('hello world', 'HELLO', true)"  // Result: true (case-insensitive)
```

### Number Conversion
```json
"=startsWith(12345, '123')"  // Result: true (from "12345")
```

### Empty String Checks
```json
"=startsWith('hello', '')"  // Result: true (empty string is prefix of any string)
"=startsWith('', 'hello')"  // Result: false (can't start with non-empty in empty string)
```

### URL Examples
```json
"=startsWith('https://example.com', 'https://')"  // Result: true
"=startsWith('http://example.com', 'https://')"   // Result: false
```

## Fluent API Usage

### Basic Prefix Check
```csharp
var script = new JLioScript()
    .Add(StartsWithBuilders.StartsWith("$.text", "prefix"))
    .OnPath("$.hasPrefix");
```

### Multiple Checks
```csharp
var script = new JLioScript()
    .Add(StartsWithBuilders.StartsWith("$.url", "https://"))
    .OnPath("$.isSecure")
    .Add(StartsWithBuilders.StartsWith("$.email", "admin@"))
    .OnPath("$.isAdminEmail");
```

### Conditional Processing
```csharp
var script = new JLioScript()
    .Add("=if(startsWith($.command, '/'), substring($.command, 1), $.command)")
    .OnPath("$.cleanCommand");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=startsWith('text')"  // Logs error: "StartsWith requires 2 or 3 arguments"

// Too many arguments
"=startsWith('text', 'prefix', true, 'extra')"  // Logs error: "StartsWith requires 2 or 3 arguments"
```

### Invalid Boolean Parameter
```json
// Invalid ignoreCase value
"=startsWith('text', 'prefix', 'invalid')"  // Logs error: "ignoreCase must be a valid boolean"
```

## Performance Considerations

- **String Length**: Performance depends on prefix length, not full string length
- **Case Sensitivity**: Case-insensitive operations may be slightly slower
- **Memory Usage**: Minimal additional memory overhead
- **Prefix Length**: Longer prefixes may impact performance slightly

## Best Practices

1. **URL Validation**: Use for protocol and domain validation
2. **File Processing**: Check file naming conventions and types
3. **Command Processing**: Identify command vs. content in user input
4. **Security**: Validate input format before further processing
5. **Performance**: Use case-sensitive matching when possible
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### URL Protocol Check Pattern
```json
"=startsWith($.url, 'https://')"
```

### Command Detection Pattern
```json
"=startsWith($.input, '/')"
```

### File Type Pattern
```json
"=startsWith(toLower($.filename), 'temp_')"
```

### Email Domain Pattern
```json
"=startsWith(substring($.email, indexOf($.email, '@') + 1), 'company.com')"
```

### Path Validation Pattern
```json
"=startsWith($.path, '/') || startsWith($.path, 'C:\\\\')"
```

## Integration Examples

### With Conditional Logic
```json
"=if(startsWith($.url, 'https://'), 'secure', 'insecure')"
```

### With String Functions
```json
"=startsWith(trim(toLower($.input)), 'admin')"
```

### With Array Operations
```json
"$.items[?startsWith(@.code, 'PROD_')]"
```

### With Validation
```json
"=startsWith($.email, '@') == false"  // Basic email format check
```

## Security Considerations

### Input Validation
```json
"=!startsWith($.userInput, '<script')"  // Basic XSS prevention
```

### Path Traversal Prevention
```json
"=!startsWith($.filePath, '../')"  // Prevent directory traversal
```

### Protocol Validation
```json
"=startsWith($.url, 'https://') || startsWith($.url, 'http://')"  // Allow only HTTP(S)
```

## Comparison with Other Functions

| Function | Purpose | Return Type | Use Case |
|----------|---------|-------------|----------|
| **StartsWith** | Check prefix | Boolean | Validation, filtering |
| **EndsWith** | Check suffix | Boolean | File extensions, domains |
| **Contains** | Check substring | Boolean | Content search |
| **IndexOf** | Find position | Integer | Extract, parse |

### Usage Decision
- Use **StartsWith** for prefix validation and protocol checking
- Use **EndsWith** for file extensions and domain validation
- Use **Contains** for general content searching
- Use **IndexOf** when you need the exact position