---
name: umbraco-block-builder
description: >
  Scaffolds a complete Umbraco 17 block list architecture from an HTML prototype,
  natural language description, or screenshot. Creates element types, settings types,
  block list data types, partial views, page content types, and Razor templates via
  the Umbraco Developer MCP Server. Use when the developer wants to set up Umbraco
  block lists, create content types from an HTML file, scaffold a block-based page
  structure, or convert a prototype to Umbraco content types. Triggered by phrases
  like "build the blocks", "scaffold this page", "create content types from this HTML",
  "set up Umbraco blocks", or "convert this prototype to Umbraco". Do NOT use for
  general Umbraco content editing, querying existing content, or non-block templates.
user_invocable: true
compatibility: >
  Requires Umbraco 17+ with the Management API enabled. Requires the umbraco-mcp MCP
  server connected (npx @umbraco-cms/mcp-dev@17). An Umbraco API User with admin
  permissions must be configured. Use against a development or staging instance only.
metadata:
  mcp-server: umbraco-mcp
  version: 1.0.0
  author: QuickBlocks
---

# Umbraco Block Builder

You are scaffolding a complete Umbraco 17 block list architecture from a developer-provided
input. You have access to the **Umbraco Developer MCP Server** via the `umbraco-mcp` MCP
connection.

Your job is to turn design intent (HTML, description, or screenshot) into a set of real
Umbraco artefacts — document types, data types, templates, and Razor partial views — using
the MCP tools as your only write mechanism.

---

## Critical Rules

These are non-negotiable. Do not proceed if any cannot be satisfied.

1. **Confirm before writing.** Never call a write MCP tool (`create-*`, `update-*`) without
   first presenting the proposed structure to the developer and receiving explicit confirmation.
   A mistake creates real artefacts in a live CMS that are harder to remove than to prevent.

2. **Check before creating.** Always call `get-all-document-types` and `get-all-data-types`
   before creating anything. If a matching artefact already exists, use its GUID — do not
   create a duplicate.

3. **Enforce creation order.** The block list data type JSON requires the GUIDs of element
   types that must already exist. Never attempt to create a block list data type before all
   referenced element types are confirmed as created. The exact dependency order is in Step 5.

4. **Never guess GUIDs.** Resolve all GUIDs by calling `get-all-data-types` or
   `get-all-document-types`. A wrong GUID silently breaks the block list.

5. **Umbraco 17 configuration format.** The block list `configuration` field uses an array
   of `{ alias, value }` pairs — NOT a flat object. See the schema in Step 6.

6. **Partial view path.** Block component partials go at:
   `Views/Partials/blocklist/Components/{alias}.cshtml`
   If the Umbraco subfolder bug (#16823) causes a 404, fall back to root partials and note
   the workaround in the summary.

7. **Use a non-production environment.** Remind the developer that scaffolding should be
   done against a development or staging instance, not production.

---

## Step 1 — Understand the Input

Accept any of the following — no `data-*` annotation is required:

- **Plain HTML** — a prototype or partial page
- **Natural language** — "I need a hero, a 3-column grid, and a testimonials carousel"
- **Screenshot or Figma image** — describe the visual structure, then treat as natural language
- **Mixed input** — HTML plus verbal additions

From the input, identify:

| What to find | What it becomes |
|---|---|
| The page name | Page document type (e.g. `homePage`) |
| Each distinct content section | A block type (element type) |
| Properties per block | Inferred from HTML element types (see Step 2) |
| Repeated sibling groups | Nested block list |
| Any block needing show/hide control | Settings type with `hide: True/False` |

**Naming rules:**
- Convert to PascalCase for type aliases (e.g. `heroRow`, `heroSettings`)
- Use sentence-case display names (e.g. `"Hero"`, `"Hero Settings"`)
- Derive names from CSS class names, element IDs, and heading text when present
- `<section class="hero">` → block called `Hero` (alias: `heroRow`)
- `<div class="service-card">` repeated → nested block called `Service Card` (alias: `serviceCardItem`)

If the input is ambiguous, ask the developer one focused clarifying question before proceeding
to the proposed structure. Do not ask multiple questions at once.

---

## Step 2 — Infer Data Types

Map HTML elements to Umbraco data types using this table:

| HTML element or context | Umbraco data type | Razor rendering |
|---|---|---|
| `<img>` | Image Media Picker | `@Url.GetCropUrl(row.Prop, w, h)` |
| `<h1>`–`<h6>` | Textstring | `@row.Prop` |
| `<p>` (body copy) | Rich Text Editor | `@Html.Raw(row.Prop)` |
| `<a>` (single link) | Single URL Picker | `@row.Prop?.Url` / `.Name` / `.Target` |
| `<a>` (repeated in list) | Multi URL Picker | loop with `@item.Url` |
| `<video>` | Media Picker | `@row.Prop?.Url` |
| `<input type="text">` | Textstring | `@row.Prop` |
| `<textarea>` | Textarea | `@row.Prop` |
| `<select>` | Dropdown | `@row.Prop` |
| `<input type="checkbox">` | True/False | `@if (row.Prop) { ... }` |
| Repeated sibling group | Nested block list | `@Html.GetBlockListHtml(row.Prop)` |
| Anything else | Textstring (default) | `@row.Prop` |

Use `get-all-data-types` to resolve the exact GUID and confirmed name for every data type
before building the block list configuration JSON. Never assume names or GUIDs.

---

## Step 3 — Present the Proposed Structure

Before making any write calls, output a structured summary in this exact format:

```
## Proposed Block Architecture

**Page type:** {Display Name} (alias: {camelCaseAlias})

**Block list:** {Block List Name}
  ├── {Block Name} (element: {alias})
  │     {property}: {Data Type}
  │     {property}: {Data Type}
  │   settings: {settingsAlias} (hide: True/False)
  │
  └── {Block Name} (element: {alias})
        {property}: {Data Type}
        [nested list] {Nested List Name}
            {Item Name} (element: {itemAlias})
                {property}: {Data Type}
      settings: none

Does this look right? Reply YES to proceed, or tell me what to change.
```

**Wait for explicit confirmation before proceeding.** If the developer replies with changes,
update the model and re-present the summary. Only proceed when the developer says YES (or
equivalent).

---

## Step 4 — Resolve Existing State

Before creating anything, call:

```
get-all-document-types   → build a map of alias → id (to avoid duplicates and reuse GUIDs)
get-all-data-types       → build a map of name → id (to resolve GUIDs for Image Media Picker etc.)
get-server-information   → confirm Umbraco version is 17+
```

Flag to the developer if the Umbraco version is not 17 — the configuration schema differs.

---

## Step 5 — Create in Dependency Order

Execute in this exact order. Each step depends on all previous steps completing successfully.

```
 1. create-document-type-folder  → "Elements"
 2. create-document-type-folder  → "Elements/Content Models"
 3. create-document-type-folder  → "Elements/Settings Models"
 4. create-document-type-folder  → "Pages"
 5. get-all-data-types           → resolve GUIDs for all needed primitive data types
 6. create-element-type          → one per block content type (e.g. heroRow)
 7. create-element-type          → one per block settings type (e.g. heroSettings)
                                   Always include a "Hide" property of type True/False
 8. create-data-type             → one per nested block list (if any)
                                   Uses GUIDs from steps 6–7
 9. create-data-type             → page-level block list(s)
                                   Uses GUIDs from steps 6–7
10. create-document-type         → the page content type
                                   References block list data types from step 9
11. create-template              → master.cshtml (see Step 7a)
12. create-template              → {pageAlias}.cshtml with masterTemplateAlias: "master" (see Step 7b)
13. create-partial-view          → Views/Partials/navigation.cshtml (see Step 7c)
14. create-partial-view          → Views/Partials/footer.cshtml (see Step 7c)
15. create-partial-view          → Views/Partials/blocklist/default.cshtml (see Step 7d)
16. create-partial-view          → Views/Partials/blocklist/Components/{alias}.cshtml per block (see Step 7e)
17. get-document-type / get-data-type → verify all created artefacts (see Step 8)
```

If any step returns a conflict (artefact already exists), use the existing item's GUID and
continue. Do not abort.

---

## Step 6 — Block List Data Type JSON (Umbraco 17)

Use this exact `configuration` structure when calling `create-data-type`:

```json
{
  "name": "{Block List Display Name}",
  "editorAlias": "Umbraco.BlockList",
  "editorUiAlias": "Umb.PropertyEditorUi.BlockList",
  "configuration": [
    {
      "alias": "blocks",
      "value": [
        {
          "contentElementTypeKey": "<GUID of content element type>",
          "settingsElementTypeKey": "<GUID of settings element type, or null if none>",
          "label": "{{ !title || title == '' ? '{Block Display Name}' : title }}",
          "editorSize": "medium",
          "forceHideContentEditorInOverlay": false,
          "iconColor": "#1b264f",
          "backgroundColor": "#ffffff"
        }
      ]
    },
    { "alias": "validationLimit",          "value": { "min": null, "max": null } },
    { "alias": "useSingleBlockMode",       "value": false },
    { "alias": "useLiveEditing",           "value": false },
    { "alias": "useInlineEditingAsDefault","value": false },
    { "alias": "maxPropertyWidth",         "value": "100%" }
  ]
}
```

**Critical:** Both `editorAlias` and `editorUiAlias` are required in Umbraco 17. Omitting
either will cause the data type to be created but non-functional in the editor.

For block lists with multiple block types, add one object per block type in the `blocks` array.

---

## Step 7a — Master Template

The master template is the full HTML shell (nav + layout + footer). Transform the input HTML:

1. Replace the main content element with `@RenderBody()`
2. Replace nav/footer elements with `@await Html.PartialAsync("~/Views/Partials/{name}.cshtml")`
3. Strip all `data-*` attributes

```razor
@* master.cshtml — no @inherits, no Layout *@
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <title>{Site Name}</title>
</head>
<body>
    @await Html.PartialAsync("~/Views/Partials/navigation.cshtml")

    @RenderBody()

    @await Html.PartialAsync("~/Views/Partials/footer.cshtml")
</body>
</html>
```

MCP call: `create-template` with `name: "Master"`, `alias: "master"`, content as above.

---

## Step 7b — Content Page Template

The content page template contains only the main content region. It sets `Layout = "master.cshtml"`
and calls `@Html.GetBlockListHtml()` for each block list property on the page:

```razor
@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.{PagePascalAlias}>
    @using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
    Layout = "master.cshtml";
}

<main>
    @Html.GetBlockListHtml(Model.{BlockListPropertyAlias})
</main>
```

MCP call: `create-template` with `name: "{Page Name}"`, `alias: "{pageAlias}"`,
`masterTemplateAlias: "master"`, content as above.

---

## Step 7c — Structural Partials (Nav, Footer, etc.)

For each element that belongs in a reusable partial (navigation, footer, cookie banner):

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage

{original HTML with data-* attributes stripped}
```

MCP call: `create-partial-view` at `Views/Partials/{name}.cshtml`.

---

## Step 7d — Block List Dispatcher (Fixed File)

This file is always identical. It routes every block to its component partial by convention.
Create it once; it never needs to change as new block types are added.

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListModel>
@{
    if (Model?.Any() != true) { return; }
}
<div class="umb-block-list">
    @foreach (var block in Model)
    {
        if (block?.ContentUdi == null) { continue; }
        var data = block.Content;

        @await Html.PartialAsync("blocklist/Components/" + data.ContentType.Alias, block)
    }
</div>
```

MCP call: `create-partial-view` at `Views/Partials/blocklist/default.cshtml`.

---

## Step 7e — Block Component Partials

One file per block type. Each casts `BlockListItem` to the block's typed content and settings
models, checks `settings.Hide`, and renders the HTML with Razor expressions:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var row      = ({ContentTypePascalAlias})Model.Content;
    var settings = ({SettingsTypePascalAlias})Model.Settings;

    if (settings.Hide) { return; }
}

{HTML from original prototype, with property values replaced by Razor expressions}
```

**Property rendering reference:**

| Data type | Razor expression |
|---|---|
| Textstring | `@row.{PropAlias}` |
| Rich Text Editor | `@Html.Raw(row.{PropAlias})` |
| Image Media Picker | `<img src="@Url.GetCropUrl(row.{PropAlias}, 800, 600)" alt="@row.{PropAlias}?.Name" />` |
| Single URL Picker | `<a href="@row.{PropAlias}?.Url" target="@row.{PropAlias}?.Target">@row.{PropAlias}?.Name</a>` |
| True/False | `@if (row.{PropAlias}) { ... }` |
| Nested block list | `@Html.GetBlockListHtml(row.{PropAlias})` |

MCP call: `create-partial-view` at `Views/Partials/blocklist/Components/{alias}.cshtml`.

---

## Step 8 — Verify and Report

After all creation steps complete, verify the key artefacts:

```
get-document-type  → for each created document/element type
get-data-type      → for each created block list data type
```

Then output a summary table:

```
## Build Complete

| Artefact | Name | Status |
|---|---|---|
| Element type | heroRow | ✓ Created |
| Element type | heroSettings | ✓ Created |
| Block list data type | Main Content | ✓ Created |
| Page document type | homePage | ✓ Created |
| Template | master.cshtml | ✓ Created |
| Template | homePage.cshtml | ✓ Created |
| Structural partial | navigation.cshtml | ✓ Created |
| Structural partial | footer.cshtml | ✓ Created |
| Block list dispatcher | blocklist/default.cshtml | ✓ Created |
| Component partial | blocklist/Components/heroRow.cshtml | ✓ Created |
```

If any item shows an error, explain what failed and offer to retry or skip.

---

## Error Handling

| Error condition | Action |
|---|---|
| `create-element-type` conflict (already exists) | Use existing GUID and continue |
| `create-partial-view` 404 on deep path (Umbraco bug #16823) | Create at root partials folder, note workaround in summary |
| GUID lookup fails | Call `get-data-type-search` with type name as fallback |
| `get-server-information` shows Umbraco < 17 | Warn developer — block list JSON schema differs; do not proceed without confirmation |
| MCP connection unavailable | Stop and ask developer to check `.mcp.json` and `UMBRACO_BASE_URL` |

---

## Reference: Worked Example

See `references/worked-example.md` for a complete end-to-end walkthrough:
input HTML → proposed structure → full MCP call sequence → all generated Razor files.

Consult it whenever you need to calibrate naming conventions, MCP call order, or
the exact shape of a generated partial view.
