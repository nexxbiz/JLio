# IfElse Command Documentation

## Overview

The `IfElse` command provides conditional execution logic by comparing two values and executing different script branches based on the comparison result. When the values are equal, the "if" script executes; otherwise, the "else" script executes. This enables dynamic, data-driven transformation workflows.

## Syntax

### JSON Script Format
```json
{
  "first": "$.value1",
  "second": 200,
  "ifScript": [
    {
      "path": "$.result",
      "value": "condition met",
      "command": "add"
    }
  ],
  "elseScript": [
    {
      "path": "$.result", 
      "value": "condition not met",
      "command": "add"
    }
  ],
  "command": "ifElse"
}
```

### Required Properties
- **first**: First value for comparison (can be literal value, JSONPath, or function expression)
- **second**: Second value for comparison (can be literal value, JSONPath, or function expression)
- **ifScript**: Script to execute when first equals second
- **command**: Must be "ifElse"

### Optional Properties
- **elseScript**: Script to execute when first does not equal second (optional)

## Programmatic Usage

### Constructor and Configuration
```csharp
var ifElseCommand = new IfElse
{
    First = new FunctionSupportedValue(new FixedValue(new JValue("$.revenue"), functionConverter)),
    Second = new FunctionSupportedValue(new FixedValue(new JValue(1000), functionConverter)),
    IfScript = highRevenueScript,
    ElseScript = lowRevenueScript
};
```

### Helper Method for Values
```csharp
private IFunctionSupportedValue Val(JToken v) => 
    new FunctionSupportedValue(new FixedValue(v, functionConverter));

var command = new IfElse
{
    First = Val(new JValue("$.status")),
    Second = Val(new JValue("active")),
    IfScript = activeUserScript,
    ElseScript = inactiveUserScript
};
```

## Comparison Logic

### Equality Check
The IfElse command uses `JToken.DeepEquals()` for comparison, which means:

- **Exact Value Match**: Values must be identical in both value and type
- **Deep Object Comparison**: Objects and arrays are compared recursively
- **Type Sensitive**: "1" (string) is not equal to 1 (number)

### Comparison Examples
```json
// String comparison
"first": "$.userType", "second": "premium"     // Matches if userType is exactly "premium"

// Number comparison  
"first": "$.amount", "second": 1000            // Matches if amount is exactly 1000

// Boolean comparison
"first": "$.isActive", "second": true          // Matches if isActive is exactly true

// Object comparison
"first": "$.config", "second": {"enabled": true}  // Matches if config object is exactly {"enabled": true}
```

## Examples

### Basic Conditional Logic
```json
{
  "first": "$.user.status",
  "second": "active",
  "ifScript": [
    {
      "path": "$.permissions",
      "value": ["read", "write", "admin"],
      "command": "add"
    }
  ],
  "elseScript": [
    {
      "path": "$.permissions", 
      "value": ["read"],
      "command": "add"
    }
  ],
  "command": "ifElse"
}
```

**Input Data**:
```json
{
  "user": {
    "id": 123,
    "status": "active"
  }
}
```

**Result**:
```json
{
  "user": {
    "id": 123,
    "status": "active"
  },
  "permissions": ["read", "write", "admin"]
}
```

### Revenue-Based Processing
```json
{
  "first": "$.order.total",
  "second": 1000,
  "ifScript": [
    {
      "path": "$.order.tier",
      "value": "premium",
      "command": "add"
    },
    {
      "path": "$.order.discount",
      "value": 0.15,
      "command": "add"
    }
  ],
  "elseScript": [
    {
      "path": "$.order.tier",
      "value": "standard", 
      "command": "add"
    },
    {
      "path": "$.order.discount",
      "value": 0.05,
      "command": "add"
    }
  ],
  "command": "ifElse"
}
```

### Function-Based Comparisons
```json
{
  "first": "=fetch($.user.subscriptionType)",
  "second": "enterprise",
  "ifScript": [
    {
      "path": "$.features",
      "value": "=fetch($.enterpriseFeatures)",
      "command": "add"
    }
  ],
  "elseScript": [
    {
      "path": "$.features",
      "value": "=fetch($.standardFeatures)", 
      "command": "add"
    }
  ],
  "command": "ifElse"
}
```

### Nested Conditional Scripts
```json
{
  "first": "$.customer.type",
  "second": "vip",
  "ifScript": [
    {
      "first": "$.order.amount",
      "second": 5000,
      "ifScript": [
        {
          "path": "$.discount",
          "value": 0.25,
          "command": "add"
        }
      ],
      "elseScript": [
        {
          "path": "$.discount",
          "value": 0.15,
          "command": "add"
        }
      ],
      "command": "ifElse"
    }
  ],
  "elseScript": [
    {
      "path": "$.discount",
      "value": 0.05,
      "command": "add"
    }
  ],
  "command": "ifElse"
}
```

### Complex Object Comparison
```json
{
  "first": "$.config.database",
  "second": {"host": "localhost", "port": 5432},
  "ifScript": [
    {
      "path": "$.environment",
      "value": "development",
      "command": "add"
    }
  ],
  "elseScript": [
    {
      "path": "$.environment",
      "value": "production",
      "command": "add"
    }
  ],
  "command": "ifElse"
}
```

## Programmatic Usage Examples

### Basic Revenue Check
```csharp
var data = JObject.Parse("{\"revenue\": 200, \"result\": 0}");

var ifScript = new JLioScript()
    .Set(new JValue("high"))
    .OnPath("$.category");

var elseScript = new JLioScript()
    .Set(new JValue("low"))
    .OnPath("$.category");

var command = new IfElse
{
    First = Val(new JValue("$.revenue")),
    Second = Val(new JValue(1000)),
    IfScript = ifScript,
    ElseScript = elseScript
};

var script = new JLioScript();
script.AddLine(command);
var result = script.Execute(data, context);
```

### User Status Processing
```csharp
var ifScript = new JLioScript()
    .Add(new JValue("premium"))
    .OnPath("$.accessLevel")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.lastAccess");

var elseScript = new JLioScript()
    .Add(new JValue("basic"))
    .OnPath("$.accessLevel")
    .Remove("$.sensitiveData");

var command = new IfElse
{
    First = Val(new JValue("$.user.subscription")),
    Second = Val(new JValue("premium")),
    IfScript = ifScript,
    ElseScript = elseScript
};
```

## Integration Examples

### User Onboarding Flow
```csharp
var script = new JLioScript()
    .Add(new IfElse
    {
        First = Val(new JValue("$.user.isNewUser")),
        Second = Val(new JValue(true)),
        IfScript = new JLioScript()
            .Add(new JValue("welcome"))
            .OnPath("$.emailTemplate")
            .Add(new JValue(["tutorial", "tips"]))
            .OnPath("$.assignedContent"),
        ElseScript = new JLioScript()
            .Add(new JValue("update"))
            .OnPath("$.emailTemplate")
            .Add(FetchBuilders.Fetch("$.user.preferences.content"))
            .OnPath("$.assignedContent")
    });
```

### API Response Formatting
```csharp
var script = new JLioScript()
    .Add(new IfElse
    {
        First = Val(new JValue("$.response.status")),
        Second = Val(new JValue("success")),
        IfScript = new JLioScript()
            .Add(FetchBuilders.Fetch("$.response.data"))
            .OnPath("$.result")
            .Add(new JValue(200))
            .OnPath("$.httpStatus"),
        ElseScript = new JLioScript()
            .Add(FetchBuilders.Fetch("$.response.error"))
            .OnPath("$.result")
            .Add(new JValue(400))
            .OnPath("$.httpStatus")
    });
```

### Configuration-Based Processing
```csharp
var script = new JLioScript()
    .Add(new IfElse
    {
        First = Val(new JValue("$.config.environment")),
        Second = Val(new JValue("production")),
        IfScript = new JLioScript()
            .Set(new JValue("error"))
            .OnPath("$.logging.level")
            .Set(new JValue(false))
            .OnPath("$.debugging.enabled"),
        ElseScript = new JLioScript()
            .Set(new JValue("debug"))
            .OnPath("$.logging.level")
            .Set(new JValue(true))
            .OnPath("$.debugging.enabled")
    });
```

## Validation and Error Handling

### Required Properties Validation
```csharp
// Missing First property
// Log: "First property for ifElse command is missing"

// Missing Second property  
// Log: "Second property for ifElse command is missing"

// Missing IfScript property
// Log: "IfScript property for ifElse command is missing"
```

### Optional ElseScript
If no ElseScript is provided and the condition is false, the command succeeds without performing any operations.

### Script Execution Errors
If either the IfScript or ElseScript fails during execution, the IfElse command returns the failure result.

## Use Cases

### User Access Control
```json
// Grant different permissions based on user role
{
  "first": "$.user.role",
  "second": "admin",
  "ifScript": [{"path": "$.permissions", "value": "all", "command": "add"}],
  "elseScript": [{"path": "$.permissions", "value": "read", "command": "add"}],
  "command": "ifElse"
}
```

### Environment-Specific Configuration
```json
// Different settings for different environments
{
  "first": "$.deployment.environment", 
  "second": "production",
  "ifScript": [{"path": "$.config.debug", "value": false, "command": "add"}],
  "elseScript": [{"path": "$.config.debug", "value": true, "command": "add"}],
  "command": "ifElse"
}
```

### Pricing Logic
```json
// Different pricing based on customer tier
{
  "first": "$.customer.tier",
  "second": "enterprise",
  "ifScript": [{"path": "$.pricing.discount", "value": 0.20, "command": "add"}],
  "elseScript": [{"path": "$.pricing.discount", "value": 0.10, "command": "add"}],
  "command": "ifElse"
}
```

### Feature Flags
```json
// Enable features based on configuration
{
  "first": "$.features.advancedMode",
  "second": true,
  "ifScript": [{"path": "$.ui.components", "value": "=fetch($.advancedComponents)", "command": "add"}],
  "elseScript": [{"path": "$.ui.components", "value": "=fetch($.basicComponents)", "command": "add"}],
  "command": "ifElse"
}
```

## Performance Considerations

- **Value Evaluation**: Both first and second values are evaluated regardless of the result
- **Script Compilation**: Only the relevant script (if or else) is executed
- **Deep Comparison**: Object comparisons require recursive traversal
- **Memory Usage**: Minimal overhead for simple value comparisons

## Best Practices

1. **Clear Conditions**: Use meaningful value comparisons that clearly express business logic
2. **Simple Scripts**: Keep if/else scripts focused and avoid overly complex nested logic
3. **Default Handling**: Always provide an elseScript for complete condition coverage
4. **Type Consistency**: Ensure compared values have consistent types to avoid unexpected mismatches
5. **Performance Awareness**: Consider using Decision Tables for complex multi-condition logic
6. **Testing**: Test both if and else branches thoroughly
7. **Documentation**: Document the business logic represented by each condition

## Comparison with Decision Tables

### Use IfElse When:
- Simple binary conditions (equal/not equal)
- Two-branch logic paths
- Direct value comparisons
- Embedded conditional logic within larger scripts

### Use Decision Tables When:
- Multiple conditions and inputs
- Complex rule matrices
- Business rules that change frequently
- Multiple possible outcomes based on combinations of conditions

## Common Patterns

### Status-Based Processing
```json
{
  "first": "$.status",
  "second": "active",
  "ifScript": [...],
  "elseScript": [...],
  "command": "ifElse"
}
```

### Threshold-Based Logic
```json
{
  "first": "$.value",
  "second": 100,
  "ifScript": [...],
  "elseScript": [...], 
  "command": "ifElse"
}
```

### Type-Based Routing
```json
{
  "first": "$.type",
  "second": "premium",
  "ifScript": [...],
  "elseScript": [...],
  "command": "ifElse"
}
```