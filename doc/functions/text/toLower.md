# ToLower Function Documentation

## Overview

The `ToLower` function converts all alphabetic characters in a string to lowercase. It supports culture-specific conversion rules and is essential for text normalization, URL generation, case-insensitive comparisons, and data standardization workflows.

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
// Basic lowercase conversion
"=toLower('Hello World')"

// Culture-specific conversion
"=toLower('ISTANBUL', 'tr-TR')"

// JSONPath source
"=toLower($.text)"
```

### Programmatic Usage
```csharp
// Default culture (invariant)
var toLowerFunction = new ToLower("text");

// Specific culture
var toLowerFunction = new ToLower("text", "culture");

// Empty constructor for dynamic arguments
var toLowerFunction = new ToLower();
```

### Builder Pattern
```csharp
var toLowerFunction = ToLowerBuilders.ToLower("$.text");
var withCulture = ToLowerBuilders.ToLower("$.text", "tr-TR");
```

## Parameters

- **text** (required): The source string to convert
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **culture** (optional): Culture code for culture-specific conversion
  - **Type**: String representing culture code (e.g., "en-US", "tr-TR")
  - **Default**: InvariantCulture for consistent behavior
  - **Validation**: Must be a valid culture identifier

## Return Value

- **Type**: String (JValue)
- **Value**: String with all alphabetic characters converted to lowercase

## Examples

### Basic Lowercase Conversion
```json
{
  "path": "$.lowercaseText",
  "value": "=toLower('Hello World')",
  "command": "add"
}
```

**Result**:
```json
{
  "lowercaseText": "hello world"
}
```

### URL Generation
```json
{
  "path": "$.urlSlug",
  "value": "=toLower(replace($.title, ' ', '-'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "title": "My Blog Post Title"
}
```

**Result**:
```json
{
  "title": "My Blog Post Title",
  "urlSlug": "my-blog-post-title"
}
```

### Email Normalization
```json
{
  "path": "$.normalizedEmail",
  "value": "=toLower(trim($.emailAddress))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "emailAddress": "  JOHN.DOE@EXAMPLE.COM  "
}
```

**Result**:
```json
{
  "emailAddress": "  JOHN.DOE@EXAMPLE.COM  ",
  "normalizedEmail": "john.doe@example.com"
}
```

### Case-Insensitive Search Preparation
```json
{
  "path": "$.searchableContent",
  "value": "=toLower($.content)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "content": "JavaScript Programming Tutorial"
}
```

**Result**:
```json
{
  "content": "JavaScript Programming Tutorial",
  "searchableContent": "javascript programming tutorial"
}
```

### Array Processing for Data Standardization
```json
{
  "path": "$.tags[*].normalized",
  "value": "=toLower(trim(@.name))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "tags": [
    {"name": "JAVASCRIPT"},
    {"name": "Web Development"},
    {"name": "  FRONTEND  "}
  ]
}
```

**Result**:
```json
{
  "tags": [
    {"name": "JAVASCRIPT", "normalized": "javascript"},
    {"name": "Web Development", "normalized": "web development"},
    {"name": "  FRONTEND  ", "normalized": "frontend"}
  ]
}
```

### File System Operations
```json
{
  "path": "$.safeFilename",
  "value": "=toLower(replace(replace($.filename, ' ', '_'), '[^a-z0-9_.-]', ''))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "My Document (Final).PDF"
}
```

**Result**:
```json
{
  "filename": "My Document (Final).PDF",
  "safeFilename": "my_document_final.pdf"
}
```

### Database Field Normalization
```json
{
  "path": "$.users[*].username",
  "value": "=toLower(replace(@.displayName, ' ', ''))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"displayName": "John Doe"},
    {"displayName": "Jane Smith"},
    {"displayName": "Bob Wilson"}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"displayName": "John Doe", "username": "johndoe"},
    {"displayName": "Jane Smith", "username": "janesmith"},
    {"displayName": "Bob Wilson", "username": "bobwilson"}
  ]
}
```

### Culture-Specific Processing
```json
{
  "path": "$.turkishLower",
  "value": "=toLower('?STANBUL', 'tr-TR')",
  "command": "add"
}
```

**Result**:
```json
{
  "turkishLower": "istanbul"
}
```

### Configuration Key Generation
```json
{
  "path": "$.configKey",
  "value": "=toLower(replace($.settingName, ' ', '_'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "settingName": "Database Connection String"
}
```

**Result**:
```json
{
  "settingName": "Database Connection String",
  "configKey": "database_connection_string"
}
```

## Advanced Usage

### URL Slug Generation Pipeline
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("$.title"))
    .OnPath("$.step1")
    .Add(ToLowerBuilders.ToLower("$.step1"))
    .OnPath("$.step2")
    .Add(ReplaceBuilders.Replace("$.step2", " ", "-"))
    .OnPath("$.step3")
    .Add(ReplaceBuilders.Replace("$.step3", "[^a-z0-9-]", ""))
    .OnPath("$.urlSlug");
```

### Multi-field Normalization
```csharp
var script = new JLioScript()
    .Add(ToLowerBuilders.ToLower("@.email"))
    .OnPath("$.users[*].email")
    .Add(ToLowerBuilders.ToLower("@.username"))
    .OnPath("$.users[*].username")
    .Add(ToLowerBuilders.ToLower("@.role"))
    .OnPath("$.users[*].role");
```

### Search Index Preparation
```json
{
  "path": "$.searchIndex",
  "value": {
    "title": "=toLower($.title)",
    "description": "=toLower($.description)",
    "tags": "=toLower(join(' ', $.tags[*]))",
    "content": "=toLower($.content)"
  },
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=toLower('HELLO WORLD')"  // Result: "hello world"
"=toLower('MiXeD cAsE')"   // Result: "mixed case"
```

### Number Conversion
```json
"=toLower(12345)"  // Result: "12345" (numbers unchanged)
```

### Special Characters
```json
"=toLower('HELLO@WORLD.COM')"  // Result: "hello@world.com"
"=toLower('CAFÉ')"             // Result: "café" (accented chars handled)
```

### Culture-Specific Examples
```json
"=toLower('STRASSE', 'de-DE')"  // Result: "strasse"
"=toLower('?STANBUL', 'tr-TR')" // Result: "istanbul" (Turkish ? to i)
```

### Empty/Null Values
```json
"=toLower('')"         // Result: ""
"=toLower($.missing)"  // Result: "" (null treated as empty)
```

## Fluent API Usage

### Basic Lowercase
```csharp
var script = new JLioScript()
    .Add(ToLowerBuilders.ToLower("$.text"))
    .OnPath("$.lowercaseText");
```

### With Culture Specification
```csharp
var script = new JLioScript()
    .Add(ToLowerBuilders.ToLower("$.turkishText", "tr-TR"))
    .OnPath("$.lowercaseTurkish");
```

### Bulk Processing
```csharp
var script = new JLioScript()
    .Add(ToLowerBuilders.ToLower("@.email"))
    .OnPath("$.contacts[*].email")
    .Add(ToLowerBuilders.ToLower("@.domain"))
    .OnPath("$.contacts[*].domain");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=toLower()"  // Logs error: "ToLower requires 1 or 2 arguments"

// Too many arguments
"=toLower('text', 'en-US', 'extra')"  // Logs error: "ToLower requires 1 or 2 arguments"
```

### Invalid Culture Code
```json
// Invalid culture
"=toLower('text', 'invalid-culture')"  // Logs error: "Invalid culture code: invalid-culture"
```

## Performance Considerations

- **String Length**: Performance scales linearly with string length
- **Culture Processing**: Culture-specific conversion may be slightly slower
- **Memory Usage**: Creates new string instances for results
- **Invariant Culture**: Default invariant culture provides consistent, fast performance

## Best Practices

1. **Email Processing**: Always normalize email addresses to lowercase
2. **URL Generation**: Use lowercase for SEO-friendly URLs and consistency
3. **Database Keys**: Normalize keys and identifiers for consistent lookups
4. **Search Operations**: Prepare text for case-insensitive searching
5. **File Operations**: Use lowercase for cross-platform file compatibility
6. **Performance**: Use default culture for most data processing scenarios

## Common Patterns

### Email Normalization Pattern
```json
"=toLower(trim($.email))"
```

### URL Slug Pattern
```json
"=toLower(replace($.title, ' ', '-'))"
```

### Username Generation Pattern
```json
"=toLower(replace($.fullName, ' ', ''))"
```

### Search Preparation Pattern
```json
"=toLower($.searchTerm)"
```

### File Path Pattern
```json
"=toLower(replace($.filename, ' ', '_'))"
```

## Integration Examples

### With String Functions
```json
"=toLower(trim(replace($.text, '  ', ' ')))"  // Clean and normalize
```

### With Conditional Logic
```json
"=if($.preserveCase, $.text, toLower($.text))"
```

### With Validation
```json
"=contains(toLower($.content), toLower($.searchTerm))"  // Case-insensitive search
```

### With Array Processing
```json
"$.users[?toLower(@.role) == 'admin']"  // Case-insensitive filtering
```

### With Join Function
```json
"=toLower(join('-', split($.title, ' ')))"  // Create URL slug
```

## Culture Examples

### Common Scenarios
```json
// Turkish I handling
"=toLower('?', 'tr-TR')"    // Result: "i" (dotted to dotless)
"=toLower('I', 'tr-TR')"    // Result: "?" (dotless I to ?)

// English (standard behavior)
"=toLower('I', 'en-US')"    // Result: "i"

// German handling
"=toLower('GRO?', 'de-DE')" // Result: "groß"
```

## Use Case Examples

### E-commerce Product Search
```csharp
var script = new JLioScript()
    .Add(ToLowerBuilders.ToLower("@.name"))
    .OnPath("$.products[*].searchName")
    .Add(ToLowerBuilders.ToLower("@.category"))
    .OnPath("$.products[*].searchCategory")
    .Add(ToLowerBuilders.ToLower("@.brand"))
    .OnPath("$.products[*].searchBrand");
```

### Content Management System
```json
{
  "path": "$.seoData",
  "value": {
    "slug": "=toLower(replace($.title, ' ', '-'))",
    "metaKeywords": "=toLower(join(', ', $.keywords[*]))",
    "searchableContent": "=toLower($.content)"
  },
  "command": "add"
}
```

### User Registration Processing
```json
[
  {
    "path": "$.normalizedUser.email",
    "value": "=toLower(trim($.registrationData.email))",
    "command": "add"
  },
  {
    "path": "$.normalizedUser.username",
    "value": "=toLower(trim($.registrationData.username))",
    "command": "add"
  }
]
```

## Comparison with ToUpper

| Function | Primary Use Cases | Example Output |
|----------|------------------|----------------|
| **ToLower** | URLs, emails, usernames, search | `"hello world"` |
| **ToUpper** | Constants, headers, emphasis | `"HELLO WORLD"` |

### Usage Decision
- Use **ToLower** for data normalization, URLs, and user-facing identifiers
- Use **ToUpper** for constants, system headers, and visual emphasis
- Consider your target system requirements and conventions