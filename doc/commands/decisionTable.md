# Decision Table Command Documentation

## Overview

The `DecisionTable` command implements rule-based decision logic for JSON data transformation. It evaluates input conditions against defined rules and applies corresponding outputs, making it ideal for business rule engines, conditional data transformations, and complex decision workflows.

## Syntax

### JSON Script Format
```json
{
  "path": "$.target.path",
  "command": "decisionTable",
  "decisionTable": {
    "inputs": [
      {
        "name": "inputName",
        "path": "@.inputPath",
        "type": "string"
      }
    ],
    "outputs": [
      {
        "name": "outputName", 
        "path": "@.outputPath"
      }
    ],
    "rules": [
      {
        "priority": 1,
        "conditions": {
          "inputName": ">=18"
        },
        "results": {
          "outputName": "adult"
        }
      }
    ],
    "defaultResults": {
      "outputName": "default_value"
    },
    "executionStrategy": {
      "mode": "firstMatch",
      "conflictResolution": "priority",
      "stopOnError": false
    }
  }
}
```

### Required Properties
- **path**: JSONPath expression targeting elements to evaluate
- **command**: Must be "decisionTable"
- **decisionTable**: Configuration object containing inputs, outputs, and rules

## Programmatic Usage

### Constructor and Configuration
```csharp
var decisionTable = new DecisionTable
{
    Path = "$.customer",
    DecisionTableConfig = new DecisionTableConfig
    {
        Inputs = new List<DecisionInput>
        {
            new DecisionInput { Name = "age", Path = "@.age", Type = "number" }
        },
        Outputs = new List<DecisionOutput>
        {
            new DecisionOutput { Name = "category", Path = "@.category" }
        },
        Rules = new List<DecisionRule>
        {
            new DecisionRule
            {
                Priority = 1,
                Conditions = new Dictionary<string, JToken> { {"age", ">=18"} },
                Results = new Dictionary<string, IFunctionSupportedValue> { {"category", "adult"} }
            }
        }
    }
};
```

### Fluent API
```csharp
var script = new JLioScript()
    .DecisionTable("$.customer")
    .With(decisionTableConfig);
```

## Configuration Components

### Inputs
Define what data to evaluate from each target element:

```json
{
  "name": "age",           // Input identifier used in conditions
  "path": "@.age",         // JSONPath relative to target element
  "type": "number",        // Data type (string, number, boolean, array, object)
  "transform": "=someFunc" // Optional transformation function
}
```

#### Path Types
- **Relative Paths** (`@.property`): Relative to current target element
- **Absolute Paths** (`$.property`): Relative to document root

### Outputs
Define where to place results:

```json
{
  "name": "category",      // Output identifier used in results
  "path": "@.category"     // JSONPath for result placement
}
```

### Rules
Define decision logic:

```json
{
  "priority": 1,           // Rule priority (lower numbers = higher priority)
  "conditions": {          // Input conditions to match
    "age": ">=18",
    "status": "active"
  },
  "results": {             // Outputs to apply when conditions match
    "category": "adult",
    "discount": 0.1
  }
}
```

### Execution Strategy
Control how rules are evaluated and applied:

```json
{
  "mode": "firstMatch",           // firstMatch, allMatches, bestMatch
  "conflictResolution": "priority", // priority, merge, lastWins
  "stopOnError": false            // Stop execution on first error
}
```

## Condition Evaluation

### Simple Equality
```json
{"status": "active"}        // Exact match
{"category": "premium"}     // String equality
```

### Comparison Operators
```json
{"age": ">=18"}            // Greater than or equal
{"score": ">100"}          // Greater than
{"price": "<=50"}          // Less than or equal
{"quantity": "<10"}        // Less than
{"status": "!=inactive"}   // Not equal
```

### Array Membership
```json
{"category": ["premium", "gold", "platinum"]}  // Must be one of these values
```

### Complex Conditions
```json
{"condition": "age >= 18 && status == 'active'"}           // AND logic
{"condition": "category == 'premium' || score > 1000"}     // OR logic
{"condition": "(age >= 18 && age <= 65) || status == 'vip'"} // Grouped conditions
```

## Execution Modes

### First Match (Default)
Applies the first rule that matches, based on priority:

```csharp
"executionStrategy": {
  "mode": "firstMatch",
  "conflictResolution": "priority"
}
```

### All Matches
Applies all rules that match:

```csharp
"executionStrategy": {
  "mode": "allMatches"
}
```

### Best Match
Finds the rule with the highest score (most specific match):

```csharp
"executionStrategy": {
  "mode": "bestMatch"
}
```

## Conflict Resolution

### Priority (Default)
Rules are sorted by priority field, then document order:

```csharp
"conflictResolution": "priority"
```

### Merge
Combines results from all matching rules:

```csharp
"conflictResolution": "merge"
```

#### Merge Behavior
- **Arrays**: Concatenated together
- **Numbers**: Maximum value taken
- **Other types**: Converted to arrays

### Last Wins
Only the last matching rule's results are applied:

```csharp
"conflictResolution": "lastWins"
```

## Examples

### Simple Age-Based Categorization
```json
{
  "command": "decisionTable",
  "path": "$.customer",
  "decisionTable": {
    "inputs": [
      {"name": "age", "path": "@.age", "type": "number"}
    ],
    "outputs": [
      {"name": "category", "path": "@.category"}
    ],
    "rules": [
      {
        "priority": 1,
        "conditions": {"age": ">=18"},
        "results": {"category": "adult"}
      },
      {
        "priority": 2,
        "conditions": {"age": "<18"},
        "results": {"category": "minor"}
      }
    ]
  }
}
```

### Multi-Input Price Tier Determination
```json
{
  "command": "decisionTable",
  "path": "$.product",
  "decisionTable": {
    "inputs": [
      {"name": "price", "path": "@.price", "type": "number"},
      {"name": "category", "path": "@.category", "type": "string"}
    ],
    "outputs": [
      {"name": "tier", "path": "@.tier"},
      {"name": "discount", "path": "@.discount"}
    ],
    "rules": [
      {
        "priority": 1,
        "conditions": {
          "price": ">1000",
          "category": "electronics"
        },
        "results": {
          "tier": "premium",
          "discount": 0.15
        }
      },
      {
        "priority": 2,
        "conditions": {"price": ">100"},
        "results": {
          "tier": "standard",
          "discount": 0.05
        }
      }
    ],
    "defaultResults": {
      "tier": "basic",
      "discount": 0
    }
  }
}
```

### Order Processing with Complex Conditions
```json
{
  "command": "decisionTable",
  "path": "$.order",
  "decisionTable": {
    "inputs": [
      {"name": "total", "path": "@.total", "type": "number"},
      {"name": "customerType", "path": "@.customer.type", "type": "string"},
      {"name": "itemCount", "path": "@.items.length", "type": "number"}
    ],
    "outputs": [
      {"name": "shippingCost", "path": "@.shipping.cost"},
      {"name": "processingTime", "path