# JLio Expert — Open WebUI Setup Guide

A step-by-step walkthrough for turning any capable chat model into a true JLio expert inside Open WebUI. Ships with three interchangeable configuration artefacts and a recipe for wiring them up, adding a knowledge base, and sanity-checking the result.

## What you get in this folder

| File | What it is | When to use it |
|---|---|---|
| `system-prompt.md` | Copy/paste system prompt only (markdown-friendly). | Quick path: paste into the UI. |
| `jlio-expert.Modelfile` | Ollama-style Modelfile with `FROM`, `PARAMETER`, `SYSTEM`, and starter messages. | You run Ollama as your backend and want a versioned, reproducible definition. |
| `jlio-expert.openwebui.json` | Open WebUI model-export JSON (id, meta, params, suggestions). | You want to import the model straight into Open WebUI via the Models page. |

All three encode the **same system prompt**; pick whichever matches your workflow.

## Path A — Create the model in the Open WebUI UI (recommended)

This is the fastest path. No CLI needed.

### 1. Pick a base model

Open WebUI needs a base chat model to wrap. JLio is syntax-sensitive, so prefer instruction-tuned, code-aware models with a long context window.

| Good choice | Why |
|---|---|
| `llama3.1:70b-instruct` (Ollama) | Strong reasoning, reliable JSON output. |
| `qwen2.5-coder:32b-instruct` | Excellent for code/JSON; cheaper to run than 70B. |
| `mistral-large-latest` via an OpenAI-compatible endpoint | Great reasoner if you already have API access. |
| `llama3.1:8b-instruct` | Lightweight fallback for local laptops; expect more hand-holding. |

Make sure the base model is already available in **Admin Panel → Settings → Models** (or via your Ollama server).

### 2. Create the model

1. In Open WebUI, go to **Workspace → Models → +** (or **Admin Panel → Models → Create a new model**, depending on version).
2. Fill in:
   - **Name:** `JLio Expert`
   - **Model ID:** `jlio-expert`
   - **Description:** *Senior-engineer-level expert on JLio, a declarative JSON transformation language and its .NET implementation.*
   - **Base Model:** the model you chose in step 1.
   - **Tags:** `jlio`, `json`, `etl`, `dotnet`, `expert`.

### 3. Paste the system prompt

Open `system-prompt.md` in this folder. Copy the text **between** the `---BEGIN SYSTEM PROMPT---` and `---END SYSTEM PROMPT---` markers and paste it into the **System Prompt** field.

### 4. Tune generation parameters

In **Advanced Params**:

| Parameter | Value | Why |
|---|---|---|
| Temperature | `0.2` | JLio is exact syntax. Lower temperature = fewer invented commands/functions. |
| Top P | `0.9` | Standard. |
| Repeat Penalty | `1.05` | Slight penalty to keep longer answers from looping. |
| Context length (`num_ctx`) | `16384` or higher | JLio scripts plus input/output JSON get large quickly. |
| Max tokens (`num_predict`) | `2048` | Enough for a worked example without runaway output. |

### 5. Add suggestion prompts

Add these as chat starters so first-time users get a feel for what to ask:

- *Write a JLio script that adds a createdAt (UTC) and an id (GUID) to every order in $.orders.*
- *Compare add, set, and put with a one-line example for each.*
- *Show me a resolve + flatten + toCsv pipeline to produce a denormalised CSV of orders with customer name and email.*
- *How do I register the Math and Text extensions using the engine builder API?*
- *Build a decisionTable that assigns a customer segment from tier, yearsActive, and totalOrders.*
- *What is the difference between `$.items` and `$.items[*]` and when does it matter?*

### 6. Attach a knowledge base (huge quality boost)

The system prompt alone is already strong, but Open WebUI's knowledge/RAG support lets the model cite the actual docs. This is the single biggest lift you can give the expert.

1. Go to **Workspace → Knowledge → + Create Knowledge**.
2. Call it `JLio Docs`.
3. Upload the **entire** contents of `JLio/doc/` — including the `commands/`, `functions/`, `functions/datetime/`, `functions/math/`, and `functions/text/` subfolders. Also include:
   - `JLio/README.md`
   - `JLio/OPTIMIZATION_SUMMARY.md`
   - `JLio/CONTRIBUTING.md`
4. In embedding settings prefer a strong retrieval-oriented model, for example `nomic-embed-text` via Ollama, `bge-m3`, or `text-embedding-3-large` via an OpenAI-compatible endpoint. Chunk size ~1000 tokens, overlap ~150 works well for this corpus.
5. Back in your `JLio Expert` model, under **Knowledge**, attach the `JLio Docs` collection.
6. Recommended retrieval settings: top-K `6–8`, similarity threshold `0.15–0.25`. JLio docs are cross-referenced, so a slightly higher K helps pick up adjacent command/function details.

### 7. Save and test

Hit **Save**. Open a new chat, pick `JLio Expert`, and run the sanity tests in the next section.

---

## Path B — Modelfile (Ollama as backend)

Use this if you want a file-in-git, reproducible definition.

```bash
# From this folder
ollama create jlio-expert -f ./jlio-expert.Modelfile

# Verify
ollama run jlio-expert "Write a JLio script that appends a UUID and UTC timestamp to every item in $.orders."
```

Open WebUI will auto-detect `jlio-expert` from the Ollama server. You still get the most polish by also completing steps 6 (knowledge) and 5 (suggestion prompts) from Path A in the UI, because Open WebUI stores those on its side.

### Swapping the base model

Edit the first line of `jlio-expert.Modelfile`:

```
FROM qwen2.5-coder:32b-instruct
```

Rebuild with `ollama create`.

---

## Path C — Import the Open WebUI JSON

Use this if you want to drop the full Open WebUI model config (including suggestions, tags, and meta) into place in one click.

1. **Admin Panel → Models → Import**.
2. Select `jlio-expert.openwebui.json`.
3. Before saving, open the imported model and change `base_model_id` to the exact ID of a base model that exists on your server — the default in the file is `llama3.1:70b-instruct`.
4. Save.
5. Attach the knowledge base as in Path A, step 6.

---

## Sanity tests (run all six before you ship)

These probe the areas where LLMs most often hallucinate JLio syntax. Correct answers in parentheses.

1. **Add vs set vs put**
   > *On input `{"user":{"id":1}}`, give three scripts: (a) add a new `email`, (b) set the existing `id` to 2, (c) create `$.user.profile.bio = "hi"` regardless of whether `profile` exists.*
   Expected: `add` for (a), `set` for (b), `put` for (c). The model should not recommend `add` for (c) and should not recommend `set` for (a).

2. **Functions need the `=` prefix**
   > *Why is the literal string `newGuid()` not evaluated in my `add` command?*
   Expected: explain the `=` prefix; correct form is `"=newGuid()"`.

3. **Iteration operator**
   > *What's the difference between `$.items` and `$.items[*]` for a `resolve` command?*
   Expected: `$.items` targets the array itself (resolve will not iterate per item); `$.items[*]` iterates.

4. **Extension registration**
   > *Why does my script using `=sum($.order.items[*].price)` fail to parse?*
   Expected: requires `JLio.Extensions.Math`; register with `ParseOptions.CreateDefault().RegisterMath()` or build an engine that includes `WithMathExtensions()`.

5. **Decision table**
   > *Build a decisionTable that sets `discountRate` from `tier` and `yearsActive`, priority based, with a default 0.05.*
   Expected: valid JSON with `inputs`, `outputs`, `rules` (with `priority`, `conditions`, `results`), `defaultResults`, and `executionStrategy`.

6. **Named engines**
   > *How do I create a named engine `"order-processor"` with core commands plus Math and Text extensions, and use it thread-safely in a web service?*
   Expected: `JLioNamedEngines.Register("order-processor", b => b.WithCoreCommands().WithMathExtensions().WithTextExtensions().Build())` and a call via `JLioNamedEngines.Execute(...)`. Should mention `ConcurrentDictionary`-backed registry, pre-registered names, and singleton vs factory option.

If any of these go wrong, bump context length, try a larger base model, or verify that the knowledge base is actually wired up (ask the model to cite a specific doc file — e.g. *"Cite the doc file that describes the resolve command"* — and confirm it names `doc/commands/resolve.md`).

---

## Tuning cheat-sheet

| Symptom | Fix |
|---|---|
| Model invents command names or fields | Lower temperature to `0.1`; confirm knowledge base is attached; use a stronger base model. |
| Answers are short and generic | Raise `max_tokens` to `3072+`; add more suggestion prompts; widen context length. |
| Knowledge base rarely triggers | Increase top-K; lower similarity threshold; re-index with a retrieval-oriented embedding model. |
| Long scripts get cut off | Increase `num_predict`; ask the model to stream; check `max_tokens` in the model card. |
| Model talks about other JSON tools (jq, JSONPath libs) unprompted | Reinforce the scope rule: add a final clause to the system prompt: *"If the user's task is genuinely outside JLio, answer in one paragraph and pivot back to how JLio would solve it."* |

## Keeping the expert fresh

JLio gains commands and extensions over time. To update:

1. Pull the latest `doc/` folder into the `JLio Docs` knowledge collection (Open WebUI → Knowledge → the collection → **Re-sync / Upload**).
2. If new commands or functions ship, extend the **Commands** and **Functions** sections of the system prompt in `system-prompt.md`, then re-save the model with the new prompt.
3. Increment a tag like `jlio:vX` in the model's meta/tags so your users can see which docs cut they're talking to.

---

## One-screen summary

1. Create model → `JLio Expert`, base = strong instruction model, temperature `0.2`, ctx `16384+`.
2. Paste `system-prompt.md` into System Prompt.
3. Attach a knowledge collection built from the entire `JLio/doc/` tree.
4. Add the six suggestion prompts.
5. Run the six sanity tests. Ship it.
