# JLio MCP Server

An [MCP (Model Context Protocol)](https://modelcontextprotocol.io/) server that exposes the full **JLio V3** transformation engine as a tool so AI agents can execute and validate JLio scripts against real data.

## Tools

### `execute_jlio_script`

Executes a JLio transformation script against a JSON input and returns the result.

| Parameter | Type | Description |
|-----------|------|-------------|
| `script` | `string` | A JLio script expressed as a JSON array of command objects |
| `input` | `string` | Any valid JSON value (object, array, string, number, boolean, null) |

**Response**

```json
{
  "success": true,
  "error": null,
  "output": { }
}
```

---

### `list_jlio_capabilities`

Returns a structured index of every command and function available in the V3 engine — names, categories, and one-line summaries. Call this first to discover what is available before generating a script.

No parameters required.

---

### `get_jlio_item_details`

Returns the full schema, all parameters, behavioural notes, and complete JSON script examples for a specific command or function.

| Parameter | Type | Description |
|-----------|------|-------------|
| `name` | `string` | The exact name returned by `list_jlio_capabilities` (e.g. `add`, `sumif`, `calculate`) |

**Recommended agent workflow**

1. Call `list_jlio_capabilities` to see all available commands and functions.
2. Call `get_jlio_item_details` for the specific item(s) needed to understand parameters and see examples.
3. Compose the script and call `execute_jlio_script` to validate it against real data.

---

The engine supports the full **V3** feature set:
- Core commands: `add`, `set`, `put`, `remove`, `copy`, `move`
- Advanced commands: `compare`, `merge`, `decisionTable`, `ifElse`
- Extensions: **Math**, **Text**, **ETL**, **JSchema**, **TimeDate**

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Clone/build this repository **or** publish a self-contained executable (see below)

---

## Building

```powershell
# From the repository root
dotnet build JLio.MCP\JLio.MCP.csproj -c Release
```

### Publish as a self-contained executable (recommended for use in MCP configs)

```powershell
# Windows x64
dotnet publish JLio.MCP\JLio.MCP.csproj -c Release -r win-x64 --self-contained true -o publish\win-x64

# Linux x64
dotnet publish JLio.MCP\JLio.MCP.csproj -c Release -r linux-x64 --self-contained true -o publish\linux-x64

# macOS arm64 (Apple Silicon)
dotnet publish JLio.MCP\JLio.MCP.csproj -c Release -r osx-arm64 --self-contained true -o publish\osx-arm64
```

---

## Running manually (stdio)

```powershell
dotnet run --project JLio.MCP\JLio.MCP.csproj
```

The server communicates over **stdin / stdout** using the MCP stdio transport. Do **not** run it interactively; connect it through an MCP host.

---

## Environment configurations

### Visual Studio Code — GitHub Copilot / Copilot Chat

Add an entry to your workspace or user MCP settings file (`.vscode/mcp.json` or the user-level equivalent):

```json
{
  "servers": {
    "jlio": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Repos\\Nexxbiz\\JLio\\JLio.MCP\\JLio.MCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

> **Tip:** Replace the project path with the published executable path for faster startup:
> ```json
> "command": "C:\\Repos\\Nexxbiz\\JLio\\publish\\win-x64\\JLio.MCP.exe",
> "args": []
> ```

---

### Visual Studio 2022 / 2025+

Visual Studio reads MCP server definitions from `.mcp.json` at the solution root.

Create or edit **`.mcp.json`** next to the `.sln` file:

```json
{
  "servers": {
    "jlio": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "JLio.MCP\\JLio.MCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

Restart Visual Studio and the **jlio** server will appear in the Copilot tool list.

---

### Claude Desktop

Edit `%APPDATA%\Claude\claude_desktop_config.json` (Windows) or `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS):

```json
{
  "mcpServers": {
    "jlio": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Repos\\Nexxbiz\\JLio\\JLio.MCP\\JLio.MCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

Using the published executable (faster):

```json
{
  "mcpServers": {
    "jlio": {
      "command": "C:\\Repos\\Nexxbiz\\JLio\\publish\\win-x64\\JLio.MCP.exe",
      "args": []
    }
  }
}
```

Restart Claude Desktop to pick up the change.

---

### Cursor

Edit `~/.cursor/mcp.json` (global) or `.cursor/mcp.json` inside your project:

```json
{
  "mcpServers": {
    "jlio": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Repos\\Nexxbiz\\JLio\\JLio.MCP\\JLio.MCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

---

### Windsurf

Edit `~/.codeium/windsurf/mcp_config.json`:

```json
{
  "mcpServers": {
    "jlio": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Repos\\Nexxbiz\\JLio\\JLio.MCP\\JLio.MCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

---

### Custom MCP host / programmatic usage

Any host that supports the MCP stdio transport can connect by spawning the process:

```
dotnet run --project <path-to>/JLio.MCP.csproj --no-build
```

or the published binary directly. The process reads JSON-RPC messages from **stdin** and writes responses to **stdout**.

---

## Example interaction

**Script** — set a field on every item in an array:

```json
[
  {
    "type": "set",
    "path": "$.items[*].processed",
    "value": true
  }
]
```

**Input:**

```json
{
  "items": [
    { "id": 1 },
    { "id": 2 }
  ]
}
```

**Response:**

```json
{
  "success": true,
  "error": null,
  "output": {
    "items": [
      { "id": 1, "processed": true },
      { "id": 2, "processed": true }
    ]
  }
}
```

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Server not appearing in the tool list | Verify the path in the config is correct and the project builds (`dotnet build`) |
| `dotnet` not found | Add the .NET 8 SDK to `PATH` or use the full path to the `dotnet` executable |
| Slow startup | Publish a self-contained executable and reference that instead of `dotnet run` |
| `"success": false` with a parse error | Check your script is a valid JSON array; validate at [jlio.online](https://jlio.online/) |
