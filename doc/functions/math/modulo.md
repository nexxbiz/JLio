# modulo Function

Calculate the remainder of division between two numbers.

## Overview

The `modulo` function returns the remainder after dividing the dividend by the divisor. This operation is useful for cyclic calculations, determining divisibility, extracting digits, and implementing periodic behaviors.

## Syntax

### Expression Format
```json
"=modulo(dividend, divisor)"
```

### Builder Pattern
```csharp
ModuloBuilders.Modulo(dividend, divisor)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| dividend | number/string | Yes | The number to be divided. Accepts numbers, numeric strings, or JSONPath expressions |
| divisor | number/string | Yes | The number to divide by. Accepts numbers, numeric strings, or JSONPath expressions. Cannot be zero |

## Examples

### Basic Usage
```json
{
  "path": "$.remainder",
  "value": "=modulo(10, 3)",
  "command": "add"
}
```

**Result**: `1`

### String Number Conversion
```json
{
  "path": "$.remainder",
  "value": "=modulo('17', '5')",
  "command": "add"
}
```

**Result**: `2`

### JSONPath Integration
```json
{
  "path": "$.checkDigit",
  "value": "=modulo($.accountNumber, 10)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "accountNumber": 123456
}
```

**Result**: `6`

### Practical Applications

#### Odd/Even Detection
```json
{
  "path": "$.isEven",
  "value": "=modulo($.value, 2) == 0",
  "command": "add"
}
```

#### Cyclic Index Calculation
```json
{
  "path": "$.cyclePosition",
  "value": "=modulo($.currentIndex, $.cycleLength)",
  "command": "add"
}
```

#### Checksum Validation
```json
{
  "path": "$.checksumDigit",
  "value": "=modulo($.number, 11)",
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Modulo
```csharp
var script = new JLioScript()
    .Add(ModuloBuilders.Modulo("17", "5"))
    .OnPath("$.result");
```

### Complex Calculations
```csharp
var script = new JLioScript()
    .Add(ModuloBuilders.Modulo("$.totalItems", "$.pageSize"))
    .OnPath("$.remainingItems")
    .Add(ModuloBuilders.Modulo("$.timestamp", "60"))
    .OnPath("$.seconds");
```

## Data Type Support

### Numeric Types
- **Positive Integers**: Standard modulo (e.g., `10 % 3` → `1`)
- **Negative Dividend**: Result follows C# semantics (e.g., `-10 % 3` → `-1`)
- **Negative Divisor**: Result follows C# semantics (e.g., `10 % -3` → `1`)
- **Both Negative**: Result follows C# semantics (e.g., `-10 % -3` → `-1`)
- **Decimal Values**: Supports floating-point modulo (e.g., `10.5 % 3` → `1.5`)

### String Numbers
Automatically converts numeric strings using InvariantCulture:
```json
"=modulo('100', '7')"    // Result: 2
"=modulo('10.5', '3')"   // Result: 1.5
"=modulo('-10', '3')"    // Result: -1
```

### Null Values
Treats `null` as `0`:
```json
"=modulo(null, 5)"    // Result: 0
"=modulo(10, null)"   // Error: divisor cannot be zero
```

## Mathematical Behavior

### Positive Operands
Standard remainder calculation:
- `modulo(10, 3)` = `1`
- `modulo(15, 4)` = `3`
- `modulo(7, 7)` = `0`

### Negative Dividend
Result has same sign as dividend (C# behavior):
- `modulo(-10, 3)` = `-1`
- `modulo(-15, 4)` = `-3`
- `modulo(-7, 7)` = `0`

### Negative Divisor
Result has same sign as dividend (C# behavior):
- `modulo(10, -3)` = `1`
- `modulo(15, -4)` = `3`
- `modulo(-10, -3)` = `-1`

### Decimal Values
Supports floating-point remainder:
- `modulo(10.5, 3)` = `1.5`
- `modulo(7.8, 2.5)` = `0.3`
- `modulo(5.5, 1.5)` = `1.0`

## Error Handling

### Argument Validation
```json
// Correct usage
"=modulo(10, 3)"  // → Result: 1

// Errors
"=modulo(10)"        // → Missing required argument
"=modulo(10, 3, 2)"  // → Too many arguments
"=modulo(10, 0)"     // → Division by zero
"=modulo('text', 3)" // → Non-numeric string
"=modulo({}, 3)"     // → Object not supported
"=modulo([1, 2], 3)" // → Array not supported
```

### Error Messages
- **Missing arguments**: "modulo requires exactly 2 arguments (dividend, divisor)"
- **Division by zero**: "modulo divisor cannot be zero"
- **Invalid types**: "modulo can only handle numeric values. Current type = [Type]"
- **Parse failures**: Non-numeric strings result in parsing errors

## Use Cases

### Programming Applications
1. **Array Index Wrapping**: Calculate cyclic array indices
2. **Pagination**: Determine items on last page
3. **Scheduling**: Calculate repeating events
4. **Hash Functions**: Distribute values across buckets

### Financial Applications
1. **Payment Cycles**: Calculate days in billing period
2. **Installment Plans**: Determine remaining payments
3. **Currency Distribution**: Allocate coins efficiently
4. **Tax Calculations**: Handle periodic tax schedules

### Data Processing
1. **Batch Processing**: Group records into batches
2. **Data Distribution**: Partition data across nodes
3. **Load Balancing**: Distribute requests evenly
4. **Checksum Validation**: Verify data integrity

## Performance Considerations

- **Execution Speed**: Fast single operation using native `%` operator
- **Memory Usage**: Minimal memory footprint
- **String Parsing**: Optimized with InvariantCulture
- **Numeric Precision**: Maintains full numeric precision for decimals

## Integration Examples

### With Conditional Logic
```json
{
  "path": "$.isMultipleOfFive",
  "command": "ifElse",
  "condition": "=modulo($.value, 5) == 0",
  "then": [
    {
      "path": "$.status",
      "value": "DIVISIBLE",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.status",
      "value": "NOT_DIVISIBLE",
      "command": "add"
    }
  ]
}
```

### Pagination Logic
```csharp
var script = new JLioScript()
    // Calculate full pages
    .Add(CalculateBuilders.Calculate("floor($.totalItems / $.pageSize)"))
    .OnPath("$.fullPages")
    // Calculate items on last page
    .Add(ModuloBuilders.Modulo("$.totalItems", "$.pageSize"))
    .OnPath("$.lastPageItems");
```

### Cyclic Processing
```json
{
  "path": "$.dayOfWeek",
  "value": "=modulo($.daysSinceEpoch, 7)",
  "command": "add"
}
```

## Related Functions

- **[subtract](subtract.md)**: Subtraction operation (modulo is remainder after division)
- **[floor](floor.md)**: Round down (useful for calculating quotient with modulo)
- **[calculate](calculate.md)**: Complex expressions (can combine with modulo)
- **[pow](pow.md)**: Power operation (useful in modular arithmetic)
- **[abs](abs.md)**: Absolute value (can be combined for unsigned modulo)

## Mathematical Properties

### Identity Properties
- `modulo(x, y)` where `x < y` and both positive → `x`
- `modulo(x, y)` where `x % y == 0` → `0`
- `modulo(0, y)` → `0`
- Range: For positive operands, `0 ≤ modulo(x, y) < y`

### Useful Relationships
- `x = floor(x / y) * y + modulo(x, y)` (division algorithm)
- `modulo(x + y, z) = modulo(modulo(x, z) + modulo(y, z), z)` (modular addition)
- `modulo(x * y, z) = modulo(modulo(x, z) * modulo(y, z), z)` (modular multiplication)

## Best Practices

### When to Use Modulo
1. **Cyclic Behavior**: Implementing repeating patterns
2. **Divisibility Tests**: Checking if numbers divide evenly
3. **Range Wrapping**: Keeping values within bounds
4. **Digit Extraction**: Getting specific digits from numbers

### Common Patterns
```csharp
// Check if even
var script = new JLioScript()
    .Add(ModuloBuilders.Modulo("$.number", "2"))
    .OnPath("$.remainder")
    .Add(CalculateBuilders.Calculate("$.remainder == 0"))
    .OnPath("$.isEven");

// Cyclic rotation
var script = new JLioScript()
    .Add(ModuloBuilders.Modulo("$.index", "$.arrayLength"))
    .OnPath("$.wrappedIndex");
```

### Avoid When
1. **Regular Division Needed**: Use calculate or subtract instead
2. **Always Positive Results Needed**: Consider `abs(modulo(x, y))` pattern
3. **Integer-Only Logic**: Ensure divisor and dividend are integers if needed

## Advanced Usage Patterns

### Checksum Calculation
```json
{
  "path": "$.luhnChecksum",
  "value": "=modulo(10 - modulo($.summedDigits, 10), 10)",
  "command": "add"
}
```

### Time-Based Scheduling
```csharp
// Calculate minutes in current hour
var script = new JLioScript()
    .Add(ModuloBuilders.Modulo("$.totalMinutes", "60"))
    .OnPath("$.minutesInHour")
    // Calculate hours in current day
    .Add(ModuloBuilders.Modulo("$.totalHours", "24"))
    .OnPath("$.hoursInDay");
```

### Load Distribution
```json
{
  "path": "$.assignedServer",
  "value": "=modulo($.requestId, $.serverCount)",
  "command": "add"
}
```

## C# Modulo Behavior

JLio follows C# modulo semantics:
- Result sign matches the dividend (first operand)
- `a % b = a - (b * trunc(a/b))`
- Different from mathematical modulo in handling negatives

**Examples comparing to mathematical modulo:**
| Expression | C# Result | Math Result |
|------------|-----------|-------------|
| `10 % 3` | `1` | `1` |
| `-10 % 3` | `-1` | `2` |
| `10 % -3` | `1` | `-2` |
| `-10 % -3` | `-1` | `-1` |

## Notes on Division by Zero

The modulo function explicitly checks for division by zero and returns an error. This is different from the mathematical undefined behavior and provides clear error messaging for debugging.

```json
"=modulo(10, 0)"  // Error: "modulo divisor cannot be zero"
```
