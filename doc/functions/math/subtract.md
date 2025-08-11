# subtract Function

Subtract one numeric value from another.

## Overview

The `subtract` function performs subtraction between two numeric values, supporting numbers, numeric strings, and JSONPath expressions. It's essential for calculating differences, changes, and performing basic arithmetic operations in data transformations.

## Syntax

### Expression Format
```json
"=subtract(minuend, subtrahend)"
```

### Builder Pattern
```csharp
SubtractBuilders.Subtract(minuend, subtrahend)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| minuend | number/string | Yes | The number from which to subtract (first operand) |
| subtrahend | number/string | Yes | The number to subtract (second operand) |

## Examples

### Basic Subtraction
```json
{
  "path": "$.difference",
  "value": "=subtract(100, 25)",
  "command": "add"
}
```

**Result**: `75`

### String Number Support
```json
{
  "path": "$.netAmount",
  "value": "=subtract('150.75', '25.50')",
  "command": "add"
}
```

**Result**: `125.25`

### JSONPath Integration
```json
{
  "path": "$.profit",
  "value": "=subtract($.revenue, $.expenses)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "revenue": 10000,
  "expenses": 7500
}
```

**Result**: `2500`

### Mixed Data Types
```json
{
  "path": "$.balance",
  "value": "=subtract($.account.credits, '125.00')",
  "command": "add"
}
```

### Financial Calculations
```json
{
  "path": "$.financialAnalysis",
  "value": {
    "netProfit": "=subtract($.totalRevenue, $.totalExpenses)",
    "remainingBudget": "=subtract($.budget, $.spent)",
    "taxableAmount": "=subtract($.grossIncome, $.deductions)",
    "netWorth": "=subtract($.assets, $.liabilities)"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Subtraction
```csharp
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.total", "$.discount"))
    .OnPath("$.finalAmount");
```

### Financial Pipeline
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.revenues[*]"))
    .OnPath("$.totalRevenue")
    .Add(SumBuilders.Sum("$.expenses[*]"))
    .OnPath("$.totalExpenses")
    .Add(SubtractBuilders.Subtract("$.totalRevenue", "$.totalExpenses"))
    .OnPath("$.netProfit");
```

### Variance Analysis
```csharp
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.actual", "$.budget"))
    .OnPath("$.variance")
    .Add(SubtractBuilders.Subtract("$.currentYear", "$.previousYear"))
    .OnPath("$.yearOverYearChange");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=subtract('100.50', '25.25')"   // Result: 75.25
"=subtract('0', '5')"            // Result: -5
"=subtract('-10', '-5')"         // Result: -5
```

### Null Handling
- **null values**: Treated as `0`
- **First parameter null**: `0 - subtrahend`
- **Second parameter null**: `minuend - 0`

## Mathematical Behavior

### Standard Subtraction
Formula: `subtract(a, b) = a - b`

### Examples
```json
"=subtract(10, 3)"               // Result: 7
"=subtract(5, 8)"                // Result: -3 (negative result)
"=subtract(0, 5)"                // Result: -5
"=subtract(10, 0)"               // Result: 10
```

### Negative Numbers
```json
"=subtract(-5, 3)"               // Result: -8
"=subtract(-5, -3)"              // Result: -2
"=subtract(5, -3)"               // Result: 8
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=subtract(10, 5)"       // ? Result: 5
"=subtract('10', '5')"   // ? Result: 5
"=subtract($.a, $.b)"    // ? Uses JSONPath values

// Errors
"=subtract(10)"          // ? Missing second argument
"=subtract()"            // ? Missing both arguments
"=subtract(1, 2, 3)"     // ? Too many arguments
"=subtract('text', 5)"   // ? Non-numeric string
"=subtract([], 5)"       // ? Array not supported
```

### Error Messages
- **Wrong argument count**: "subtract requires exactly two arguments"
- **Invalid types**: "subtract can only handle numeric values"
- **Parse failures**: Non-numeric strings result in errors

## Use Cases

### Financial Applications
1. **Profit Calculation**: Revenue minus expenses
2. **Budget Analysis**: Budget minus actual spending
3. **Tax Calculations**: Gross minus deductions
4. **Account Balances**: Credits minus debits

### Business Analytics
1. **Performance Variance**: Actual minus target
2. **Growth Analysis**: Current minus previous period
3. **Cost Analysis**: Price minus cost for margins
4. **Inventory Changes**: Starting minus ending inventory

### System Metrics
1. **Resource Usage**: Capacity minus current usage
2. **Performance Deltas**: New metrics minus baseline
3. **Error Analysis**: Total requests minus successful requests
4. **Time Calculations**: End time minus start time

## Performance Considerations

- **Execution Speed**: Very fast two-operand operation
- **Memory Usage**: Minimal memory footprint
- **String Parsing**: Optimized with InvariantCulture
- **Precision**: Maintains full numeric precision

## Integration Examples

### Complete Financial Statement
```json
{
  "path": "$.incomeStatement",
  "value": {
    "grossRevenue": "=sum($.revenues[*])",
    "totalExpenses": "=sum($.expenses[*])",
    "grossProfit": "=subtract(sum($.revenues[*]), sum($.costOfGoods[*]))",
    "netIncome": "=subtract(subtract(sum($.revenues[*]), sum($.expenses[*])), $.taxes)",
    "ebitda": "=subtract($.netIncome, subtract($.interest, $.depreciation))"
  },
  "command": "add"
}
```

### Budget vs Actual Analysis
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.budget[*].amount"))
    .OnPath("$.totalBudget")
    .Add(SumBuilders.Sum("$.actual[*].amount"))
    .OnPath("$.totalActual")
    .Add(SubtractBuilders.Subtract("$.totalActual", "$.totalBudget"))
    .OnPath("$.variance")
    .Add(CalculateBuilders.Calculate("$.variance / $.totalBudget * 100"))
    .OnPath("$.variancePercent");
```

### Inventory Management
```json
[
  {
    "path": "$.inventory.startingBalance",
    "value": "=sum($.openingStock[*].quantity)",
    "command": "add"
  },
  {
    "path": "$.inventory.received",
    "value": "=sum($.receipts[*].quantity)",
    "command": "add"
  },
  {
    "path": "$.inventory.shipped",
    "value": "=sum($.shipments[*].quantity)",
    "command": "add"
  },
  {
    "path": "$.inventory.endingBalance",
    "value": "=subtract(add($.inventory.startingBalance, $.inventory.received), $.inventory.shipped)",
    "command": "add"
  }
]
```

## Related Functions

- **[sum](sum.md)**: Addition operation (opposite of subtract)
- **[abs](abs.md)**: Absolute value (useful for absolute differences)
- **[min](min.md)**: Find minimum (subtract can create negative values)
- **[max](max.md)**: Find maximum
- **[avg](avg.md)**: Calculate averages
- **[calculate](calculate.md)**: Complex expressions including subtraction

## Mathematical Properties

### Arithmetic Properties
- **Not commutative**: `subtract(a, b) ? subtract(b, a)` (unless a = b)
- **Not associative**: `subtract(subtract(a, b), c) ? subtract(a, subtract(b, c))`
- **Identity element**: `subtract(a, 0) = a`
- **Inverse operation**: `subtract(a, b) = add(a, -b)`

### Relationship to Addition
```json
// These are equivalent:
"=subtract(a, b)"
"=sum(a, -b)"  // If negative values supported
```

## Best Practices

### Order Matters
```csharp
// Be explicit about operand order
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.revenue", "$.expenses"))  // Revenue - Expenses
    .OnPath("$.profit");

// Not the same as:
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.expenses", "$.revenue"))  // Expenses - Revenue (negative profit)
    .OnPath("$.loss");
```

### Handle Negative Results
```json
{
  "path": "$.analysis",
  "value": {
    "difference": "=subtract($.current, $.previous)",
    "isImprovement": "=subtract($.current, $.previous) > 0",
    "absoluteChange": "=abs(subtract($.current, $.previous))"
  },
  "command": "add"
}
```

### Financial Precision
```csharp
var script = new JLioScript()
    .Add(RoundBuilders.Round("subtract($.revenue, $.expenses)", "2"))
    .OnPath("$.netProfit");  // Round to 2 decimal places for currency
```

## Advanced Usage Patterns

### Multi-Step Calculations
```json
{
  "path": "$.complexCalculation",
  "value": {
    "step1": "=subtract($.grossSales, $.returns)",
    "step2": "=subtract($.step1, $.discounts)",
    "step3": "=subtract($.step2, $.taxes)",
    "finalNet": "=$.step3"
  },
  "command": "add"
}
```

### Conditional Subtraction
```json
{
  "path": "$.adjustedAmount",
  "command": "ifElse",
  "condition": "$.hasDiscount == true",
  "then": [
    {
      "path": "$.result",
      "value": "=subtract($.baseAmount, $.discountAmount)",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result",
      "value": "$.baseAmount",
      "command": "add"
    }
  ]
}
```

### Time-Based Calculations
```csharp
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.endTime", "$.startTime"))
    .OnPath("$.duration")
    .Add(SubtractBuilders.Subtract("$.currentValue", "$.baselineValue"))
    .OnPath("$.change")
    .Add(SubtractBuilders.Subtract("$.targetDate", "$.currentDate"))
    .OnPath("$.daysRemaining");
```

### Percentage Change Calculation
```json
{
  "path": "$.performanceMetrics",
  "value": {
    "absoluteChange": "=subtract($.current, $.previous)",
    "percentageChange": "=subtract($.current, $.previous) / $.previous * 100",
    "isPositiveChange": "=subtract($.current, $.previous) > 0"
  },
  "command": "add"
}
```

## Common Pitfalls

### Operand Order Confusion
```json
// Clear: Revenue minus expenses = profit
"=subtract($.revenue, $.expenses)"  // ? Positive profit

// Confusing: Expenses minus revenue = negative profit
"=subtract($.expenses, $.revenue)"  // ? Negative value (might be intended)
```

### Null Handling
```csharp
// Handle potential null values
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.value1 ?? 0", "$.value2 ?? 0"))
    .OnPath("$.safeDifference");
```

### Precision in Financial Calculations
```json
{
  "path": "$.financialResults",
  "value": {
    // Good: Round final result
    "netAmount": "=round(subtract($.gross, $.deductions), 2)",
    // Avoid: Multiple intermediate rounding
    "inefficient": "=subtract(round($.gross, 2), round($.deductions, 2))"
  },
  "command": "add"
}
```

### Large Number Precision
```csharp
// Be aware of floating point precision with large numbers
var script = new JLioScript()
    .Add(SubtractBuilders.Subtract("$.largeNumber1", "$.largeNumber2"))
    .OnPath("$.difference");
    // Consider rounding if precision issues occur
```