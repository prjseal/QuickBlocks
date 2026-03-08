# Umbraco Block Builder — Claude Skill

The Claude skill replaces the QuickBlocks C# package for Umbraco 17+ projects. Instead of
annotating HTML with `data-*` attributes and running a back-office dashboard, you hand Claude
a plain HTML prototype (or describe what you need in plain English) and it scaffolds the
entire block list architecture for you via the Umbraco Developer MCP Server.

---

## Prerequisites

| Requirement | Details |
|---|---|
| **Umbraco** | 17+ with the Management API enabled (built-in from Umbraco 14+) |
| **Node.js** | v18+ (for `npx`) |
| **Claude Code** | Any version, or Claude.ai with skill upload support |
| **Umbraco API User** | An API User with admin permissions — created in the Umbraco back-office |

---

## 1. Create an Umbraco API User

The MCP server authenticates against Umbraco using OAuth2 client credentials.

1. Log in to the Umbraco back-office
2. Go to **Settings → API Users → Create**
3. Set permissions to **Administrator** (required for creating document types and data types)
4. Copy the **Client ID** and **Client Secret** — you'll need these in the next step
5. Use this against a **development or staging instance only**, not production

---

## 2. Configure Environment Variables

Add these to your shell profile (`.zshrc`, `.bashrc`) or a `.env` file in your project root.
Never commit secrets to source control.

```bash
export UMBRACO_CLIENT_ID="your-client-id-here"
export UMBRACO_CLIENT_SECRET="your-client-secret-here"
export UMBRACO_BASE_URL="https://localhost:44367"
```

The `.mcp.json` file at the root of this repo reads these variables automatically:

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

---

## 3. Add the Skill to Claude

### Option A — Claude Code (recommended)

The skill is already in this repository at `.claude/skills/umbraco-block-builder/`. When
you open this project in Claude Code, the skill is available automatically. No extra steps.

Verify it's loaded by asking Claude:

```
When would you use the umbraco-block-builder skill?
```

Claude should describe the scaffold workflow. If it doesn't, check that the
`.claude/skills/umbraco-block-builder/SKILL.md` file exists.

### Option B — Claude.ai

1. Zip the skill folder:
   ```bash
   cd .claude/skills
   zip -r umbraco-block-builder.zip umbraco-block-builder/
   ```
2. In Claude.ai, go to **Settings → Capabilities → Skills**
3. Upload `umbraco-block-builder.zip`
4. Confirm the skill appears in your skills list

> Note: Claude.ai does not have access to your local MCP server. Use Claude Code for the
> full workflow including MCP tool calls. Claude.ai is useful for planning and reviewing
> proposed structures without executing them.

---

## 4. Verify the MCP Connection

In Claude Code, with your Umbraco instance running, ask:

```
Using the umbraco-mcp MCP server, call get-server-information and tell me the Umbraco version.
```

If it returns a version number, the connection is working. If it fails, check:

- Your Umbraco instance is running and reachable at `UMBRACO_BASE_URL`
- The API User credentials are correct
- `npx` is available in your terminal (`npx --version`)

---

## 5. Using the Skill

### Invoking the skill

You can trigger the skill in any of these ways:

```
/umbraco-block-builder
```

Or just describe what you want — Claude will load the skill automatically:

```
Scaffold this HTML prototype as an Umbraco block list page
```

```
Set up Umbraco blocks for a hero, services grid, and testimonials section
```

```
Create content types from this HTML
```

### Providing input

The skill accepts any of these — no `data-*` annotation required:

**Plain HTML:**
```
Here's my prototype HTML, please scaffold it as Umbraco blocks:

<section class="hero">
  <h1>Welcome</h1>
  <p>Intro text here.</p>
  <img src="hero.jpg" alt="">
  <a href="/about">Learn More</a>
</section>

<section class="features">
  <div class="feature-card">
    <img src="icon.svg" alt="">
    <h3>Feature Title</h3>
    <p>Feature description.</p>
  </div>
</section>
```

**Natural language:**
```
I need a home page with a full-width hero (heading, body text, background image, CTA link),
a 3-column services grid where each card has an icon, title, and description,
and a testimonials carousel with quote text, author name, and author photo.
```

**Mixed:**
```
Here's the hero HTML — [paste HTML]. The rest of the page needs a services section
with three cards, each with an image and a Rich Text body.
```

### The workflow

The skill follows a confirm-before-write pattern:

1. **You provide input** — HTML, description, or both
2. **Claude proposes a structure** — a list of block types, properties, and data types
3. **You review and confirm** — reply `YES` or ask for changes
4. **Claude creates everything** — all MCP calls happen in the correct dependency order
5. **Claude reports** — a summary table showing what was created and its status

**Example exchange:**

```
You: /umbraco-block-builder — [paste HTML]

Claude:
## Proposed Block Architecture

**Page type:** Home Page (alias: homePage)

**Block list:** Main Content
  ├── Hero (element: heroRow)
  │     title: Textstring
  │     bodyText: Rich Text Editor
  │     image: Image Media Picker
  │     link: Single URL Picker
  │   settings: heroSettings (hide: True/False)
  │
  └── Features (element: featuresRow)
        [nested list] Feature Items
            Feature Card (element: featureCardItem)
                icon: Image Media Picker
                title: Textstring
                description: Rich Text Editor
      settings: none

Does this look right? Reply YES to proceed, or tell me what to change.

You: YES

Claude: [creates 15–20 artefacts in sequence, reports summary table]
```

### Iterating

The workflow is conversational. After the initial scaffold you can continue:

```
Add a "Background Colour" dropdown to the Hero settings
```

```
Rename "Features" to "Services" throughout
```

```
The hero doesn't need a link — remove it
```

---

## 6. What Gets Created

For a typical page with 2–3 block types, the skill creates:

| Artefact | Location in Umbraco |
|---|---|
| Document type folder | Settings → Document Types → Elements |
| Element type per block | Settings → Document Types → Elements/Content Models |
| Settings type per block | Settings → Document Types → Elements/Settings Models |
| Page document type | Settings → Document Types → Pages |
| Block list data type | Settings → Data Types |
| Master template | Settings → Templates → Master |
| Content page template | Settings → Templates → {Page Name} |
| Navigation partial | Views/Partials/navigation.cshtml |
| Footer partial | Views/Partials/footer.cshtml |
| Block list dispatcher | Views/Partials/blocklist/default.cshtml |
| Component partial per block | Views/Partials/blocklist/Components/{alias}.cshtml |

---

## 7. Troubleshooting

**Skill doesn't trigger automatically**

Ask Claude directly: `/umbraco-block-builder` — this forces it to load. If the skill body
never appears in the response, check that `SKILL.md` exists at
`.claude/skills/umbraco-block-builder/SKILL.md`.

**MCP connection fails**

```
Error: connect ECONNREFUSED
```

- Check `UMBRACO_BASE_URL` — include the port, e.g. `https://localhost:44367`
- Check that Umbraco is running (`dotnet run` or IIS/Kestrel)
- Check that the API User credentials match what's in the back-office

**Partial view subfolder returns 404**

This is a known Umbraco bug ([#16823](https://github.com/umbraco/Umbraco-CMS/issues/16823))
affecting paths deeper than one level (e.g. `blocklist/Components/hero.cshtml`). The skill
will note the workaround in its summary and place the file at root level if needed.

**Duplicate artefact error**

If you run the skill twice on the same project, it will detect existing artefacts via
`get-all-document-types` / `get-all-data-types` and reuse their GUIDs instead of creating
duplicates.

**Wrong Umbraco version**

The block list configuration JSON changed between Umbraco 10 and 17. The skill targets v17.
If `get-server-information` returns a version below 17, the skill will warn you before
proceeding.

---

## Comparison with the QuickBlocks C# Package

| | QuickBlocks (C# package) | Claude Skill |
|---|---|---|
| **Umbraco version** | 10 | 17+ |
| **Input** | Annotated HTML (`data-*` attributes) | Plain HTML, natural language, screenshot |
| **Annotation required** | Yes — developer must name every property | No — Claude infers from HTML semantics |
| **Iteration** | Re-annotate and re-run | Conversational — describe changes in plain English |
| **Maintenance** | NuGet release per Umbraco major version | Edit one Markdown file |
| **Dependencies** | `Umbraco.Cms.Core`, `HtmlAgilityPack` | `npx` (no install), one JSON config |
| **Error recovery** | Partial creation, hard to debug | Claude explains failures and offers alternatives |
