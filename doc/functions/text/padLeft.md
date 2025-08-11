# PadLeft Function Documentation

## Overview

The `PadLeft` function pads a string with specified characters on the left side to reach a desired total width. It's essential for creating fixed-width output, formatting numbers with leading zeros, right-aligning text, and generating structured data displays.

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
// Pad with spaces to specified width
"=padLeft('123', 8)"

// Pad with custom character (leading zeros)
"=padLeft('123', 8, '0')"

// JSONPath source
"=padLeft($.number, $.width, $.padChar)"
```

### Programmatic Usage
```csharp
// Pad with spaces (default)
var padLeftFunction = new PadLeft("text", "totalWidth");

// Pad with custom character
var padLeftFunction = new PadLeft("text", "totalWidth", "padChar");

// Empty constructor for dynamic arguments
var padLeftFunction = new PadLeft();
```

### Builder Pattern
```csharp
var padLeftFunction = PadLeftBuilders.PadLeft("$.text", "10");
var zeroPadded = PadLeftBuilders.PadLeft("$.number", "8", "0");
```

## Parameters

- **text** (required): The source string to pad
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **totalWidth** (required): The desired total width of the result string
  - **Type**: Integer or string representation of integer
  - **Validation**: Must be non-negative
  - **Behavior**: If text is longer than totalWidth, original text is returned
- **padChar** (optional): The character to use for padding
  - **Type**: String (first character is used)
  - **Default**: Space character (' ')
  - **Empty String**: Uses space character if empty

## Return Value

- **Type**: String (JValue)
- **Value**: String padded on the left to reach totalWidth

## Examples

### Basic Left Padding with Spaces
```json
{
  "path": "$.paddedText",
  "value": "=padLeft('123', 8)",
  "command": "add"
}
```

**Result**:
```json
{
  "paddedText": "     123"
}
```

### Leading Zero Padding
```json
{
  "path": "$.paddedNumber",
  "value": "=padLeft('123', 8, '0')",
  "command": "add"
}
```

**Result**:
```json
{
  "paddedNumber": "00000123"
}
```

### ID Generation with Zero Padding
```json
{
  "path": "$.formattedId",
  "value": "=padLeft(toString($.id), 6, '0')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "id": 42
}
```

**Result**:
```json
{
  "id": 42,
  "formattedId": "000042"
}
```

### Right-Aligned Text in Fixed Width
```json
{
  "path": "$.rightAligned",
  "value": "=padLeft($.amount, 12)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "amount": "$1,234.56"
}
```

**Result**:
```json
{
  "amount": "$1,234.56",
  "rightAligned": "   $1,234.56"
}
```

### Array Processing for Invoice Numbers
```json
{
  "path": "$.invoices[*].invoiceNumber",
  "value": "=concat('INV-', padLeft(toString(@.id), 6, '0'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "invoices": [
    {"id": 1, "amount": 100},
    {"id": 23, "amount": 250},
    {"id": 456, "amount": 750}
  ]
}
```

**Result**:
```json
{
  "invoices": [
    {"id": 1, "amount": 100, "invoiceNumber": "INV-000001"},
    {"id": 23, "amount": 250, "invoiceNumber": "INV-000023"},
    {"id": 456, "amount": 750, "invoiceNumber": "INV-000456"}
  ]
}
```

### Time Formatting
```json
{
  "path": "$.formattedTime",
  "value": "=concat(padLeft(toString($.hours), 2, '0'), ':', padLeft(toString($.minutes), 2, '0'), ':', padLeft(toString($.seconds), 2, '0'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "hours": 9,
  "minutes": 5,
  "seconds": 3
}
```

**Result**:
```json
{
  "hours": 9,
  "minutes": 5,
  "seconds": 3,
  "formattedTime": "09:05:03"
}
```

### Financial Report Alignment
```json
{
  "path": "$.formattedAmount",
  "value": "=padLeft(concat('$', toString($.amount)), 15)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "amount": 1234.56
}
```

**Result**:
```json
{
  "amount": 1234.56,
  "formattedAmount": "       $1234.56"
}
```

### Sequential Code Generation
```json
{
  "path": "$.productCode",
  "value": "=concat($.category, '-', padLeft(toString($.sequence), 4, '0'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "category": "ELEC",
  "sequence": 7
}
```

**Result**:
```json
{
  "category": "ELEC",
  "sequence": 7,
  "productCode": "ELEC-0007"
}
```

### Table Column Formatting
```json
{
  "path": "$.reports[*].formattedRow",
  "value": "=concat(padLeft(@.name, 20), ' | ', padLeft(toString(@.value), 10, '0'), ' | ', padLeft(@.status, 8))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "reports": [
    {"name": "Sales", "value": 12500, "status": "Active"},
    {"name": "Marketing", "value": 8750, "status": "Review"}
  ]
}
```

**Result**:
```json
{
  "reports": [
    {"name": "Sales", "value": 12500, "status": "Active", "formattedRow": "               Sales | 0000012500 |   Active"},
    {"name": "Marketing", "value": 8750, "status": "Review", "formattedRow": "           Marketing | 0000008750 |   Review"}
  ]
}
```

## Advanced Usage

### Multi-Column Report Generation
```csharp
var script = new JLioScript()
    .Add(PadLeftBuilders.PadLeft("@.name", "15"))
    .OnPath("$.reports[*].col1")
    .Add(PadLeftBuilders.PadLeft("toString(@.amount)", "12", "0"))
    .OnPath("$.reports[*].col2")
    .Add(PadLeftBuilders.PadLeft("@.status", "10"))
    .OnPath("$.reports[*].col3");
```

### Sequential Number Formatting
```csharp
var script = new JLioScript()
    .Add(PadLeftBuilders.PadLeft("toString(@.orderNumber)", "8", "0"))
    .OnPath("$.orders[*].formattedOrderNumber")
    .Add(PadLeftBuilders.PadLeft("toString(@.customerNumber)", "6", "0"))
    .OnPath("$.orders[*].formattedCustomerNumber");
```

### Fixed-Width Data Export
```json
{
  "path": "$.fixedWidthRecord",
  "value": "=concat(padLeft($.customerCode, 10), padLeft(toString($.amount), 12, '0'), padLeft($.status, 8))",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=padLeft('hello', 10)"  // Result: "     hello"
```

### Number Conversion
```json
"=padLeft(123, 8, '0')"  // Result: "00000123" (from "123")
```

### Empty String
```json
"=padLeft('', 5, '-')"  // Result: "-----"
```

### Text Longer Than Width
```json
"=padLeft('Hello World', 5)"  // Result: "Hello World" (unchanged)
```

## Fluent API Usage

### Basic Left Padding
```csharp
var script = new JLioScript()
    .Add(PadLeftBuilders.PadLeft("$.text", "15"))
    .OnPath("$.paddedText");
```

### Zero Padding for Numbers
```csharp
var script = new JLioScript()
    .Add(PadLeftBuilders.PadLeft("toString($.id)", "6", "0"))
    .OnPath("$.formattedId");
```

### Multi-field Formatting
```csharp
var script = new JLioScript()
    .Add(PadLeftBuilders.PadLeft("@.code", "8"))
    .OnPath("$.items[*].paddedCode")
    .Add(PadLeftBuilders.PadLeft("toString(@.quantity)", "5", "0"))
    .OnPath("$.items[*].paddedQuantity");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=padLeft('text')"  // Logs error: "PadLeft requires 2 or 3 arguments"

// Too many arguments
"=padLeft('text', 10, '0', 'extra')"  // Logs error: "PadLeft requires 2 or 3 arguments"
```

### Invalid Width Parameter
```json
// Non-numeric width
"=padLeft('text', 'abc')"  // Logs error: "totalWidth must be a non-negative integer"

// Negative width
"=padLeft('text', -5)"  // Logs error: "totalWidth must be a non-negative integer"
```

## Performance Considerations

- **String Length**: Performance scales with target width
- **Character Count**: More padding characters require more memory
- **Memory Usage**: Creates new string instances for results
- **Large Widths**: Very large widths may impact memory usage

## Best Practices

1. **Number Formatting**: Use for consistent number display with leading zeros
2. **ID Generation**: Create formatted identifiers and codes
3. **Report Alignment**: Right-align numeric data in fixed-width reports
4. **Time Formatting**: Ensure consistent time display (HH:MM:SS)
5. **Performance**: Consider memory usage with very large widths
6. **Testing**: Test with various text lengths and edge cases

## Common Patterns

### Leading Zero Pattern
```json
"=padLeft(toString($.number), 6, '0')"
```

### Right-Aligned Text Pattern
```json
"=padLeft($.text, 20)"
```

### ID Code Pattern
```json
"=concat('ID-', padLeft(toString($.id), 8, '0'))"
```

### Time Format Pattern
```json
"=concat(padLeft(toString($.hour), 2, '0'), ':', padLeft(toString($.minute), 2, '0'))"
```

### Financial Format Pattern
```json
"=padLeft(concat('$', toString($.amount)), 15)"
```

## Integration Examples

### With String Functions
```json
"=padLeft(toUpper($.code), 10)"  // Uppercase and pad
```

### With Math Functions
```json
"=padLeft(toString(round($.value, 2)), 12, '0')"  // Round and zero-pad
```

### With Conditional Logic
```json
"=padLeft($.text, if(length($.text) < 10, 15, length($.text) + 5))"
```

### Report Generation
```json
[
  {
    "path": "$.header",
    "value": "=concat(padLeft('Item', 20), padLeft('Quantity', 10), padLeft('Price', 12))",
    "command": "add"
  },
  {
    "path": "$.items[*].row",
    "value": "=concat(padLeft(@.name, 20), padLeft(toString(@.qty), 10, '0'), padLeft(concat('$', toString(@.price)), 12))",
    "command": "add"
  }
]
```

## Use Case Examples

### Invoice Generation
```csharp
var script = new JLioScript()
    .Add(PadLeftBuilders.PadLeft("toString(@.invoiceNumber)", "8", "0"))
    .OnPath("$.invoices[*].formattedNumber")
    .Add(PadLeftBuilders.PadLeft("concat('$', toString(@.amount))", "12"))
    .OnPath("$.invoices[*].formattedAmount");
```

### Employee ID Generation
```json
{
  "path": "$.employeeId",
  "value": "=concat($.department, '-', padLeft(toString($.empNumber), 5, '0'))",
  "command": "add"
}
```

### Log File Formatting
```json
{
  "path": "$.formattedLogEntry",
  "value": "=concat(padLeft(toString($.lineNumber), 6, '0'), ' | ', $.timestamp, ' | ', $.message)",
  "command": "add"
}
```

### Product Code System
```json
{
  "path": "$.productSku",
  "value": "=concat(toUpper($.category), padLeft(toString($.productId), 4, '0'), toUpper($.variant))",
  "command": "add"
}
```

## Comparison with PadRight

| Function | Padding Side | Use Case |
|----------|-------------|----------|
| **PadLeft** | Left side | Numbers, IDs, right-aligned text |
| **PadRight** | Right side | Text fields, left-aligned content |

### Usage Decision
- Use **PadLeft** for numeric data, identifiers, and right-aligned text
- Use **PadRight** for text content and left-aligned fields
- Consider your formatting requirements and target system

## Financial Formatting Examples

### Currency Alignment
```json
"=padLeft(concat('$', toString($.amount)), 15)"  // Right-align currency
```

### Account Numbers
```json
"=padLeft(toString($.accountNumber), 12, '0')"  // Zero-pad account numbers
```

### Transaction IDs
```json
"=concat('TXN-', padLeft(toString($.transactionId), 10, '0'))"  // Format transaction IDs
```