# QuickBlocks as a Claude Skill
### How the Umbraco Developer MCP Server and Claude Agent Skills Replace a Custom Package

---

## Table of Contents

1. [The Problem, Restated](#1-the-problem-restated)
2. [Why QuickBlocks Has a Ceiling](#2-why-quickblocks-has-a-ceiling)
3. [The New Stack](#3-the-new-stack)
4. [Architecture](#4-architecture)
5. [The Umbraco Developer MCP Server](#5-the-umbraco-developer-mcp-server)
6. [Exact MCP Tools Used](#6-exact-mcp-tools-used)
6a. [The Razor Template and Partial Pattern](#6a-the-razor-template-and-partial-pattern)
7. [How the Skill Works — Step by Step](#7-how-the-skill-works--step-by-step)
8. [The Skill Definition](#8-the-skill-definition)
9. [MCP Configuration](#9-mcp-configuration)
10. [Processing Pipeline Comparison](#10-processing-pipeline-comparison)
11. [What the Developer Does Differently](#11-what-the-developer-does-differently)
12. [Worked Example](#12-worked-example)
13. [Known Considerations](#13-known-considerations)
14. [Future Possibilities](#14-future-possibilities)

---

## 1. The Problem, Restated

Building a block-list-based Umbraco site from an HTML prototype requires creating a large number of tightly coupled artefacts manually:

- Element content types (one per block)
- Settings content types (one per block)
- Block list data types (one per list, referencing the above by GUID)
- A page content type with the correct block list properties
- A master template (`master.cshtml`) — the full HTML shell with `@RenderBody()` and `@await Html.PartialAsync(...)` for nav/footer
- A content page template — inherits master, calls `@Html.GetBlockListHtml(Model.PropertyAlias)` for each block list
- Structural partials — navigation, footer, and other reusable HTML regions
- A block list dispatcher (`blocklist/default.cshtml`) — loops block list items and routes each to the right component partial by convention
- Block component partials — one per block type, casting `BlockListItem` to the typed content/settings models
- A folder structure to keep it organised

Doing this by hand for a site with 10–15 block types takes hours and is highly error-prone. QuickBlocks automates it from an annotated HTML file.

---

## 2. Why QuickBlocks Has a Ceiling

QuickBlocks is effective but constrained by its design:

| Constraint | Impact |
|---|---|
| Requires `data-*` annotation | The developer still does the hard thinking; HTML must be rewritten to use QuickBlocks vocabulary |
| Input is always HTML | Can't accept a description, a screenshot, or a Figma link |
| Runs inside Umbraco (C# package) | Tied to Umbraco 10's internal APIs; porting to Umbraco 14–17 requires rewriting against the new Management API |
| No conversation or iteration | One-shot; if the output is wrong you adjust attributes and re-run |
| Black box | Doesn't explain why it made each decision |
| No version targeting | Outputs Umbraco 10 conventions; block list JSON schema changed in Umbraco 17 |
| Maintenance burden | Every Umbraco major version break requires C# package updates and a new NuGet release |

The annotation step is the sharpest problem. Writing:

```html
<section data-row-name="Hero"
         data-settings-name="Hero Settings"
         data-has-settings="true"
         data-icon-class="icon-landscape"
         data-label-property="Title">
  <h1 data-prop-name="Title">Heading</h1>
  <p  data-prop-name="Body Text">Description</p>
```

…is not dramatically faster than creating the content type by hand. It requires the same Umbraco domain knowledge. It's a domain-specific language that happens to look like HTML.

---

## 3. The New Stack

Three components, each doing what it is best at:

| Component | Role |
|---|---|
| **Claude** (via Claude Code or Agent SDK) | Understands intent, infers structure, generates Razor, makes decisions |
| **Umbraco Developer MCP Server** | Executes operations against a live Umbraco 17 instance via 330+ tools wrapping the Management API |
| **Claude Skill (`SKILL.md`)** | Teaches Claude the Umbraco block list workflow — the sequence of steps, naming conventions, and pitfalls |

None of these require a C# Umbraco package. The skill is a Markdown file. The MCP server is an npm package run via `npx`. The Management API is built into Umbraco 14+.

---

## 4. Architecture

```mermaid
flowchart TD
    DEV[Developer] -->|plain HTML\nnatural language\nscreenshot| CC[Claude Code\nor Agent SDK]

    CC -->|invokes| SKILL[Claude Skill\nSKILL.md\nUmbraco Block Builder]

    SKILL -->|teaches Claude| WORKFLOW[Umbraco Block Build Workflow\n1 Analyse input\n2 Infer blocks and properties\n3 Confirm with developer\n4 Create artefacts in order\n5 Generate Razor partials\n6 Verify and report]

    WORKFLOW -->|tool calls| MCP[Umbraco Developer MCP Server\nnpx @umbraco-cms/mcp-dev@17]

    MCP -->|REST calls| MGMT[Umbraco 17 Management API\n/umbraco/management/api/v1/...]

    MGMT --> DT[Data Types\nBlock List configs]
    MGMT --> DOCTYPE[Document Types\nElement types\nSettings types\nPage type]
    MGMT --> TMPL[Templates\nMaster + Content]
    MGMT --> PV[Partial Views\nRazor cshtml files]
    MGMT --> FS[Folder Structure\nContent type folders]
```

---

## 5. The Umbraco Developer MCP Server

The **Umbraco Developer MCP Server** is an official Umbraco product (originally community-built by Matthew Wise, adopted into the Umbraco GitHub organisation).

- **NPM package**: `@umbraco-cms/mcp-dev@17`
- **Repo**: [github.com/umbraco/Umbraco-CMS-MCP-Dev](https://github.com/umbraco/Umbraco-CMS-MCP-Dev)
- **Docs**: [docs.umbraco.com/umbraco-cms/reference/developer-mcp](https://docs.umbraco.com/umbraco-cms/reference/developer-mcp)
- **Scale**: 330+ tools across 36 endpoint groups, near-complete parity with the Management API
- **Auth**: Umbraco API User (OAuth2 client credentials) — fine-grained permission control
- **Transport**: runs as a local `stdio` process via `npx`; no installation required

The server exposes named **tool collections**. You opt in to only the ones you need:

```
document-type  data-type  template  partial-view  document
script  stylesheet  media  language  log-viewer  webhook  …
```

For a QuickBlocks-equivalent workflow, only five collections are needed:
`document-type`, `data-type`, `template`, `partial-view`, `document`

---

## 6. Exact MCP Tools Used

The following MCP tools map directly to the steps QuickBlocks performs in C#:

### Folder structure

| QuickBlocks (C#) | MCP Tool |
|---|---|
| `IContentTypeService.Save()` (folder) | `create-document-type-folder` |

### Element and settings content types

| QuickBlocks (C#) | MCP Tool |
|---|---|
| `IContentTypeService.Save()` (element) | `create-element-type` |
| `IContentTypeService.Save()` (page) | `create-document-type` |
| `AddPropertiesToContentType()` | `update-document-type` (with properties array) |
| Check if type exists | `get-document-type` / `get-all-document-types` |
| `get-document-type-available-compositions` | `get-document-type-available-compositions` |

### Data types / block lists

| QuickBlocks (C#) | MCP Tool |
|---|---|
| `IDataTypeService.Save()` | `create-data-type` |
| Check if data type exists | `get-data-type-search` |
| Get property editor schema | `get-data-type-property-editor-template` |
| Resolve existing data type GUID | `get-all-data-types` |

### Templates

| QuickBlocks (C#) | MCP Tool |
|---|---|
| `_fileService.CreateTemplateWithIdentity("Master", "master", ...)` | `create-template` (name: `Master`, alias: `master`) |
| `_fileService.CreateTemplateForContentType(alias, name)` | `create-template` (name: page name, alias: page alias) |
| `template.SetMasterTemplate(masterTemplate)` | `create-template` with `masterTemplateAlias: "master"` |
| `_fileService.SaveTemplate(template)` | `update-template` |

### Partial views

| QuickBlocks (C#) | MCP Tool |
|---|---|
| `_fileService.SavePartialView()` (nav/footer) | `create-partial-view` at `Views/Partials/{name}.cshtml` |
| `StreamWriter` to `Views/Partials/blocklist/Components/{alias}.cshtml` | `create-partial-view` at `Views/Partials/blocklist/Components/{alias}.cshtml` |
| Hard-coded `Views/Partials/blocklist/default.cshtml` | `create-partial-view` at `Views/Partials/blocklist/default.cshtml` |

### Verification

| Purpose | MCP Tool |
|---|---|
| Confirm block list was created | `get-data-type` |
| Confirm element type exists | `get-document-type` |
| Check Umbraco version compatibility | `get-server-information` (called automatically on startup) |

---

## 6a. The Razor Template and Partial Pattern

This is the section most commonly misunderstood when comparing QuickBlocks to a manual build. QuickBlocks generates **five distinct types of Razor file**, each with a specific structure and a specific relationship to the others. The Claude skill must replicate all five.

### The five file types

```
master.cshtml                              ← Master template (Layout = null)
{pageAlias}.cshtml                         ← Content page template (Layout = "master.cshtml")
Views/Partials/{name}.cshtml               ← Nav, footer, and other structural partials
Views/Partials/blocklist/default.cshtml    ← Block list dispatcher (loops blocks → components)
Views/Partials/blocklist/Components/{alias}.cshtml  ← One per block type
```

---

### 1. Master template (`master.cshtml`)

QuickBlocks takes the **full HTML document** (the prototype with nav, header, footer, and a placeholder for the main content) and turns it into the master template.

**Three transformations are applied:**

**a) Main content area → `@RenderBody()`**

The element marked with `data-content-type-name` is the placeholder for page-specific content. QuickBlocks replaces the entire element with `@RenderBody()`:

```razor
@* Before *@
<main data-content-type-name="Home Page">
    <!-- page content placeholder -->
</main>

@* After (in master.cshtml) *@
@RenderBody()
```

**b) Nav/footer elements → `@await Html.PartialAsync(...)`**

Elements marked with `data-partial-name` are extracted and replaced with a partial call:

```razor
@* Before *@
<nav data-partial-name="navigation">
    <a href="/">Home</a>
    <a href="/about">About</a>
</nav>

@* After (in master.cshtml) *@
@await Html.PartialAsync("~/Views/Partials/navigation.cshtml")
```

The nav HTML itself is saved as `Views/Partials/navigation.cshtml` with a standard `@inherits UmbracoViewPage` header.

**c) All `data-*` attributes are stripped** before saving.

**Resulting `master.cshtml` structure:**

```razor
@* Auto-generated by QuickBlocks / Claude skill *@
<!DOCTYPE html>
<html>
<head>...</head>
<body>
    @await Html.PartialAsync("~/Views/Partials/navigation.cshtml")

    @RenderBody()

    @await Html.PartialAsync("~/Views/Partials/footer.cshtml")
</body>
</html>
```

**MCP call:** `create-template` with `name: "Master"`, `alias: "master"`, and the above as `content`.

---

### 2. Content page template (`{pageAlias}.cshtml`)

The content that *was* inside the `data-content-type-name` element becomes the content page template. This template:

- Sets `Layout = "master.cshtml"` (Umbraco passes this through `template.SetMasterTemplate()`)
- Inherits from the page's strongly-typed content model
- Calls `@Html.GetBlockListHtml(Model.{PropertyAlias})` for each block list property

**Resulting template structure:**

```razor
@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.HomePage>
    @using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
    Layout = "master.cshtml";
}

<main>
    <div class="main-content-wrapper">
        @Html.GetBlockListHtml(Model.MainContent)
    </div>
</main>
```

Where `@Html.GetBlockListHtml(Model.MainContent)` was generated by `RenderListPropertyCalls()` — replacing the element that originally had `data-list-name="Main Content"` with this call.

**MCP call:** `create-template` with `name: "Home Page"`, `alias: "homePage"`, `masterTemplateAlias: "master"`, and the above as `content`.

---

### 3. Navigation and footer partials

Any element in the master HTML marked `data-partial-name` is extracted and saved as its own partial view file at `Views/Partials/{name}.cshtml`:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage

<nav class="main-nav">
    <a href="/">Home</a>
    <a href="/about">About</a>
</nav>
```

These are **structural partials** — they don't receive a typed model unless Claude generates one. For the initial scaffold, a plain `@inherits UmbracoViewPage` (non-generic) is correct.

**MCP call:** `create-partial-view` at path `Views/Partials/navigation.cshtml`.

---

### 4. Block list dispatcher (`Views/Partials/blocklist/default.cshtml`)

This is a **fixed file** — it does not vary per site. It is the bridge between `@Html.GetBlockListHtml()` and the individual block component partials. QuickBlocks creates it once; the Claude skill should always create it.

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

This file reads each block's content type alias and renders the matching component partial by convention. No configuration required — adding a new block type just means adding a new file in `blocklist/Components/`.

**MCP call:** `create-partial-view` at path `Views/Partials/blocklist/default.cshtml`.

---

### 5. Block component partials (`Views/Partials/blocklist/Components/{alias}.cshtml`)

One file per block type. Each receives the `BlockListItem` model, casts it to the specific content and settings types, checks `settings.Hide`, and renders the block's HTML with Razor property bindings:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var row = (HeroRow)Model.Content;
    var settings = (HeroSettings)Model.Settings;

    if (settings.Hide) { return; }
}

<section class="hero">
    <h1>@row.Title</h1>
    @Html.Raw(row.BodyText)
    <img src="@Url.GetCropUrl(row.BackgroundImage, 1920, 600)" alt="@row.BackgroundImage?.Name" />
    @if (row.CallToAction != null)
    {
        <a href="@row.CallToAction.Url" target="@row.CallToAction.Target">@row.CallToAction.Name</a>
    }
</section>
```

**MCP call:** `create-partial-view` at path `Views/Partials/blocklist/Components/heroRow.cshtml`.

---

### How the five files connect at render time

```
Request for /home
    ↓
Umbraco selects homePage.cshtml (content template)
    ↓
Layout = "master.cshtml" wraps it
    ↓
master.cshtml renders:
    @await Html.PartialAsync("~/Views/Partials/navigation.cshtml")
    @RenderBody()          ← homePage.cshtml renders here
    @await Html.PartialAsync("~/Views/Partials/footer.cshtml")
    ↓
Inside @RenderBody(), homePage.cshtml calls:
    @Html.GetBlockListHtml(Model.MainContent)
    ↓
Umbraco finds default.cshtml (the dispatcher)
    ↓
default.cshtml loops each block and calls:
    @await Html.PartialAsync("blocklist/Components/heroRow", block)
    @await Html.PartialAsync("blocklist/Components/servicesRow", block)
    ↓
Each component partial renders its HTML
```

---

## 7. How the Skill Works — Step by Step

```mermaid
flowchart TD
    A[Developer invokes /umbraco-block-builder\nor describes task in natural language] --> B[Claude loads SKILL.md]

    B --> C{What type of input?}
    C -->|Plain HTML| D[Parse HTML semantically\nIdentify sections, headings, images, links]
    C -->|Natural language| E[Ask clarifying questions:\nblock names, data types, nesting]
    C -->|Screenshot| F[Describe visual structure\nthen treat as natural language]

    D & E & F --> G[Build block architecture model\nBlocks, properties, nesting, settings]

    G --> H[Present proposed structure to developer\nConfirm or adjust before any writes]

    H --> I[Resolve existing Umbraco state\nget-all-data-types to find Image Media Picker etc.\nget-all-document-types to avoid duplicates]

    I --> J[Create folder structure\ncreate-document-type-folder x N]

    J --> K[For each block type:\ncreate-element-type for content\ncreate-element-type for settings]

    K --> L[For each block list:\nresolve all element type GUIDs\ncreate-data-type with Umbraco.BlockList editor\nand full block configuration JSON]

    L --> M[Create page document type\ncreate-document-type with block list properties\nlink to templates]

    M --> N[Generate Razor partial views\nFor each block: correct model cast\nRazor expressions for each property type\ncreate-partial-view via MCP]

    N --> O[Create Master template\nCreate Content template\ncreate-template x2]

    O --> P[Verify all artefacts\nget-document-type, get-data-type\nReport what was created]

    P --> Q[Done — report summary to developer\nwith links to each created item]
```

### What Claude infers automatically (no annotation needed)

| HTML element | Inferred Umbraco data type |
|---|---|
| `<img>` | Image Media Picker |
| `<h1>`–`<h6>` | Textstring |
| `<p>` | Rich Text Editor |
| `<a>` (single) | Single URL Picker |
| `<a>` (repeated in a list) | Multi URL Picker |
| `<video>` | Media Picker |
| Repeated sibling groups | Nested block list |
| `<section>`, `<article>`, `<div>` with distinct content | Block type boundary |

Claude uses class names, IDs, content, and document structure to infer meaningful names. `<section class="hero">` becomes a block called `"Hero"`. A `<div class="service-items">` containing repeated `<div class="service-card">` elements becomes a nested block list called `"Service Items"` with an item type `"Service Card"`.

---

## 8. The Skill Definition

A skill is a `SKILL.md` file in `.claude/skills/umbraco-block-builder/`. It has two parts: YAML frontmatter that controls invocation, and a Markdown body that is the actual instruction set Claude follows.

```markdown
---
name: umbraco-block-builder
description: >
  Scaffolds a complete Umbraco 17 block list architecture from an HTML prototype or
  natural language description. Creates element types, settings types, block list data
  types, partial views, page content types, and templates via the Umbraco Developer MCP
  Server. Use when the developer wants to set up Umbraco block lists, create content
  types from HTML, or scaffold a block-based page structure.
user_invocable: true
---

# Umbraco Block Builder

You are scaffolding a complete Umbraco 17 block list architecture.
You have access to the Umbraco Developer MCP Server via the `umbraco-mcp` MCP connection.

## Ground Rules

- NEVER create anything without first presenting the proposed structure to the developer and
  getting confirmation. A mistake here creates real artefacts in a live CMS.
- ALWAYS check for existing artefacts before creating. Use `get-all-data-types` and
  `get-all-document-types` to avoid duplicates.
- Element types MUST be created before the block list data type that references them.
  The block list needs the element type GUIDs.
- The block list data type JSON for Umbraco 17 uses an array of `{ alias, value }` pairs
  in the `configuration` field — NOT a flat object. See the schema below.
- Partial views for block list components go at:
  `Views/Partials/blocklist/Components/{alias}.cshtml`

## Step 1 — Understand the Input

Accept any of:
- Plain HTML (no data-* annotation needed)
- Natural language description ("I need a hero, a 3-column grid, and a testimonials carousel")
- A mix of both

From the input, identify:
1. The page name (becomes the page document type)
2. Each distinct block type (becomes an element type)
3. Properties on each block (infer data types from HTML elements — see table below)
4. Whether any block contains a repeating sub-group (becomes a nested block list)
5. Whether any block needs a settings type (default: yes, with a Hide toggle)

## Step 2 — Infer Data Types

| HTML element or context | Umbraco data type | Notes |
|---|---|---|
| `<img>` | Image Media Picker | Use `get-all-data-types` to confirm the exact name |
| `<h1>`–`<h6>` | Textstring | |
| `<p>` | Rich Text Editor | |
| `<a>` (single) | Single URL Picker | |
| `<a>` (repeated / list) | Multi URL Picker | |
| `<video>` | Media Picker | |
| `<input type="text">` | Textstring | |
| `<textarea>` | Textarea | |
| `<select>` | Dropdown | |
| `<input type="checkbox">` | True/False | |
| Anything else | Textstring (default) | Ask if unsure |

Use `get-all-data-types` to resolve the exact GUID of each data type before building the
block list configuration. Never guess a GUID.

## Step 3 — Present the Proposed Structure

Before making any MCP calls that write data, output a structured summary:

```
## Proposed Block Architecture

**Page type:** Home Page (alias: homePage)

**Block list:** Main Content
  - Hero (element: heroRow)
      title: Textstring
      bodyText: Rich Text Editor
      backgroundImage: Image Media Picker
      callToAction: Single URL Picker
    settings: heroSettings (hide: True/False)

  - Services (element: servicesRow)
      title: Textstring
      [nested list] Service Items
          Service Card (element: serviceCardItem)
              title: Textstring
              description: Rich Text Editor
              icon: Image Media Picker
        settings: none

Does this look right? Reply YES to proceed, or tell me what to change.
```

Wait for confirmation before proceeding.

## Step 4 — Resolve Existing State

```
get-all-document-types   → build a map of name → id (avoid duplicates)
get-all-data-types       → build a map of name → id (resolve GUIDs for known types)
```

## Step 5 — Create in Dependency Order

Execute in this exact order (each step depends on the previous):

1. `create-document-type-folder` for each folder (Components, Elements/Content Blocks,
   Elements/Settings Models, Pages)
2. `create-element-type` for each block's content element type
3. `create-element-type` for each block's settings element type (with a Hide property)
4. `create-data-type` for each block list (using GUIDs from step 2–3)
5. `create-document-type` for the page type
6. `create-template` — Master template (see Step 7a)
7. `create-template` — Content page template (see Step 7b)
8. `create-partial-view` — Navigation partial (see Step 7c)
9. `create-partial-view` — Footer partial (see Step 7c)
10. `create-partial-view` — `Views/Partials/blocklist/default.cshtml` (see Step 7d)
11. `create-partial-view` — one per block type in `Views/Partials/blocklist/Components/` (see Step 7e)

## Step 6 — Block List Data Type JSON (Umbraco 17)

Use this exact `configuration` structure for `create-data-type`:

```json
{
  "name": "Main Content",
  "editorAlias": "Umbraco.BlockList",
  "editorUiAlias": "Umb.PropertyEditorUi.BlockList",
  "configuration": [
    {
      "alias": "blocks",
      "value": [
        {
          "contentElementTypeKey": "<GUID of content element type>",
          "settingsElementTypeKey": "<GUID of settings element type or null>",
          "label": "{{ !title || title == '' ? 'Hero' : title }}",
          "editorSize": "medium",
          "forceHideContentEditorInOverlay": false,
          "iconColor": "#1b264f",
          "backgroundColor": "#ffffff"
        }
      ]
    },
    { "alias": "validationLimit", "value": { "min": null, "max": null } },
    { "alias": "useSingleBlockMode", "value": false },
    { "alias": "useLiveEditing", "value": false },
    { "alias": "useInlineEditingAsDefault", "value": false },
    { "alias": "maxPropertyWidth", "value": "100%" }
  ]
}
```

IMPORTANT: Both `editorAlias` and `editorUiAlias` are required in Umbraco 17.

## Step 7a — Create the Master Template

The master template is the full HTML shell of the site (containing nav, header, footer, etc.).
The main content area is replaced with `@RenderBody()`. Nav/footer elements become
`@await Html.PartialAsync(...)` calls. All `data-*` attributes are stripped.

```razor
@* master.cshtml — no @inherits, no Layout *@
<!DOCTYPE html>
<html>
<head>
    <title>Site Name</title>
</head>
<body>
    @await Html.PartialAsync("~/Views/Partials/navigation.cshtml")

    @RenderBody()

    @await Html.PartialAsync("~/Views/Partials/footer.cshtml")
</body>
</html>
```

MCP call: `create-template` with `name: "Master"`, `alias: "master"`, and the above as `content`.

## Step 7b — Create the Content Page Template

The content page template contains only the main content area HTML (what was inside the
element marked as the page's content region). It inherits `master.cshtml` and calls
`@Html.GetBlockListHtml(Model.{PropertyAlias})` for each block list property on the page:

```razor
@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.{PageAlias}>
    @using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
    Layout = "master.cshtml";
}

<main>
    @Html.GetBlockListHtml(Model.{BlockListPropertyAlias})
</main>
```

MCP call: `create-template` with `name: "{Page Name}"`, `alias: "{pageAlias}"`,
`masterTemplateAlias: "master"`, and the above as `content`.

## Step 7c — Create Structural Partials (Nav, Footer, etc.)

For each element in the original HTML that should be a reusable partial (navigation, footer,
cookie banner, etc.), create a standalone partial view:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage

<nav class="main-nav">
    {original HTML of the nav element, data-* attributes stripped}
</nav>
```

MCP call: `create-partial-view` at `Views/Partials/{name}.cshtml` for each structural partial.

## Step 7d — Create the Block List Dispatcher

This file is always the same. It is the bridge between `@Html.GetBlockListHtml()` and the
individual block component partials. Create it once — it routes every block by convention:

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

## Step 7e — Create Block Component Partials

One file per block type. Each casts the `BlockListItem` to the block's content and settings
types, checks `settings.Hide`, and renders the block's HTML with Razor property bindings:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var row = ({ContentTypePascalAlias})Model.Content;
    var settings = ({SettingsTypePascalAlias})Model.Settings;

    if (settings.Hide) { return; }
}

{HTML with properties replaced by Razor expressions}
```

Property rendering rules:
- `Textstring` → `@row.{PropertyAlias}`
- `Rich Text Editor` → `@Html.Raw(row.{PropertyAlias})`
- `Image Media Picker` → `<img src="@Url.GetCropUrl(row.{PropertyAlias}, 800, 600)" alt="@row.{PropertyAlias}?.Name" />`
- `Single URL Picker` → `<a href="@row.{PropertyAlias}?.Url" target="@row.{PropertyAlias}?.Target">@row.{PropertyAlias}?.Name</a>`
- `True/False` → `@if (row.{PropertyAlias}) { ... }`
- Nested block list → `@Html.GetBlockListHtml(row.{PropertyAlias})`

MCP call: `create-partial-view` at `Views/Partials/blocklist/Components/{alias}.cshtml`.

## Step 8 — Verify and Report

After all creation steps, call:
- `get-document-type` for each created document type
- `get-data-type` for each created block list

Report a summary table:

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

## Error Handling

- If `create-element-type` returns a conflict (already exists), use the existing type's GUID
- If `create-partial-view` returns 404 for a subfolder path (known Umbraco bug #16823),
  create the partial in the root partials folder and note the workaround
- If a GUID lookup fails, call `get-data-type-search` with the type name as a fallback
```

---

## 9. MCP Configuration

`.mcp.json` in the project root:

```json
{
  "mcpServers": {
    "umbraco-mcp": {
      "command": "npx",
      "args": ["@umbraco-cms/mcp-dev@17"],
      "env": {
        "UMBRACO_CLIENT_ID": "${UMBRACO_CLIENT_ID}",
        "UMBRACO_CLIENT_SECRET": "${UMBRACO_CLIENT_SECRET}",
        "UMBRACO_BASE_URL": "${UMBRACO_BASE_URL}",
        "UMBRACO_INCLUDE_TOOL_COLLECTIONS": "document-type,data-type,template,partial-view,document"
      }
    }
  }
}
```

**Key environment variables** (set in shell or `.env`):

| Variable | Value |
|---|---|
| `UMBRACO_CLIENT_ID` | API User client ID from Umbraco back-office |
| `UMBRACO_CLIENT_SECRET` | API User secret |
| `UMBRACO_BASE_URL` | e.g. `https://localhost:44367` |

**Limiting to five tool collections** keeps the MCP tool footprint small (avoiding context bloat) and restricts permissions to only what the workflow needs.

---

## 10. Processing Pipeline Comparison

```mermaid
flowchart LR
    subgraph QB[QuickBlocks - Today]
        direction TB
        QB1[Developer annotates HTML\nwith data-* attributes] --> QB2[Paste into dashboard]
        QB2 --> QB3[C# parses HTML\nvia HtmlAgilityPack]
        QB3 --> QB4[C# calls Umbraco internal APIs\nIContentTypeService etc.]
        QB4 --> QB5[Artefacts created\nin Umbraco 10]
    end

    subgraph CS[Claude Skill - Proposed]
        direction TB
        CS1[Developer provides plain HTML\nor natural language] --> CS2[Claude reads SKILL.md\nunderstands workflow]
        CS2 --> CS3[Claude infers block structure\nnames, types, nesting]
        CS3 --> CS4[Claude confirms with developer\nbefore any writes]
        CS4 --> CS5[Claude calls MCP tools\nwhich call Management API]
        CS5 --> CS6[Artefacts created\nin Umbraco 17]
    end
```

| Dimension | QuickBlocks | Claude Skill |
|---|---|---|
| **Input** | Annotated HTML only | Plain HTML, natural language, screenshot |
| **Umbraco version** | 10 (internal APIs) | 17 (Management API via MCP) |
| **Naming** | Developer must name everything via `data-*` | Inferred from HTML semantics; confirmed before creation |
| **Data type resolution** | Developer must know Umbraco type names | Inferred from element type; resolved by GUID lookup |
| **Iteration** | Re-annotate and re-run | Conversational — "rename that", "add a field", "skip settings" |
| **Explanation** | Silent | Explains every decision; shows proposed structure before writing |
| **Maintenance** | C# NuGet package per Umbraco major version | `SKILL.md` update (Markdown) |
| **Error recovery** | Partial creation, hard to debug | Can explain what failed, retry, or suggest alternatives |
| **Custom data types** | Requires C# `IDataTypeMapper` implementation | Tell Claude in natural language |
| **Dependencies** | Umbraco.Cms.Core, HtmlAgilityPack, NuGet | `npx` (no install), one JSON config file |

---

## 11. What the Developer Does Differently

### Before (QuickBlocks)

1. Write HTML prototype
2. Re-annotate every meaningful element with `data-row-name`, `data-prop-name`, `data-prop-type`, `data-list-name`, etc.
3. Open the Umbraco back-office, navigate to the QuickBlocks dashboard
4. Paste annotated HTML
5. Click Submit
6. Debug any failures by re-annotating and re-running

### After (Claude Skill)

1. Write HTML prototype (or describe what you need)
2. Type `/umbraco-block-builder` in Claude Code (or just describe the task)
3. Review and confirm the proposed block architecture
4. Done

The annotation step — which required deep Umbraco knowledge — is gone. The developer reviews Claude's proposal rather than writing the proposal themselves.

---

## 12. Worked Example

**Input (no annotation):**

```html
<main>
  <section class="hero">
    <h1>Welcome to Acme</h1>
    <p>We build things that matter.</p>
    <img src="hero.jpg" alt="Hero image">
    <a href="/about">Learn More</a>
  </section>

  <section class="services">
    <h2>Our Services</h2>
    <div class="service-grid">
      <div class="service-card">
        <img src="icon.svg" alt="icon">
        <h4>Service Name</h4>
        <p>Service description here.</p>
      </div>
      <div class="service-card">...</div>
      <div class="service-card">...</div>
    </div>
  </section>
</main>
```

**Claude's proposed structure (before any writes):**

```
Page type: Home Page (alias: homePage)

Block list: Main Content
├── Hero (element: heroRow)
│     title: Textstring
│     bodyText: Rich Text Editor
│     image: Image Media Picker
│     link: Single URL Picker
│   settings: heroSettings
│     hide: True/False
│
└── Services (element: servicesRow)
      title: Textstring
      [nested block list] Service Grid
          Service Card (element: serviceCardItem)
              icon: Image Media Picker
              title: Textstring
              description: Rich Text Editor
        settings: none

Does this look right? Reply YES to proceed, or tell me what to change.
```

**After confirmation, MCP calls in order:**

```
1.  create-document-type-folder  → "Elements"
2.  create-document-type-folder  → "Elements/Content Models"
3.  create-document-type-folder  → "Elements/Settings Models"
4.  create-document-type-folder  → "Pages"
5.  get-all-data-types           → resolve GUIDs for Image Media Picker, Textstring, etc.
6.  create-element-type          → heroRow
7.  create-element-type          → heroSettings
8.  create-element-type          → servicesRow
9.  create-element-type          → serviceCardItem
10. create-data-type             → "Service Grid" (block list, references serviceCardItem)
11. create-data-type             → "Main Content" (block list, references heroRow + servicesRow)
12. create-document-type         → homePage (with mainContent property)

    ── Template generation ──────────────────────────────────────────────────

13. create-template              → master.cshtml
                                   (full HTML with @RenderBody() where <main> was,
                                    @await Html.PartialAsync for nav and footer)

14. create-template              → homePage.cshtml
                                   (Layout = "master.cshtml",
                                    @inherits UmbracoViewPage<ContentModels.HomePage>,
                                    @Html.GetBlockListHtml(Model.MainContent))

    ── Structural partials ──────────────────────────────────────────────────

15. create-partial-view          → Views/Partials/navigation.cshtml
                                   (@inherits UmbracoViewPage + nav HTML)

16. create-partial-view          → Views/Partials/footer.cshtml
                                   (@inherits UmbracoViewPage + footer HTML)

    ── Block list wiring ────────────────────────────────────────────────────

17. create-partial-view          → Views/Partials/blocklist/default.cshtml
                                   (fixed dispatcher: loops blocks →
                                    @await Html.PartialAsync("blocklist/Components/" +
                                    data.ContentType.Alias, block))

    ── Block component partials ─────────────────────────────────────────────

18. create-partial-view          → Views/Partials/blocklist/Components/heroRow.cshtml
                                   (cast to HeroRow + HeroSettings, if (settings.Hide) return,
                                    HTML with @row.Title, @Html.Raw(row.BodyText), etc.)

19. create-partial-view          → Views/Partials/blocklist/Components/servicesRow.cshtml
                                   (cast to ServicesRow, @Html.GetBlockListHtml(row.ServiceItems))

20. create-partial-view          → Views/Partials/blocklist/Components/serviceCardItem.cshtml
                                   (cast to ServiceCardItem, render icon/title/description)

    ── Verification ─────────────────────────────────────────────────────────

21. get-document-type            → verify heroRow ✓
22. get-document-type            → verify servicesRow ✓
23. get-data-type                → verify Main Content ✓
```

**Generated `homePage.cshtml`:**

```razor
@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.HomePage>
    @using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
    Layout = "master.cshtml";
}

<main>
    @Html.GetBlockListHtml(Model.MainContent)
</main>
```

**Generated `master.cshtml`:**

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <title>Acme</title>
</head>
<body>
    @await Html.PartialAsync("~/Views/Partials/navigation.cshtml")

    @RenderBody()

    @await Html.PartialAsync("~/Views/Partials/footer.cshtml")
</body>
</html>
```

**Generated `Views/Partials/blocklist/default.cshtml`:**

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

**Generated `Views/Partials/blocklist/Components/heroRow.cshtml`:**

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var row = (HeroRow)Model.Content;
    var settings = (HeroSettings)Model.Settings;

    if (settings.Hide) { return; }
}

<section class="hero">
    <h1>@row.Title</h1>
    @Html.Raw(row.BodyText)
    <img src="@Url.GetCropUrl(row.Image, 1920, 600)" alt="@row.Image?.Name" />
    @if (row.Link != null)
    {
        <a href="@row.Link.Url" target="@row.Link.Target">@row.Link.Name</a>
    }
</section>
```

---

## 13. Known Considerations

### Partial view subfolder bug

Umbraco has a known issue ([#16823](https://github.com/umbraco/Umbraco-CMS/issues/16823)) where `partial-view` API calls to paths at depth > 1 (e.g. `blocklist/Components/hero.cshtml`) return 404. The skill's instructions note this and fall back to placing files at root level if needed. This is a bug in Umbraco, not in the skill.

### GUID dependency ordering

The block list data type JSON requires the GUIDs of element types that must already exist. The skill enforces strict creation order to handle this. Claude will never attempt to create the block list data type before all referenced element types are confirmed created.

### Umbraco version

The MCP server is versioned to match Umbraco CMS (`@umbraco-cms/mcp-dev@17` for Umbraco 17). The block list configuration JSON structure changed between Umbraco 10 and 17 — the skill uses the v17 format (array of `{ alias, value }` pairs). Updating to Umbraco 18 requires only updating the SKILL.md schema section and the npm version pin.

### API user permissions

The MCP server authenticates as an Umbraco API User. Creating document types and data types requires admin-level permissions. A non-admin API user can still use the skill to manage content only. The skill should be connected to a **non-production environment** during scaffolding.

### Context budget

The five MCP tool collections (`document-type`, `data-type`, `template`, `partial-view`, `document`) expose roughly 100–150 tools. This is within normal context budget. If MCP Tool Search activates (Sonnet 4+ / Opus 4+), tools are loaded on demand transparently — no action needed.

---

## 14. Future Possibilities

### Figma / screenshot input

Claude's vision capability means the skill could accept a Figma screenshot or a design mockup as input. Claude identifies sections, repeating patterns, and content zones visually, then follows the same block builder workflow.

### Reverse scaffolding

Given an existing Umbraco site, use `get-all-document-types` and `get-all-data-types` to understand the current structure, then generate the HTML prototype that would have produced it — useful for documenting legacy sites.

### Umbraco.AI Agents alignment

Umbraco's official roadmap (Q2 2026) includes **Umbraco.AI Agents** for content modelling and multi-step scaffolding workflows. The pattern described in this document — skill + MCP — directly anticipates that direction. A skill built today could be migrated into the official agent framework with minimal changes.

### Live preview

After creating the block list and partial views, trigger a Umbraco Models Builder rebuild (via MCP or CLI) and render a preview of a test page using the new block types — giving the developer instant visual confirmation.

### Content population

After scaffolding the structure, use `create-document` and `update-document-properties` to populate initial content — turning a block scaffold into a full working prototype page.

---

## References

| Resource | URL |
|---|---|
| Umbraco Developer MCP Server | [docs.umbraco.com/umbraco-cms/reference/developer-mcp](https://docs.umbraco.com/umbraco-cms/reference/developer-mcp) |
| MCP Available Tools | [docs.umbraco.com/umbraco-cms/reference/developer-mcp/available-tools](https://docs.umbraco.com/umbraco-cms/reference/developer-mcp/available-tools) |
| MCP GitHub | [github.com/umbraco/Umbraco-CMS-MCP-Dev](https://github.com/umbraco/Umbraco-CMS-MCP-Dev) |
| Management API | [docs.umbraco.com/umbraco-cms/reference/management-api](https://docs.umbraco.com/umbraco-cms/reference/management-api) |
| Claude Skills docs | [code.claude.com/docs/en/skills](https://code.claude.com/docs/en/skills) |
| Claude Agent SDK | [platform.claude.com/docs/en/agent-sdk/overview](https://platform.claude.com/docs/en/agent-sdk/overview) |
| MCP + Agent SDK | [platform.claude.com/docs/en/agent-sdk/mcp](https://platform.claude.com/docs/en/agent-sdk/mcp) |
| Block list v17 forum thread | [forum.umbraco.com/t/programmatically-creating-blocklist-data-type-v17/6950](https://forum.umbraco.com/t/programmatically-creating-blocklist-data-type-v17/6950) |
| Partial view depth bug | [github.com/umbraco/Umbraco-CMS/issues/16823](https://github.com/umbraco/Umbraco-CMS/issues/16823) |
| Umbraco.AI Agents roadmap | [umbraco.com/products/knowledge-center/roadmap/](https://umbraco.com/products/knowledge-center/roadmap/) |

---

*Proposal based on Umbraco Developer MCP Server v17, Claude Agent SDK, and the Claude Code Skills system.*
