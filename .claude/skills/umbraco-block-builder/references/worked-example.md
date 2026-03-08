# Worked Example: Hero + Services Page

A complete end-to-end example showing input → proposed structure → MCP call sequence → generated Razor files.

---

## Input (unannotated HTML)

```html
<body>
  <nav class="main-nav">
    <a href="/">Home</a>
    <a href="/about">About</a>
  </nav>

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
      </div>
    </section>
  </main>

  <footer class="site-footer">
    <p>&copy; 2025 Acme</p>
  </footer>
</body>
```

---

## Proposed Structure (shown to developer before any writes)

```
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
  └── Services (element: servicesRow)
        title: Textstring
        [nested list] Service Grid
            Service Card (element: serviceCardItem)
                icon: Image Media Picker
                title: Textstring
                description: Rich Text Editor
      settings: none

Does this look right? Reply YES to proceed, or tell me what to change.
```

---

## MCP Call Sequence (after developer says YES)

```
 1.  create-document-type-folder  → "Elements"
 2.  create-document-type-folder  → "Elements/Content Models"
 3.  create-document-type-folder  → "Elements/Settings Models"
 4.  create-document-type-folder  → "Pages"
 5.  get-all-data-types           → resolve GUIDs for Textstring, Rich Text Editor,
                                     Image Media Picker, Single URL Picker, True/False
 6.  create-element-type          → heroRow
 7.  create-element-type          → heroSettings (with hide: True/False)
 8.  create-element-type          → servicesRow
 9.  create-element-type          → serviceCardItem
10.  create-data-type             → "Service Grid" (nested block list, references serviceCardItem)
11.  create-data-type             → "Main Content" (references heroRow + servicesRow)
12.  create-document-type         → homePage (with mainContent block list property)

     ── Template generation ──────────────────────────────────────────────────

13.  create-template              → master.cshtml
14.  create-template              → homePage.cshtml (masterTemplateAlias: "master")

     ── Structural partials ──────────────────────────────────────────────────

15.  create-partial-view          → Views/Partials/navigation.cshtml
16.  create-partial-view          → Views/Partials/footer.cshtml

     ── Block list wiring ────────────────────────────────────────────────────

17.  create-partial-view          → Views/Partials/blocklist/default.cshtml

     ── Block component partials ─────────────────────────────────────────────

18.  create-partial-view          → Views/Partials/blocklist/Components/heroRow.cshtml
19.  create-partial-view          → Views/Partials/blocklist/Components/servicesRow.cshtml
20.  create-partial-view          → Views/Partials/blocklist/Components/serviceCardItem.cshtml

     ── Verification ─────────────────────────────────────────────────────────

21.  get-document-type            → verify heroRow ✓
22.  get-document-type            → verify servicesRow ✓
23.  get-data-type                → verify "Main Content" ✓
```

---

## Generated Files

### `master.cshtml`

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

---

### `homePage.cshtml`

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

---

### `Views/Partials/navigation.cshtml`

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage

<nav class="main-nav">
    <a href="/">Home</a>
    <a href="/about">About</a>
</nav>
```

---

### `Views/Partials/blocklist/default.cshtml`

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

---

### `Views/Partials/blocklist/Components/heroRow.cshtml`

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var row      = (HeroRow)Model.Content;
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

### `Views/Partials/blocklist/Components/servicesRow.cshtml`

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var row = (ServicesRow)Model.Content;
}

<section class="services">
    <h2>@row.Title</h2>
    <div class="service-grid">
        @Html.GetBlockListHtml(row.ServiceGrid)
    </div>
</section>
```

---

### `Views/Partials/blocklist/Components/serviceCardItem.cshtml`

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>

@{
    var item = (ServiceCardItem)Model.Content;
}

<div class="service-card">
    <img src="@Url.GetCropUrl(item.Icon, 80, 80)" alt="@item.Icon?.Name" />
    <h4>@item.Title</h4>
    @Html.Raw(item.Description)
</div>
```

---

## Final Summary Table

```
## Build Complete

| Artefact | Name | Status |
|---|---|---|
| Element type | heroRow | ✓ Created |
| Element type | heroSettings | ✓ Created |
| Element type | servicesRow | ✓ Created |
| Element type | serviceCardItem | ✓ Created |
| Block list data type | Service Grid | ✓ Created |
| Block list data type | Main Content | ✓ Created |
| Page document type | homePage | ✓ Created |
| Template | master.cshtml | ✓ Created |
| Template | homePage.cshtml | ✓ Created |
| Structural partial | navigation.cshtml | ✓ Created |
| Structural partial | footer.cshtml | ✓ Created |
| Block list dispatcher | blocklist/default.cshtml | ✓ Created |
| Component partial | blocklist/Components/heroRow.cshtml | ✓ Created |
| Component partial | blocklist/Components/servicesRow.cshtml | ✓ Created |
| Component partial | blocklist/Components/serviceCardItem.cshtml | ✓ Created |
```
