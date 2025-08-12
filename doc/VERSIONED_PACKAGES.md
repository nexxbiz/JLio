# JLio Versioned Package Support

## Overview

The enhanced JLio engine architecture now supports loading different versions of extension packages side-by-side using .NET's `AssemblyLoadContext`. This enables true isolation between package versions, allowing you to:

- Run different versions of the same extension package simultaneously
- Isolate tenant-specific package versions
- Gradually migrate from old to new package versions
- Test against multiple package versions
- Deploy applications with different package requirements

## Key Classes

### JLioVersionedEngineBuilder
Main builder for creating engines that support versioned packages.

### JLioVersionedEngine
Engine instance that manages AssemblyLoadContexts for package isolation.

### JLioVersionedEngineConfigurations
Predefined configurations for common versioned scenarios.

## Usage Examples

### Basic Versioned Package Loading

```csharp
// Load specific versions of extension packages
using var engine = new JLioVersionedEngineBuilder()
    .WithCoreCommands()
    .WithCoreFunctions()
    .WithMathExtensions("2.0.0", @"C:\packages\JLio.Extensions.Math.2.0.0\JLio.Extensions.Math.dll")
    .WithTextExtensions("1.5.0", @"C:\packages\JLio.Extensions.Text.1.5.0\JLio.Extensions.Text.dll")
    .Build();

var result = engine.ParseAndExecute(script, data);
```

### Multi-Tenant Scenario

```csharp
public class MultiTenantJLioService
{
    private readonly Dictionary<string, JLioVersionedEngine> tenantEngines;

    public MultiTenantJLioService()
    {
        tenantEngines = new Dictionary<string, JLioVersionedEngine>
        {
            ["tenant-legacy"] = new JLioVersionedEngineBuilder()
                .WithCoreCommands()
                .WithMathExtensions("1.0.0") // Legacy version for old tenant
                .Build(),
                
            ["tenant-modern"] = new JLioVersionedEngineBuilder()
                .WithCoreCommands()
                .WithMathExtensions("3.0.0") // Latest version for new tenant
                .WithTextExtensions("2.0.0")
                .Build(),
                
            ["tenant-custom"] = new JLioVersionedEngineBuilder()
                .WithCoreCommands()
                .WithVersionedPackage(
                    "CustomExtensions", 
                    "1.0.0", 
                    @"C:\custom\CustomExtensions.dll",
                    (options, assembly) => {
                        // Custom registration logic for tenant-specific extensions
                        RegisterCustomExtensions(options, assembly);
                    })
                .Build()
        };
    }

    public JLioExecutionResult ProcessForTenant(string tenantId, string script, JToken data)
    {
        var engine = tenantEngines[tenantId];
        return engine.ParseAndExecute(script, data);
    }

    public void Dispose()
    {
        foreach (var engine in tenantEngines.Values)
        {
            engine.Dispose(); // Properly unload AssemblyLoadContexts
        }
    }
}
```

### A/B Testing Different Package Versions

```csharp
public class JLioABTestService
{
    private readonly JLioVersionedEngine versionA;
    private readonly JLioVersionedEngine versionB;

    public JLioABTestService()
    {
        versionA = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithMathExtensions("2.0.0") // Current version
            .Build();

        versionB = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithMathExtensions("3.0.0-beta") // Beta version being tested
            .Build();
    }

    public JLioExecutionResult ProcessWithABTest(string script, JToken data, bool useBeta = false)
    {
        var engine = useBeta ? versionB : versionA;
        return engine.ParseAndExecute(script, data);
    }
}
```

### Migration Scenario

```csharp
public class JLioMigrationService
{
    private readonly JLioVersionedEngine oldEngine;
    private readonly JLioVersionedEngine newEngine;
    private readonly bool migrationInProgress;

    public JLioMigrationService(bool enableMigration)
    {
        migrationInProgress = enableMigration;

        oldEngine = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithMathExtensions("1.0.0")
            .WithTextExtensions("1.0.0")
            .Build();

        newEngine = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithMathExtensions("2.0.0")
            .WithTextExtensions("2.0.0")
            .Build();
    }

    public JLioExecutionResult Process(string script, JToken data, bool forceNewVersion = false)
    {
        if (forceNewVersion || (migrationInProgress && ShouldUseBetaForScript(script)))
        {
            return newEngine.ParseAndExecute(script, data);
        }
        
        return oldEngine.ParseAndExecute(script, data);
    }

    private bool ShouldUseBetaForScript(string script)
    {
        // Logic to determine if this script should use the new version
        // Could be based on script content, user flags, etc.
        return script.Contains("new-feature");
    }
}
```

### Custom Package Version Loading

```csharp
// Load a custom assembly with specific version handling
using var engine = new JLioVersionedEngineBuilder()
    .WithCoreCommands()
    .WithVersionedPackage(
        packageName: "MyCompany.JLio.Extensions", 
        version: "1.2.3", 
        assemblyPath: @"C:\mypackages\MyCompany.JLio.Extensions.1.2.3.dll",
        registerAction: (options, assembly) =>
        {
            // Custom registration logic
            var registrarType = assembly.GetType("MyCompany.JLio.Extensions.Registrar");
            var registerMethod = registrarType.GetMethod("Register");
            registerMethod.Invoke(null, new object[] { options });
        })
    .Build();

// Check what versions are loaded
var loadedVersions = engine.GetLoadedPackageVersions();
Console.WriteLine($"Loaded: {string.Join(", ", loadedVersions)}");
```

## Predefined Configurations

### Multi-Tenant Configuration
```csharp
var engine = JLioVersionedEngineConfigurations.CreateMultiTenant()
    .WithMathExtensions("2.0.0")
    .WithTextExtensions("1.5.0")
    .Build();
```

### Testing Configuration
```csharp
var engine = JLioVersionedEngineConfigurations.CreateForTesting()
    .WithMathExtensions("3.0.0-beta")
    .Build();
```

### Migration Configuration
```csharp
var engine = JLioVersionedEngineConfigurations.CreateForMigration()
    .WithMathExtensions("1.0.0") // Keep stable version during migration
    .WithTextExtensions("2.0.0") // Upgrade text features
    .Build();
```

### Specific Version Configuration
```csharp
var engine = JLioVersionedEngineConfigurations.CreateWithSpecificVersions(
    mathVersion: "2.1.0",
    textVersion: "1.8.0", 
    etlVersion: "3.0.0"
).Build();
```

## Package Structure Requirements

For versioned package loading to work, organize your packages like this:

```
packages/
??? JLio.Extensions.Math.1.0.0/
?   ??? JLio.Extensions.Math.dll
?   ??? dependencies/
??? JLio.Extensions.Math.2.0.0/
?   ??? JLio.Extensions.Math.dll
?   ??? dependencies/
??? JLio.Extensions.Text.1.5.0/
?   ??? JLio.Extensions.Text.dll
?   ??? dependencies/
??? JLio.Extensions.Text.2.0.0/
    ??? JLio.Extensions.Text.dll
    ??? dependencies/
```

## Benefits

### True Package Isolation
- Each version loads in its own `AssemblyLoadContext`
- No conflicts between different versions
- Proper memory cleanup when engines are disposed

### Tenant Isolation
- Different tenants can use different package versions
- No shared state between tenant engines
- Independent upgrade paths

### Testing & Migration Support
- Test new package versions alongside production versions
- Gradual rollout capabilities
- A/B testing support

### Memory Management
- AssemblyLoadContexts are collectible
- Proper cleanup when engines are disposed
- No memory leaks from version conflicts

## Important Considerations

### Performance
- Loading multiple versions uses more memory
- AssemblyLoadContext creation has overhead
- Consider caching engines for frequently used configurations

### Dependency Management
- Each versioned package must include all its dependencies
- Assembly resolution works within each isolated context
- May need custom dependency resolvers for complex scenarios

### Cleanup
```csharp
// Always dispose versioned engines to unload AssemblyLoadContexts
using var engine = new JLioVersionedEngineBuilder()...Build();
// or
var engine = new JLioVersionedEngineBuilder()...Build();
try 
{
    // Use engine
}
finally 
{
    engine.Dispose();
}
```

## Migration from Standard Engine

Existing code using the standard `JLioEngine` continues to work unchanged. To adopt versioned packages:

1. **No changes needed** for default versions
2. **Opt-in** to specific versions when needed
3. **Gradual adoption** - start with critical packages
4. **Full migration** when all packages support versioning

The versioned engine architecture provides the ultimate flexibility for complex deployment scenarios while maintaining the simplicity of the standard engine for common use cases.