# EndsWith Function Documentation

## Overview

The `EndsWith` function checks whether a string ends with a specified suffix. It supports both case-sensitive and case-insensitive matching, making it essential for file extension validation, domain checking, URL processing, and content filtering workflows.

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
// Basic ends-with check (case-sensitive)
"=endsWith('document.pdf', '.pdf')"

// Case-insensitive check
"=endsWith('Document.PDF', '.pdf', true)"

// JSONPath source
"=endsWith($.filename, '.jpg')"
```

### Programmatic Usage
```csharp
// Case-sensitive check
var endsWithFunction = new EndsWith("text", "suffix");

// Case-insensitive check
var endsWithFunction = new EndsWith("text", "suffix", "true");

// Empty constructor for dynamic arguments
var endsWithFunction = new EndsWith();
```

### Builder Pattern
```csharp
var endsWithFunction = EndsWithBuilders.EndsWith("$.text", "suffix");
var caseInsensitive = EndsWithBuilders.EndsWith("$.text", "SUFFIX", "true");
```

## Parameters

- **text** (required): The source string to check
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **suffix** (required): The suffix string to check for
  - **Type**: String, JSONPath expression, or any value
  - **Empty String**: Always returns true (empty string is suffix of any string)
- **ignoreCase** (optional): Whether to perform case-insensitive matching
  - **Type**: Boolean, string representation, or integer (0/1)
  - **Default**: false (case-sensitive)

## Return Value

- **Type**: Boolean (JValue)
- **Value**: true if string ends with suffix, false otherwise

## Examples

### Basic Suffix Check
```json
{
  "path": "$.endsWithPdf",
  "value": "=endsWith('document.pdf', '.pdf')",
  "command": "add"
}
```

**Result**:
```json
{
  "endsWithPdf": true
}
```

### Case-Insensitive File Extension Check
```json
{
  "path": "$.isImageFile",
  "value": "=endsWith(toLower($.filename), '.jpg') || endsWith(toLower($.filename), '.png')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "photo.JPG"
}
```

**Result**:
```json
{
  "filename": "photo.JPG",
  "isImageFile": true
}
```

### Domain Validation
```json
{
  "path": "$.isCompanyDomain",
  "value": "=endsWith($.email, '@company.com')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "email": "john.doe@company.com"
}
```

**Result**:
```json
{
  "email": "john.doe@company.com",
  "isCompanyDomain": true
}
```

### URL Path Validation
```json
{
  "path": "$.isApiEndpoint",
  "value": "=endsWith($.url, '/api') || endsWith($.url, '/api/')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "url": "https://example.com/api"
}
```

**Result**:
```json
{
  "url": "https://example.com/api",
  "isApiEndpoint": true
}
```

### Array Processing for File Filtering
```json
{
  "path": "$.documentFiles",
  "value": "$.files[?endsWith(@.name, '.pdf') || endsWith(@.name, '.doc') || endsWith(@.name, '.docx')]",
  "command": "add"
}
```

**Input Data**:
```json
{
  "files": [
    {"name": "report.pdf", "size": 1024},
    {"name": "image.jpg", "size": 512},
    {"name": "presentation.docx", "size": 2048},
    {"name": "data.json", "size": 256}
  ]
}
```

**Result**:
```json
{
  "files": [...],
  "documentFiles": [
    {"name": "report.pdf", "size": 1024},
    {"name": "presentation.docx", "size": 2048}
  ]
}
```

### Log File Classification
```json
{
  "path": "$.logs[*].fileType",
  "value": "=if(endsWith(@.filename, '.log'), 'log', if(endsWith(@.filename, '.txt'), 'text', 'other'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "logs": [
    {"filename": "error.log"},
    {"filename": "debug.txt"},
    {"filename": "config.json"}
  ]
}
```

**Result**:
```json
{
  "logs": [
    {"filename": "error.log", "fileType": "log"},
    {"filename": "debug.txt", "fileType": "text"},
    {"filename": "config.json", "fileType": "other"}
  ]
}
```

### Email Provider Detection
```json
{
  "path": "$.emailProvider",
  "value": "=if(endsWith($.email, '@gmail.com'), 'Gmail', if(endsWith($.email, '@outlook.com'), 'Outlook', 'Other'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "email": "user@gmail.com"
}
```

**Result**:
```json
{
  "email": "user@gmail.com",
  "emailProvider": "Gmail"
}
```

### Backup File Detection
```json
{
  "path": "$.isBackupFile",
  "value": "=endsWith($.filename, '.bak') || endsWith($.filename, '.backup') || endsWith($.filename, '~')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "important_data.bak"
}
```

**Result**:
```json
{
  "filename": "important_data.bak",
  "isBackupFile": true
}
```

### Script File Validation
```json
{
  "path": "$.isExecutableScript",
  "value": "=endsWith($.filename, '.sh') || endsWith($.filename, '.bat') || endsWith($.filename, '.ps1')",
  "command": "add"
}
```

## Advanced Usage

### File Type Classification
```csharp
var script = new JLioScript()
    .Add(EndsWithBuilders.EndsWith("@.filename", ".jpg", "true"))
    .OnPath("$.files[*].isJpeg")
    .Add(EndsWithBuilders.EndsWith("@.filename", ".png", "true"))
    .OnPath("$.files[*].isPng")
    .Add(EndsWithBuilders.EndsWith("@.filename", ".gif", "true"))
    .OnPath("$.files[*].isGif");
```

### Multi-Domain Email Validation
```csharp
var script = new JLioScript()
    .Add(EndsWithBuilders.EndsWith("@.email", "@company.com"))
    .OnPath("$.users[*].isCompanyEmail")
    .Add(EndsWithBuilders.EndsWith("@.email", "@partner.com"))
    .OnPath("$.users[*].isPartnerEmail")
    .Add(EndsWithBuilders.EndsWith("@.email", "@contractor.com"))
    .OnPath("$.users[*].isContractorEmail");
```

### URL Endpoint Classification
```json
{
  "path": "$.endpointType",
  "value": {
    "isApi": "=endsWith($.path, '/api')",
    "isWebhook": "=endsWith($.path, '/webhook')",
    "isCallback": "=endsWith($.path, '/callback')",
    "isHealth": "=endsWith($.path, '/health')"
  },
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=endsWith('hello.txt', '.txt')"  // Result: true
"=endsWith('hello.txt', '.TXT')"  // Result: false (case-sensitive)
"=endsWith('hello.txt', '.TXT', true)"  // Result: true (case-insensitive)
```

### Number Conversion
```json
"=endsWith(12345, '45')"  // Result: true (from "12345")
```

### Empty String Checks
```json
"=endsWith('hello', '')"  // Result: true (empty string is suffix of any string)
"=endsWith('', 'hello')"  // Result: false (can't end with non-empty in empty string)
```

### File Extension Examples
```json
"=endsWith('document.pdf', '.pdf')"     // Result: true
"=endsWith('image.jpeg', '.jpg')"       // Result: false
"=endsWith('script.PS1', '.ps1', true)" // Result: true (case-insensitive)
```

## Fluent API Usage

### Basic Suffix Check
```csharp
var script = new JLioScript()
    .Add(EndsWithBuilders.EndsWith("$.filename", ".pdf"))
    .OnPath("$.isPdfFile");
```

### Multiple Extension Checks
```csharp
var script = new JLioScript()
    .Add(EndsWithBuilders.EndsWith("$.filename", ".jpg", "true"))
    .OnPath("$.isJpeg")
    .Add(EndsWithBuilders.EndsWith("$.filename", ".png", "true"))
    .OnPath("$.isPng");
```

### Domain Validation
```csharp
var script = new JLioScript()
    .Add(EndsWithBuilders.EndsWith("$.email", "@company.com"))
    .OnPath("$.isCompanyEmail");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=endsWith('text')"  // Logs error: "EndsWith requires 2 or 3 arguments"

// Too many arguments
"=endsWith('text', 'suffix', true, 'extra')"  // Logs error: "EndsWith requires 2 or 3 arguments"
```

### Invalid Boolean Parameter
```json
// Invalid ignoreCase value
"=endsWith('text', 'suffix', 'invalid')"  // Logs error: "ignoreCase must be a valid boolean"
```

## Performance Considerations

- **String Length**: Performance depends on suffix length, not full string length
- **Case Sensitivity**: Case-insensitive operations may be slightly slower
- **Memory Usage**: Minimal additional memory overhead
- **Suffix Length**: Longer suffixes may impact performance slightly

## Best Practices

1. **File Extensions**: Use for file type validation and processing
2. **Domain Checking**: Validate email domains and URLs
3. **Path Validation**: Check URL endpoints and file paths
4. **Security**: Validate file uploads and input formats
5. **Case Handling**: Use case-insensitive matching for file extensions
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### File Extension Check Pattern
```json
"=endsWith(toLower($.filename), '.pdf')"
```

### Email Domain Pattern
```json
"=endsWith($.email, '@company.com')"
```

### URL Endpoint Pattern
```json
"=endsWith($.path, '/api')"
```

### Backup File Pattern
```json
"=endsWith($.filename, '.bak') || endsWith($.filename, '~')"
```

### Script File Pattern
```json
"=endsWith(toLower($.filename), '.sh') || endsWith(toLower($.filename), '.bat')"
```

## Integration Examples

### With Conditional Logic
```json
"=if(endsWith($.filename, '.tmp'), 'temporary', 'permanent')"
```

### With String Functions
```json
"=endsWith(trim(toLower($.filename)), '.log')"
```

### With Array Operations
```json
"$.files[?endsWith(@.name, '.pdf')]"
```

### With File Processing
```json
"=if(endsWith($.filename, '.pdf'), substring($.filename, 0, length($.filename) - 4), $.filename)"
```

## Security Applications

### File Upload Validation
```json
"=endsWith(toLower($.uploadedFile), '.jpg') || endsWith(toLower($.uploadedFile), '.png')"
```

### Script Execution Prevention
```json
"=!endsWith(toLower($.filename), '.exe') && !endsWith(toLower($.filename), '.bat')"
```

### Safe File Extensions
```json
{
  "path": "$.isSafeFile",
  "value": "=endsWith(toLower($.filename), '.txt') || endsWith(toLower($.filename), '.pdf') || endsWith(toLower($.filename), '.jpg')",
  "command": "add"
}
```

## Comparison with Other Functions

| Function | Purpose | Return Type | Use Case |
|----------|---------|-------------|----------|
| **EndsWith** | Check suffix | Boolean | File extensions, domains |
| **StartsWith** | Check prefix | Boolean | Protocols, prefixes |
| **Contains** | Check substring | Boolean | Content search |
| **IndexOf** | Find position | Integer | Extract, parse |

### Usage Decision
- Use **EndsWith** for file extensions, domain validation, and URL paths
- Use **StartsWith** for protocols and command detection
- Use **Contains** for general content searching
- Combine multiple functions for comprehensive validation

## File Type Detection Examples

### Image Files
```json
"=endsWith(toLower($.filename), '.jpg') || endsWith(toLower($.filename), '.jpeg') || endsWith(toLower($.filename), '.png') || endsWith(toLower($.filename), '.gif')"
```

### Document Files
```json
"=endsWith(toLower($.filename), '.pdf') || endsWith(toLower($.filename), '.doc') || endsWith(toLower($.filename), '.docx')"
```

### Archive Files
```json
"=endsWith(toLower($.filename), '.zip') || endsWith(toLower($.filename), '.rar') || endsWith(toLower($.filename), '.7z')"
```