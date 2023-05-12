# QuickBlocks

[![Downloads](https://img.shields.io/nuget/dt/Umbraco.Community.QuickBlocks?color=cc9900)](https://www.nuget.org/packages/Umbraco.Community.QuickBlocks/)
[![NuGet](https://img.shields.io/nuget/vpre/Umbraco.Community.QuickBlocks?color=0273B3)](https://www.nuget.org/packages/Umbraco.Community.QuickBlocks)
[![GitHub license](https://img.shields.io/github/license/prjseal/QuickBlocks?color=8AB803)](LICENSE)

A package for quickly building block list based Umbraco websites all from data attributes in your HTMl

## Installation

Add the package to an existing Umbraco website (v10.4+) from nuget:

`dotnet add package Umbraco.Community.QuickBlocks`


## Data Attributes

Here are some examples

### Home Page and Block List Property

```html
<div data-content-type-name="Home Page" 
     data-prop-name="Main Content" 
     data-prop-type="[BlockList] Main Content" 
     data-prop-location="page" 
     data-list-name="Main Content">
  ...
</div>
```

### Add a row

```html
<section data-row-name="Simple Link">
...
</section>
```

### Add a property to the row

```html
<a href="#" data-prop-name="Link" data-prop-location="row">My Link</a>

<img src="#" data-prop-name="Image" data-prop-location="row" />

<h2 data-prop-name="Image" data-prop-location="row">

<p data-prop-name="Description" data-prop-location="row">
     My content in here
</p>
```

### Specify a different data type
```html
<h2 data-prop-name="Title" data-prop-type="Richtext editor" data-prop-location="row">
```

### Use an image as a background image
```html
<header style="background-image: url('[!image!]')" data-row-name="Header" data-prop-name="Image" data-prop-location="row" data-replace-marker="[!image!]" data-replace-inner="false" data-prop-type="Image Media Picker">
     ...
</header>
```

### Use a Multi URL Picker for repeating links and use the name for the icon
```html
<a href="#" data-prop-name="Social Links" 
     data-prop-type="Multi URL Picker" 
     data-multiple="true" 
     data-replace-attribute="class"  
     data-replace-inner="true" 
     data-replace-marker="[!name!]" 
     data-prop-value=".Name">
          <i class="fa fa-[!name!]"></i>
</a>
```

### Create a list property inside a row
In the sub list items, we don't need to specify the property location, we only do that for row or page properties.

```html
<div data-row-name="Services">
     <h2 class="title" data-prop-name="Title" data-prop-location="row">We build awesome products</h2>
     <h5 class="description" data-prop-name="Description" data-prop-location="row">This is the paragraph where you can write more details </h5>
     <div data-sub-list-name="Service List" 
          data-prop-name="Services" 
          data-prop-type="[BlockList] Service List" 
          data-prop-location="row" 
          data-list-inline="false">
          
          <div data-item-name="Service Item">
               <h4 class="info-title" data-prop-name="Title">1. Design</h4>
               <p data-prop-name="Description">blah blah blah</p>
               <a data-prop-name="Link" href="#pablo">Find more...</a>
          </div>
     </div>
</div>
```

### Move some HTML to a partial view

```html
<footer data-partial-name="Footer">
     ...
</footer>
```

### Extra block list options

#### Max Width

```html
data-list-maxwidth="100%"
```

#### Single block mode

```html
data-list-single="true"
```
#### Live editing mode

```html
data-list-live="true"
```
#### Inline Editing

```html
data-list-inline="true"
```

#### List Min Items

```html
data-list-min="0"
```

#### List Max Items

```html
data-list-max="3"
```

### Extra row options

#### Block Row Icon

```html
data-icon-class="icon-science"
```

#### Block Row Icon Colour

```html
data-icon-colour="color-indigo"
```

#### Block Row Label Property

```html
data-label-property="title"
```

## Contributing

Contributions to this package are most welcome! Please read the [Contributing Guidelines](CONTRIBUTING.md).

## Acknowledgments

Thanks to my employers [ClerksWell](https://www.clerkswell.com) for allowing me some time during my work day to work on this project on top of my own spare time.