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
      {"name": "processingTime", "path": "@.processing.time"}
    ],
    "rules": [
      {
        "priority": 1,
        "conditions": {
          "total": ">=50",
          "customerType": "premium"
        },
        "results": {
          "shippingCost": 0,
          "processingTime": "1 day"
        }
      },
      {
        "priority": 2,
        "conditions": {
          "total": ">=100"
        },
        "results": {
          "shippingCost": 0,
          "processingTime": "2 days"
        }
      },
      {
        "priority": 3,
        "conditions": {
          "itemCount": ">10"
        },
        "results": {
          "shippingCost": 5,
          "processingTime": "3 days"
        }
      }
    ],
    "defaultResults": {
      "shippingCost": 10,
      "processingTime": "5 days"
    },
    "executionStrategy": {
      "mode": "firstMatch",
      "conflictResolution": "priority",
      "stopOnError": false
    }
  }
}
```

### Merge Strategy Example
```json
{
  "command": "decisionTable",
  "path": "$.user",
  "decisionTable": {
    "inputs": [
      {"name": "score", "path": "@.score", "type": "number"},
      {"name": "level", "path": "@.level", "type": "string"}
    ],
    "outputs": [
      {"name": "badges", "path": "@.badges"},
      {"name": "points", "path": "@.bonusPoints"}
    ],
    "rules": [
      {
        "conditions": {"score": ">1000"},
        "results": {
          "badges": ["high_scorer"],
          "points": 100
        }
      },
      {
        "conditions": {"level": "premium"},
        "results": {
          "badges": ["premium_member"],
          "points": 50
        }
      }
    ],
    "executionStrategy": {
      "mode": "allMatches",
      "conflictResolution": "merge"
    }
  }
}
```

## Function Integration

Decision tables support function expressions in results:

```json
{
  "rules": [
    {
      "conditions": {"status": "new"},
      "results": {
        "id": "=newGuid()",
        "timestamp": "=datetime(UTC)",
        "welcomeMessage": "=concat('Welcome ', @.name, '!')"
      }
    }
  ]
}
```

## Validation and Error Handling

### Required Configuration Validation
```csharp
// Missing path
// Log: "Path property for decisionTable command is missing"

// Missing decision table config
// Log: "DecisionTable property for decisionTable command is missing"

// Missing inputs
// Log: "DecisionTable inputs are required"

// Missing outputs  
// Log: "DecisionTable outputs are required"

// Missing rules
// Log: "DecisionTable rules are required"
```

### Runtime Error Handling
```csharp
// Input evaluation errors are logged as warnings
// Output application errors are logged as errors
// Execution can continue or stop based on stopOnError setting
```

### Execution Context Logging
```csharp
// Input evaluation: "Input 'age' evaluated to: 25"
// Rule matching: "Best matching rule selected with score: 150 (Priority: 1)"
// Output application: "Output 'category' set to: adult at path: @.category"
// Completion: "DecisionTable executed for path: $.customer"
```

## Performance Considerations

### Rule Evaluation Order
- **Priority Sorting**: Rules sorted by priority for firstMatch mode
- **Document Order**: Used as secondary sort criteria
- **Best Match Scoring**: Requires evaluation of all matching rules

### Condition Complexity
- **Simple Conditions**: Fast equality and comparison checks
- **Complex Expressions**: AND/OR logic requires more processing
- **Array Membership**: Efficient for reasonable array sizes

### Input Path Evaluation
- **Relative Paths**: Generally faster than absolute paths
- **Complex JSONPath**: May require full tree traversal
- **Missing Inputs**: Handled gracefully with null values

## Advanced Features

### Custom Type Conversion
Input types are automatically converted:

```csharp
// Supported types: string, number, boolean, array, object
// Automatic conversion from JToken values
// Resilient decimal parsing for numbers
```

### Cultural Number Parsing
Handles different decimal separators and number formats:

```csharp
// Supports both comma and dot decimal separators
// Culture-aware parsing with fallback strategies
// Graceful handling of malformed numbers
```

### Best Match Scoring Algorithm
Calculates rule specificity:

```csharp
// Score = (conditions matched * 100) + priority
// Higher scores indicate more specific matches
// Used in bestMatch execution mode
```

## Fluent API Examples

### Simple Decision Table
```csharp
var config = new DecisionTableConfig
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
            Conditions = new Dictionary<string, JToken> { {"age", ">=18"} },
            Results = new Dictionary<string, IFunctionSupportedValue> { {"category", "adult"} }
        }
    }
};

var script = new JLioScript()
    .DecisionTable("$.users[*]")
    .With(config);
```

### Combined with Other Commands
```csharp
var script = new JLioScript()
    .Add(new JValue("pending"))
    .OnPath("$.status")
    .DecisionTable("$.order")
    .With(orderProcessingRules)
    .Set(new JValue("=datetime()"))
    .OnPath("$.processedAt");
```

## Use Cases

### Business Rule Engine
- Customer categorization based on purchase history
- Pricing tier determination
- Discount calculation rules
- Approval workflow logic

### Data Validation and Enrichment
- Adding calculated fields based on existing data
- Standardizing data formats
- Applying business logic transformations
- Conditional data population

### Workflow Automation
- Status transitions based on conditions
- Task assignment rules
- Notification trigger logic
- Process routing decisions

## Best Practices

1. **Design Rules Carefully**: Ensure rules are mutually exclusive when using firstMatch
2. **Use Appropriate Priorities**: Lower numbers = higher priority for clear precedence
3. **Test Edge Cases**: Verify behavior with missing inputs and boundary conditions
4. **Choose Right Execution Mode**: Consider data requirements when selecting strategy
5. **Validate Inputs**: Ensure input paths exist and contain expected data types
6. **Handle Defaults**: Provide defaultResults for unmatched cases
7. **Monitor Performance**: Complex conditions and large rule sets may impact performance
8. **Log Appropriately**: Use execution context logging for debugging and monitoring
9. **Document Rules**: Maintain clear documentation of business logic in rules
10. **Version Control**: Track changes to decision table configurations over time