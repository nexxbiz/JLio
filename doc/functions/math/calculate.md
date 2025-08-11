# calculate Function

Evaluate complex mathematical expressions with variable substitution.

## Overview

The `calculate` function evaluates mathematical expressions provided as strings, supporting arithmetic operations, mathematical functions, and **JSONPath variable substitution** using double curly braces `{{$.path}}`. This is the primary function for complex calculations involving multiple JSON values.

## Syntax

### Expression Format
```json
"=calculate('expression with {{$.jsonPath}} variables')"
```

### Builder Pattern
```csharp
CalculateBuilders.Calculate(expression)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| expression | string | Yes | Mathematical expression string with optional `{{$.path}}` variable substitutions |

## Variable Substitution

### JSONPath Variables
Use **double curly braces** to substitute JSON values into expressions:

```json
"=calculate('{{$.value1}} + {{$.value2}}')"
```

### Supported JSONPath Patterns
```json
"{{$.property}}"                    // Simple property
"{{$.nested.property}}"             // Nested object
"{{$.array[0]}}"                    // Array element
"{{$.users[2].age}}"                // Complex nested access
"{{$.data.measurements.length}}"    // Deep nesting
```

## Examples

### Basic Arithmetic with Variables
```json
{
  "path": "$.total",
  "value": "=calculate('{{$.price}} + {{$.tax}} + {{$.shipping}}')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "price": 100,
  "tax": 8.50,
  "shipping": 15.25
}
```

**Result**: `123.75`

### Division Operations  
```json
{
  "path": "$.ratio", 
  "value": "=calculate('{{$.totalItems}} / {{$.batchSize}}')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "totalItems": 157,
  "batchSize": 25
}
```

**Result**: `6.28`

For ceiling operation, use a two-step approach:
```json
[
  {
    "path": "$.ratio",
    "value": "=calculate('{{$.totalItems}} / {{$.batchSize}}')",
    "command": "add"
  },
  {
    "path": "$.batchesNeeded",
    "value": "=ceiling($.ratio)",
    "command": "add"
  }
]
```

**Result**: `$.ratio = 6.28`, `$.batchesNeeded = 7`

### Complex Business Logic
```json
{
  "path": "$.profitAnalysis",
  "value": {
    "grossProfit": "=calculate('{{$.revenue}} - {{$.cogs}}')",
    "grossMargin": "=calculate('({{$.revenue}} - {{$.cogs}}) / {{$.revenue}} * 100')",
    "netProfit": "=calculate('{{$.revenue}} - {{$.cogs}} - {{$.expenses}}')",
    "roi": "=calculate('{{$.netProfit}} / {{$.investment}} * 100')"
  },
  "command": "add"
}
```

### Nested Object Access
```json
{
  "path": "$.calculations",
  "value": "=calculate('{{$.user.age}} + 5')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {
    "name": "John",
    "age": 25
  }
}
```

**Result**: `30`

### Array Element Operations
```json
{
  "path": "$.arrayCalculations",
  "value": {
    "firstTwo": "=calculate('{{$.numbers[0]}} + {{$.numbers[1]}}')",
    "product": "=calculate('{{$.prices[2]}} * {{$.quantities[2]}}')",
    "average": "=calculate('({{$.scores[0]}} + {{$.scores[1]}} + {{$.scores[2]}}) / 3')"
  },
  "command": "add"
}
```

**Input Data**:
```json
{
  "numbers": [10, 15, 20],
  "prices": [5.99, 7.50, 12.25],
  "quantities": [2, 3, 4],
  "scores": [84, 92, 79]
}
```

**Results**: 
- `firstTwo`: `25`
- `product`: `49` 
- `average`: `85`

### Deep Nested Access
```json
{
  "path": "$.result",
  "value": "=calculate('{{$.company.departments[0].budget}} + {{$.company.departments[1].budget}}')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "company": {
    "departments": [
      {"name": "IT", "budget": 50000},
      {"name": "HR", "budget": 30000}
    ]
  }
}
```

**Result**: `80000`

## Mathematical Functions  
The `calculate` function uses .NET's `DataTable.Compute()` method which supports:

- **Basic Arithmetic**: `+`, `-`, `*`, `/`, `%`
- **Parentheses**: `()` for grouping
- **Comparison Operators**: `<`, `>`, `<=`, `>=`, `=`, `<>`
- **Logical Operators**: `AND`, `OR`, `NOT`

**? Advanced mathematical functions are NOT supported:**
- `sqrt()`, `pow()`, `sin()`, `cos()`, `ceiling()`, `floor()`, etc.

For complex mathematical functions, use a **multi-step approach**:

```json
[
  {
    "path": "$.intermediate",
    "value": "=calculate('{{$.a}} / {{$.b}}')",
    "command": "add"
  },
  {
    "path": "$.result",
    "value": "=ceiling($.intermediate)",
    "command": "add"
  }
]
```

## Builder Pattern Usage

### Simple Calculation
```csharp
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.revenue}} - {{$.expenses}}"))
    .OnPath("$.profit");
```

### Complex Multi-Step Analysis
```csharp
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.totalItems}} / {{$.batchSize}}"))
    .OnPath("$.batchRatio")
    .Add(CalculateBuilders.Calculate("{{$.batchRatio}}"))  // Simple variable reference
    .OnPath("$.processedRatio")
    .Add(CalculateBuilders.Calculate("{{$.processedRatio}} * {{$.costPerBatch}}"))
    .OnPath("$.totalCost");
```

### Financial Modeling
```csharp
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("1 + {{$.rate}}"))
    .OnPath("$.rateBase")
    .Add(CalculateBuilders.Calculate("{{$.principal}} * {{$.years}} * {{$.rateBase}}"))  // Simplified calculation
    .OnPath("$.futureValue")
    .Add(CalculateBuilders.Calculate("{{$.futureValue}} - {{$.principal}}"))
    .OnPath("$.totalInterest");
```

## Supported Operations

### Basic Arithmetic
- **Addition**: `+`
- **Subtraction**: `-` 
- **Multiplication**: `*`
- **Division**: `/`
- **Modulo**: `%`

### Advanced Operations
- **Exponentiation**: `^` or `**` (may not be supported)
- **Parentheses**: `()` for grouping
- **Negative numbers**: `-5`

### Mathematical Functions  
The `calculate` function uses .NET's `DataTable.Compute()` method which supports:

- **Basic Arithmetic**: `+`, `-`, `*`, `/`, `%`
- **Parentheses**: `()` for grouping
- **Comparison Operators**: `<`, `>`, `<=`, `>=`, `=`, `<>`
- **Logical Operators**: `AND`, `OR`, `NOT`

**? Advanced mathematical functions are NOT supported:**
- `sqrt()`, `pow()`, `sin()`, `cos()`, `ceiling()`, `floor()`, etc.

For complex mathematical functions, use a **multi-step approach**:

```json
[
  {
    "path": "$.intermediate",
    "value": "=calculate('{{$.a}} / {{$.b}}')",
    "command": "add"
  },
  {
    "path": "$.result",
    "value": "=ceiling($.intermediate)",
    "command": "add"
  }
]
```

### Decimal Notation
Supports both period and comma decimal separators:
```json
{
  "path": "$.decimals",
  "value": {
    "periodNotation": "=calculate('{{$.value1}} + 3.14')",      // US/UK style
    "commaNotation": "=calculate('{{$.value2}} + 3,14')"       // European style  
  },
  "command": "add"
}
```

## Error Handling

### Common Errors
```json
// Correct usage
"=calculate('{{$.a}} + {{$.b}}')"           // ? Variables with arithmetic
"=calculate('({{$.x}} + {{$.y}}) * 2')"     // ? Complex expression

// Errors
"=calculate()"                              // ? Missing expression
"=calculate('{{$.missing}} + 5')"           // ? Non-existent property
"=calculate('{{$.array[*]}} + 1')"          // ? Multi-token JSONPath
"=calculate('2 + +')"                       // ? Invalid syntax
"=calculate('{{$.value}} / 0')"             // ? Division by zero
"=calculate('sqrt({{$.value}})')"           // ? sqrt function not supported
"=calculate('pow({{$.a}}, {{$.b}})')"       // ? pow function not supported
```

### Error Messages
- **Missing arguments**: "calculate requires exactly one argument"
- **JSONPath errors**: "Property not found: $.missing"
- **Multi-token errors**: "JSONPath returned multiple values"
- **Syntax errors**: "Invalid mathematical expression"
- **Mathematical errors**: "Division by zero"

## Use Cases

### Business Calculations
1. **Financial Analysis**: Revenue, profit, margin calculations
2. **Resource Planning**: Capacity, allocation, optimization
3. **Pricing Models**: Dynamic pricing, discounts, taxes
4. **Performance Metrics**: KPIs, ratios, growth rates

### Engineering Applications
1. **Physics Calculations**: Force, energy, motion equations
2. **Statistical Analysis**: Variance, standard deviation, correlations
3. **Optimization Problems**: Cost functions, efficiency metrics
4. **Signal Processing**: Mathematical transformations

### E-commerce Applications
1. **Order Totals**: Price + tax + shipping calculations
2. **Inventory Management**: Stock levels, reorder points
3. **Discount Calculations**: Percentage discounts, bulk pricing
4. **Shipping Costs**: Weight-based, distance-based pricing

## Integration Examples

### E-commerce Order Processing
```json
{
  "path": "$.orderCalculations",
  "value": {
    "subtotal": "=calculate('{{$.quantity}} * {{$.unitPrice}}')",
    "discount": "=calculate('{{$.subtotal}} * {{$.discountRate}}')",
    "discountedTotal": "=calculate('{{$.subtotal}} - {{$.discount}}')",
    "tax": "=calculate('{{$.discountedTotal}} * {{$.taxRate}}')",
    "shipping": "=calculate('{{$.weight}} * {{$.shippingRate}}')",
    "grandTotal": "=calculate('{{$.discountedTotal}} + {{$.tax}} + {{$.shipping}}')"
  },
  "command": "add"
}
```

### Financial Dashboard
```json
{
  "path": "$.financialMetrics",
  "value": {
    "grossMargin": "=calculate('({{$.revenue}} - {{$.cogs}}) / {{$.revenue}} * 100')",
    "operatingMargin": "=calculate('({{$.revenue}} - {{$.cogs}} - {{$.opex}}) / {{$.revenue}} * 100')",
    "netMargin": "=calculate('{{$.netIncome}} / {{$.revenue}} * 100')",
    "roe": "=calculate('{{$.netIncome}} / {{$.shareholderEquity}} * 100')",
    "currentRatio": "=calculate('{{$.currentAssets}} / {{$.currentLiabilities}}')"
  },
  "command": "add"
}
```

### Inventory Management
```csharp
var script = new JLioScript()
    // Calculate reorder point
    .Add(CalculateBuilders.Calculate("{{$.leadTimeDays}} * {{$.dailyUsage}} + {{$.safetyStock}}"))
    .OnPath("$.reorderPoint")
    // Calculate simple order quantity (simplified from EOQ formula)
    .Add(CalculateBuilders.Calculate("{{$.annualDemand}} / {{$.orderFrequency}}"))
    .OnPath("$.basicOrderQuantity")
    // Calculate total inventory value
    .Add(CalculateBuilders.Calculate("{{$.quantity}} * {{$.unitCost}}"))
    .OnPath("$.inventoryValue");
```

## Performance Considerations

- **Variable Substitution**: Each `{{$.path}}` requires JSONPath resolution
- **Expression Complexity**: Complex expressions take longer to parse and evaluate
- **Error Handling**: Extensive validation for security and correctness
- **Memory Usage**: Variable substitution creates temporary string replacements

## Best Practices

### Error Prevention
```csharp
// Validate data exists before calculation
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.numerator}} / {{$.denominator}}"))
    .OnPath("$.ratio")
    .ConditionalOn("$.denominator != 0 && $.denominator != null");
```

### Complex Expression Management
```json
{
  "path": "$.managedCalculation",
  "value": {
    // Break complex calculations into steps
    "step1": "=calculate('{{$.value1}} * {{$.multiplier1}}')",
    "step2": "=calculate('{{$.value2}} * {{$.multiplier2}}')", 
    "step3": "=calculate('{{$.step1}} + {{$.step2}}')"
  },
  "command": "add"
}
```

**For mathematical functions, use individual function calls:**
```json
[
  {
    "path": "$.intermediate",
    "value": "=calculate('{{$.step3}}')",
    "command": "add"
  },
  {
    "path": "$.final",
    "value": "=sqrt($.intermediate)",
    "command": "add"
  }
]
```

### Precision for Financial Calculations
```json
{
  "path": "$.financialResults",
  "value": {
    "calculation": "=calculate('{{$.principal}} * {{$.rate}} * {{$.time}}')"
  },
  "command": "add"
}
```

**For rounding, use the round function separately:**
```json
[
  {
    "path": "$.rawCalculation",
    "value": "=calculate('{{$.principal}} * {{$.rate}} * {{$.time}}')",
    "command": "add"
  },
  {
    "path": "$.rounded",
    "value": "=round($.rawCalculation, 2)",
    "command": "add"
  }
]
```

## Advanced Patterns

### Conditional Calculations
```json
[
  {
    "path": "$.discountRate",
    "command": "ifElse", 
    "condition": "$.orderAmount > 1000",
    "then": [{"path": "$.rate", "value": 0.15, "command": "add"}],
    "else": [{"path": "$.rate", "value": 0.05, "command": "add"}]
  },
  {
    "path": "$.finalPrice",
    "value": "=calculate('{{$.orderAmount}} * (1 - {{$.discountRate.rate}})')",
    "command": "add"
  }
]
```

### Multi-Currency Calculations
```json
{
  "path": "$.currencyCalculations",
  "value": {
    "usdAmount": "=calculate('{{$.localAmount}} / {{$.exchangeRate}}')",
    "withFees": "=calculate('{{$.usdAmount}} * (1 + {{$.conversionFee}})')"
  },
  "command": "add"
}
```

**For rounding, use separate round function:**
```json
[
  {
    "path": "$.rawAmount",
    "value": "=calculate('{{$.usdAmount}} * (1 + {{$.conversionFee}})')",
    "command": "add"
  },
  {
    "path": "$.rounded",
    "value": "=round($.rawAmount, 2)",
    "command": "add"
  }
]
```

### Statistical Calculations  
For statistical calculations requiring mathematical functions, use a **multi-step approach**:

```json
[
  {
    "path": "$.statistics.mean",
    "value": "=calculate('({{$.data[0]}} + {{$.data[1]}} + {{$.data[2]}} + {{$.data[3]}}) / 4')",
    "command": "add"
  },
  {
    "path": "$.deviation0",
    "value": "=calculate('{{$.data[0]}} - {{$.statistics.mean}}')",
    "command": "add"
  },
  {
    "path": "$.deviationSquared0",
    "value": "=pow($.deviation0, 2)",
    "command": "add"
  },
  {
    "path": "$.statistics.variance",
    "value": "=calculate('({{$.deviationSquared0}} + {{$.deviationSquared1}} + {{$.deviationSquared2}} + {{$.deviationSquared3}}) / 4')",
    "command": "add"
  },
  {
    "path": "$.statistics.stdDev",
    "value": "=sqrt($.statistics.variance)",
    "command": "add"
  }
]
```

## Related Functions

### Individual Math Functions
When you need simple operations, consider using dedicated functions:
- **[ceiling](ceiling.md)**: For simple ceiling of single values
- **[floor](floor.md)**: For simple floor of single values  
- **[round](round.md)**: For simple rounding of single values
- **[sum](sum.md)**: For adding multiple values or arrays
- **[avg](avg.md)**: For averaging multiple values or arrays

### When to Use Calculate vs Individual Functions

**Use Calculate when:**
- You need arithmetic expressions (`a + b`, `a / b`, etc.)
- You need complex multi-step expressions with basic arithmetic
- You need variable substitution with arithmetic

**Use Individual Functions when:**
- You have simple single-value operations
- You need mathematical functions (sqrt, pow, ceiling, floor, etc.)
- You want better performance
- You're working with arrays or multiple arguments

```json
{
  "path": "$.comparison", 
  "value": {
    // Use individual functions for simple operations
    "simpleSum": "=sum($.a, $.b, $.c)",
    "simpleCeiling": "=ceiling($.value)",
    
    // Use calculate for arithmetic expressions only
    "arithmeticExpression": "=calculate('{{$.a}} / {{$.b}} * {{$.c}}')"
  },
  "command": "add"
}
```

**? Don't mix mathematical functions with calculate:**
```json
{
  "path": "$.incorrect",
  "value": "=calculate('ceiling({{$.a}} / {{$.b}}) * {{$.c}}')",  // ? Won't work
  "command": "add"
}
```

**? Use multi-step approach instead:**
```json
[
  {
    "path": "$.ratio",
    "value": "=calculate('{{$.a}} / {{$.b}}')",
    "command": "add"
  },
  {
    "path": "$.ceilingValue",
    "value": "=ceiling($.ratio)",
    "command": "add"
  },
  {
    "path": "$.result",
    "value": "=calculate('{{$.ceilingValue}} * {{$.c}}')",
    "command": "add"
  }
]