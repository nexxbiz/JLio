# JLio Upgrade Guide: Moving to Engine-Based Architecture

## Overview

This guide covers the migration from JLio's singleton-based architecture to the new engine-based architecture that provides:

- **Multiple Version Support**: Run different JLio configurations side-by-side
- **Per-Execution Configuration**: Choose parse options and commands per execution
- **Eliminated Singletons**: Instance-based architecture for better control
- **Backward Compatibility**: Existing code continues to work unchanged

## What's Changing

### Current Architecture (Before)
```csharp
// Static singleton approach
var parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
var executionContext = ExecutionContext.CreateDefault();

// Static parsing and execution
var script = JLioConvert.Parse(scriptText, parseOptions);
var result = script.Execute(data, executionContext);
```

### New Architecture (After)
```csharp
// Instance-based engine approach
var engine = JLioEngineConfigurations.CreateLatest()
    .WithMathExtensions()
    .WithTextExtensions()
    .Build();

// Engine-based parsing and execution
var result = engine.ParseAndExecute(scriptText, data);
```

## Migration Strategies

### Strategy 1: No Changes Required (Recommended for Most Users)

If you're using the default configuration, **no code changes are needed**. The existing static methods remain available and work exactly as before:

```csharp
// This continues to work unchanged
var script = JLioConvert.Parse(scriptText);
var result = script.Execute(data);

// This also continues to work unchanged
var customOptions = ParseOptions.CreateDefault().RegisterMath();
var script = JLioConvert.Parse(scriptText, customOptions);
var result = script.Execute(data, ExecutionContext.CreateDefault());
```

### Strategy 2: Gradual Migration (Recommended for Advanced Users)

Gradually adopt the new engine approach while keeping existing code:

```csharp
// Phase 1: Create engines alongside existing code
public class DocumentProcessor
{
    private readonly JLioEngine legacyEngine;
    private readonly JLioEngine modernEngine;

    public DocumentProcessor()
    {
        // Keep using existing approach for legacy scripts
        legacyEngine = new JLioEngine(
            ParseOptions.CreateDefault(),
            ExecutionContext.CreateDefault()
        );

        // Use new approach for new features
        modernEngine = JLioEngineConfigurations.CreateLatest()
            .WithMathExtensions()
            .WithTextExtensions()
            .WithETLExtensions()
            .Build();
    }

    public JLioResult ProcessLegacy(string script, JToken data)
    {
        return legacyEngine.ParseAndExecute(script, data);
    }

    public JLioResult ProcessModern(string script, JToken data)
    {
        return modernEngine.ParseAndExecute(script, data);
    }
}
```

### Strategy 3: Full Migration (For New Projects)

Adopt the new engine architecture completely:

```csharp
public class JLioService
{
    private readonly Dictionary<string, JLioEngine> engines;

    public JLioService()
    {
        engines = new Dictionary<string, JLioEngine>
        {
            ["basic"] = JLioEngineConfigurations.CreateV1().Build(),
            ["standard"] = JLioEngineConfigurations.CreateV2()
                .WithMathExtensions()
                .Build(),
            ["full"] = JLioEngineConfigurations.CreateLatest()
                .WithAllExtensions()
                .Build(),
            ["custom"] = new JLioEngineBuilder()
                .WithCoreCommands()
                .WithFunction<CustomFunction>()
                .WithCommand<CustomCommand>()
                .Build()
        };
    }

    public JLioResult Execute(string script, JToken data, string engineType = "standard")
    {
        var engine = engines[engineType];
        return engine.ParseAndExecute(script, data);
    }
}
```

## New Features and Capabilities

### 1. Multiple Versions Side-by-Side

```csharp
// Different feature sets for different use cases
var basicEngine = JLioEngineConfigurations.CreateV1().Build();
var advancedEngine = JLioEngineConfigurations.CreateV3().Build();

// Process with appropriate engine
var basicResult = basicEngine.ParseAndExecute(simpleScript, data);
var advancedResult = advancedEngine.ParseAndExecute(complexScript, data);
```

### 2. Custom Engine Configurations

```csharp
// Build exactly the engine you need
var customEngine = new JLioEngineBuilder()
    .WithCommand<Add>()
    .WithCommand<Set>()
    .WithCommand<Remove>()
    .WithFunction<Fetch>()
    .WithFunction<DateTime>()
    .WithMathExtensions()  // Only include what you need
    .ConfigureParsing(options => {
        // Custom parse configuration
    })
    .ConfigureExecution(context => {
        // Custom execution configuration
    })
    .Build();
```

### 3. Runtime Engine Selection

```csharp
public class DynamicProcessor
{
    public JLioResult Process(string script, JToken data, ProcessingProfile profile)
    {
        var engine = profile switch
        {
            ProcessingProfile.Minimal => JLioEngineConfigurations.CreateV1().Build(),
            ProcessingProfile.Standard => JLioEngineConfigurations.CreateV2()
                .WithMathExtensions()
                .Build(),
            ProcessingProfile.Advanced => JLioEngineConfigurations.CreateLatest()
                .WithAllExtensions()
                .Build(),
            _ => throw new ArgumentException("Unknown profile")
        };

        return engine.ParseAndExecute(script, data);
    }
}
```

## Configuration Mappings

### Current Extension Registration ? New Engine Configuration

| Current Approach | New Engine Approach |
|-----------------|-------------------|
| `ParseOptions.CreateDefault().RegisterMath()` | `JLioEngineConfigurations.CreateV2().WithMathExtensions()` |
| `ParseOptions.CreateDefault().RegisterText()` | `JLioEngineConfigurations.CreateV2().WithTextExtensions()` |
| `ParseOptions.CreateDefault().RegisterETL()` | `JLioEngineConfigurations.CreateV3().WithETLExtensions()` |
| `ParseOptions.CreateDefault().RegisterMath().RegisterText().RegisterETL()` | `JLioEngineConfigurations.CreateLatest().WithAllExtensions()` |

### Engine Configuration Levels

| Engine Level | Includes | Use Cases |
|-------------|----------|-----------|
| `CreateV1()` | Core commands + basic functions | Legacy compatibility, minimal footprint |
| `CreateV2()` | V1 + advanced commands + extension support | Standard business logic |
| `CreateV3()` | V2 + ETL capabilities + advanced features | Complex data transformations |
| `CreateLatest()` | All available features | New projects, full functionality |

## Breaking Changes (None for Existing Code)

**Important**: There are **no breaking changes** for existing code. All current APIs remain functional:

? **Still Works**:
```csharp
var script = JLioConvert.Parse(scriptText);
var result = script.Execute(data);
```

? **Still Works**:
```csharp
var options = ParseOptions.CreateDefault().RegisterMath();
var script = JLioConvert.Parse(scriptText, options);
var result = script.Execute(data, ExecutionContext.CreateDefault());
```

? **Still Works**:
```csharp
var script = new JLioScript()
    .Add(new JValue("test"))
    .OnPath("$.path");
```

## Performance Considerations

### Memory Usage

The new architecture can reduce memory usage by:

- Creating engines with only needed commands/functions
- Eliminating global static registrations
- Allowing garbage collection of unused engines

### Execution Speed

- **Same Speed**: Existing code runs at identical speed
- **Potential Improvements**: Custom engines with fewer features may run slightly faster
- **Multiple Versions**: Different engines can be optimized for specific use cases

## Testing Strategy

### Compatibility Testing

```csharp
[Test]
public void ExistingCodeStillWorks()
{
    // Test that current approach works unchanged
    var script = JLioConvert.Parse("[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]");
    var result = script.Execute(new JObject());
    
    Assert.IsTrue(result.Success);
    Assert.AreEqual("hello", result.Data.SelectToken("$.test")?.Value<string>());
}

[Test]
public void NewEngineProducesSameResults()
{
    var data = JToken.Parse("{\"input\": 42}");
    var script = "[{\"path\":\"$.output\",\"value\":\"=calculate(@.input * 2)\",\"command\":\"add\"}]";
    
    // Old approach
    var oldOptions = ParseOptions.CreateDefault().RegisterMath();
    var oldResult = JLioConvert.Parse(script, oldOptions).Execute(data.DeepClone());
    
    // New approach
    var newEngine = JLioEngineConfigurations.CreateV2().WithMathExtensions().Build();
    var newResult = newEngine.ParseAndExecute(script, data.DeepClone());
    
    Assert.IsTrue(JToken.DeepEquals(oldResult.Data, newResult.Data));
}
```

### Migration Testing

```csharp
[Test]
public void CanRunMultipleVersions()
{
    var script = "[{\"path\":\"$.result\",\"value\":\"test\",\"command\":\"add\"}]";
    var data = new JObject();
    
    var v1Result = JLioEngineConfigurations.CreateV1().Build()
        .ParseAndExecute(script, data.DeepClone());
    var v3Result = JLioEngineConfigurations.CreateV3().Build()
        .ParseAndExecute(script, data.DeepClone());
    
    Assert.IsTrue(v1Result.Success);
    Assert.IsTrue(v3Result.Success);
    Assert.IsTrue(JToken.DeepEquals(v1Result.Data, v3Result.Data));
}
```

## Best Practices

### 1. Engine Lifecycle Management

```csharp
// Good: Reuse engines
public class ServiceManager
{
    private static readonly JLioEngine Engine = JLioEngineConfigurations
        .CreateV2()
        .WithMathExtensions()
        .Build();
    
    public JLioResult Process(string script, JToken data)
    {
        return Engine.ParseAndExecute(script, data);
    }
}

// Avoid: Creating engines per request
public JLioResult ProcessBad(string script, JToken data)
{
    var engine = JLioEngineConfigurations.CreateV2().Build(); // Expensive
    return engine.ParseAndExecute(script, data);
}
```

### 2. Configuration Management

```csharp
// Good: Centralized configuration
public static class JLioConfigs
{
    public static readonly JLioEngine StandardEngine = JLioEngineConfigurations
        .CreateV2()
        .WithMathExtensions()
        .WithTextExtensions()
        .Build();
        
    public static readonly JLioEngine MinimalEngine = JLioEngineConfigurations
        .CreateV1()
        .Build();
}
```

### 3. Version Selection

```csharp
// Good: Explicit version selection based on requirements
var engine = requirements.NeedsETL 
    ? JLioEngineConfigurations.CreateV3().WithETLExtensions().Build()
    : JLioEngineConfigurations.CreateV2().WithMathExtensions().Build();

// Avoid: Always using latest just because it's available
var engine = JLioEngineConfigurations.CreateLatest().Build(); // May be overkill
```

## Timeline and Support

### Current Version (Existing Architecture)
- **Status**: Fully supported
- **Timeline**: Indefinite support
- **Recommendation**: Safe to continue using

### New Version (Engine Architecture)
- **Status**: Available now
- **Timeline**: Recommended for new projects
- **Recommendation**: Migrate gradually when beneficial

### Support Matrix

| Feature | Current Architecture | Engine Architecture |
|---------|---------------------|-------------------|
| All existing APIs | ? Fully Supported | ? Fully Supported |
| Extension registration | ? Supported | ? Enhanced |
| Multiple versions | ? Not available | ? Available |
| Custom configurations | ?? Limited | ? Full support |
| Performance tuning | ?? Global only | ? Per-engine |

## Getting Help

### Migration Support

- Check existing tests to ensure compatibility
- Review performance with your specific use cases
- Consider gradual migration approach
- Test thoroughly before production deployment

### Common Issues and Solutions

**Issue**: "I have custom functions/commands"
**Solution**: Use `JLioEngineBuilder` with `.WithFunction<T>()` and `.WithCommand<T>()`

**Issue**: "Need different configurations per tenant"
**Solution**: Create multiple engines with different configurations

**Issue**: "Worried about performance impact"
**Solution**: No impact on existing code; new architecture can improve performance

**Issue**: "Complex existing setup"
**Solution**: Keep existing approach - no changes required

## Conclusion

The new engine-based architecture provides powerful new capabilities while maintaining full backward compatibility. You can:

1. **Continue using existing code unchanged** - no migration required
2. **Adopt new features gradually** - when you need multiple versions or custom configurations
3. **Take advantage of new capabilities** - for better performance and flexibility

The choice of migration strategy depends on your specific needs, but rest assured that your existing JLio code will continue to work exactly as it does today.