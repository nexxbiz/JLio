# JLio MCP Server — Improvement Plan

## Context

This plan is grounded in real friction observed during a single session where an LLM (me) tried to write a JLio script to convert a JSON message from SIVI AFD 1.0 to AFD 2.0 (Dutch insurance data standard). The task should have taken ~3 tool calls. It took ~12, with most of the extra cost spent debugging silent failures and inconsistent argument conventions across functions.

Items are ordered by expected impact on agent success rate, not by implementation cost.

---

## P0 — Issues that broke the task

### 1. `set` silently fails when the target path doesn't exist

**What happened.** I called `set` to create new properties (`$.policy.startDate`, `$.commonFunctional.entityType`, etc.). The script returned `success: true`, no error, no hint — and the properties simply weren't there in the output. I burned several tool calls before realising `set` only *replaces* existing leaves; for new properties I needed `add` or `put`.

The `whenToUse` field for `set` says *"Use when you always want the value written, regardless of whether it exists. Pick 'add' to skip existing values."* That description is actively misleading — it implies `set` is the more aggressive of the two, when in fact `set` is the *narrower* one (replace-only) and `add` is the broader one (create-if-absent).

**Fix.**

- **Behaviour.** Return a hint when `set` matches zero tokens: `"set matched 0 tokens at '$.policy.startDate'. Use 'add' or 'put' to create the property if it does not exist."`
- **Documentation.** Rewrite the `whenToUse` for all three core writers to make the matrix obvious:
  - `add` — creates if missing, **skips** if present
  - `set` — **replaces** if present, **does nothing** if missing
  - `put` — upsert (creates or replaces)
- Add this matrix to `list_jlio_capabilities` output explicitly, not buried in `whenToUse` prose.

**Why P0.** This single ambiguity caused ~40% of the wasted tool calls. An LLM that picks `set` for "I want this property to exist with this value" is making a reasonable inference from English and from the docs, and gets a false-positive success.

---

### 2. Functions silently no-op when they receive an unresolvable path

**What happened.** I wrote `=substring('$.PP_INGDAT', 0, 4)` expecting `"2024"`. I got `"$.PP"` — the function had treated the path string as a literal. Then I tried `=substring(fetch('$.PP_INGDAT'), 0, 4)` — the whole `set` silently failed and produced nothing. No error, no hint. I had to bisect by calling each function in isolation.

The root cause is that **path resolution in function arguments is inconsistent across functions**:

| Function context | Path resolution? |
|---|---|
| `calculate('{{$.path}}')` | Yes, via `{{}}` |
| `ifElse.condition: "=contains($.path, 'x')"` | Yes, bare path |
| `concat('$.path', '...')` | **No** (despite the docs example showing `'$.name'` → `<name>`) |
| `substring('$.path', 0, 4)` | **No** |
| `fetch('$.path')` inside another function | **No** (fetch returns null/path-as-string) |
| `set value: "=fetch('$.path')"` as top-level | Inconsistent — sometimes works, sometimes silent fail |

**Fix.** Pick one of these and apply it everywhere:

- **Option A (recommended):** Every function argument that is a string starting with `$.` is resolved as a JSONPath. Document this once. Add `'literal:$.foo'` or a dedicated escape if a literal `$.` string is ever needed.
- **Option B:** Require explicit `fetch()` for path resolution and make `fetch` work uniformly inside any function. Update the `concat` docs example, which currently lies (`'$.name'` does not resolve in plain `concat`).

Whichever is chosen, **the inconsistency is the bug**, not the choice.

**Additional fix — diagnostics.** When a function expression evaluates and produces something obviously wrong (a value containing `$.` is a strong signal), surface a hint: `"function 'substring' received argument '$.PP_INGDAT' which looks like a JSONPath. Wrap with fetch() or use {{$.path}} syntax."`

**Why P0.** This is the single largest source of "the script ran successfully but produced wrong output" — the worst possible failure mode for an agent because there is no error to react to.

---

### 3. `decisionTable` silently produces no output

**What happened.** I tried `decisionTable` with `path: "$"` and again with `path: "$[*]"` on a wrapped array. Both ran with `success: true`, no error, no hints, **and no changes to the document**. The example in `get_jlio_item_details('decisionTable')` works (`path: "$.orders[*]"`), so the command exists and runs — but the failure mode for "path matched but outputs didn't write" is invisible.

**Fix.**

- Validate that `decisionTable.path` resolves to ≥1 token; if zero, return an error.
- Validate that each rule's output writes succeeded; if zero writes happened across all matched tokens, return a hint.
- Document whether `path: "$"` is supported. If yes, fix it. If no, reject it with a clear error.

**Why P0.** Same failure mode as #2 — silent no-op on a command that *looks* correct.

---

## P1 — Issues that significantly slowed the task

### 4. `ifElse` schema confusion: `condition` vs `first`/`second`

**What happened.** First attempt used `condition` as an object: `{ "referencePath": "$.PP_PRINBOK", "operator": "==", "compareValue": "J" }`. The error was `"Sequence contains no elements"` with hint `"Command 'ifElse' at position 7 is missing a 'path' or 'fromPath' property"` — which is wrong; `ifElse` doesn't take a `path`. The hint pointed me to call `get_jlio_item_details('ifElse')`, which is good, but the hint itself was misleading.

**Fix.**

- Wrong hint. The error generator is reaching for a generic "missing path/fromPath" message that doesn't apply to `ifElse`. Make the hint command-aware: `"Command 'ifElse' requires either 'condition' (a function expression) OR both 'first' and 'second' (values for equality comparison). See get_jlio_item_details('ifElse')."`
- The `first`/`second` form *does* work but is non-obvious. Consider deprecating it in favour of `condition` only, OR put both forms front-and-centre in the schema with two complete examples.

---

### 5. The `whenToUse` text uses "pick X to Y" phrasing that doesn't always match behaviour

**What happened.** Multiple `whenToUse` strings are written from the perspective of "you have command X, here's when to switch to Y." That's useful, but several of them are subtly wrong:

- `set.whenToUse` — see #1 above; says "always want the value written" which is false.
- `merge.whenToUse` mentions strategies "fullMerge, onlyStructure, onlyValues" — but those aren't documented in `list_jlio_capabilities`. An LLM has to guess they exist as parameters and call `get_jlio_item_details('merge')` to find out how to invoke them.
- `decisionTable.whenToUse` recommends `ifElse` "for a single binary condition" — fine, but the inverse (when to switch from `ifElse` to `decisionTable`) isn't there.

**Fix.** Audit every `whenToUse` field against the actual command behaviour. For commands with multi-value parameters (merge strategies, decisionTable execution modes), surface the parameter name in `list_jlio_capabilities` so the LLM knows it exists without a second tool call.

---

### 6. JSONPath filter expressions in write commands fail silently

**What happened.** I tried `add` with `path: "$[?(@.PP_PRINBOK=='J')].principleBookkeeping"` — a perfectly valid read-side JSONPath filter — to conditionally write a property. It ran with `success: true` and did nothing.

If filter expressions aren't supported in write paths, that's a reasonable engine constraint. But the silent success is the problem.

**Fix.** When a write command's `path` contains a filter expression `[?(...)]` and matches zero tokens, return a hint explicitly mentioning that filter expressions on missing paths cannot create new tokens. Recommend `ifElse` instead.

---

### 7. `validate_jsonpath` tool hung for 4 minutes

**What happened.** A single call to `validate_jsonpath` with a small input never returned and timed out. Could not retry the rest of the session. Possibly an isolated incident, but `validate_jsonpath` is exactly the tool an agent reaches for when path syntax is in question, so reliability matters disproportionately.

**Fix.** Add a hard timeout (5–10 seconds) inside the server with a clean error response, so a hang doesn't block the whole agent loop.

---

## P2 — Quality-of-life improvements

### 8. Add a `dryRun` / `explain_jlio_script` for write commands

The existing `execute_jlio_script` is essentially a dry run already (no side effects), which is great. What's missing is a per-command trace: "command 1 (`add`) wrote 1 token at `$.commonFunctional`. command 2 (`set`) matched 0 tokens — no-op." This would have caught issues #1, #3, and #6 in a single tool call instead of bisection.

I notice the system prompt mentions an `explain_jlio_script` tool — if it exists, expose it via `list_jlio_capabilities` discovery. I never saw it surface in my available tools.

### 9. Provide a `mapping` recipe for AFD 1.0 → 2.0 (or a generic "rename + restructure" recipe)

Renaming dozens of fields and reorganising them under entity buckets is a common pattern. A canonical recipe via `list_jlio_recipes` / `get_jlio_recipe` covering bulk rename, entity wrapping (scalar → array), and J/N → boolean conversion would let agents succeed in 1 tool call instead of 10.

### 10. Document the "wrap in array" idiom

AFD 2.0 requires every entity to be an array even with one element. JLio doesn't have a primitive for this — `promote` goes the other direction. Either add an `arrayify` / `wrap` command, or document the canonical workaround (e.g. via `merge` with a `[{}]` template) in a recipe.

### 11. Output format: include a `changesSummary` in execute responses

Today's response: `{ success, error, hints, output, logs }`. Adding `{ changesSummary: { commandsRun: 18, tokensWritten: 14, tokensRemoved: 9, noOpCommands: [3, 7] } }` would make silent no-ops impossible to miss without changing the engine semantics.

### 12. Consolidate function-argument syntax docs into one page

Right now the conventions are scattered across each function's `get_jlio_item_details`. A single top-level entry — perhaps returned by `list_jlio_capabilities` under a `functionArgumentConventions` key — covering: when to use `'$.path'` vs `{{$.path}}` vs bare `$.path`, when `fetch()` is required, and the literal-vs-resolved rules — would prevent #2 from recurring.

---

## Summary scorecard

| # | Issue | Cost in this session | Fix difficulty |
|---|---|---|---|
| 1 | `set` silently fails on missing path | ~3 tool calls | Low (docs + hint) |
| 2 | Inconsistent path resolution in functions | ~4 tool calls | Medium (engine consistency) |
| 3 | `decisionTable` silent no-op | ~2 tool calls | Low (validation) |
| 4 | `ifElse` schema hint is wrong | ~1 tool call | Low (hint text) |
| 5 | `whenToUse` text inaccuracies | ~1 tool call | Low (audit) |
| 6 | Filter paths silently no-op on write | ~1 tool call | Low (hint) |
| 7 | `validate_jsonpath` hang | session-blocking | Medium (timeout) |
| 8–12 | QoL | future sessions | Mixed |

**Headline.** The engine is capable. The discoverability and the silent-failure modes are what cost an agent time. Fixing #1, #2, and #3 alone would cut typical agent task length roughly in half.
