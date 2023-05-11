# QuickBlocks
A package for Umbraco which builds up your Umbraco block lists and other Umbraco stuff just from your HTML

Here are some examples

## Home Page and Block List Property

```html
<div data-content-type-name="Home Page" 
     data-prop-name="Main Content" 
     data-prop-type="[BlockList] Main Content" 
     data-prop-location="page" 
     data-list-name="Main Content">
  ...
</div>
```

## Add a row

```html
<section data-row-name="Simple Link">
...
</section>
```

## Add a property to the row

```html
<a href="#" data-prop-name="Link" data-prop-location="row">My Link</a>

<img src="#" data-prop-name="Image" data-prop-location="row" />

<h2 data-prop-name="Image" data-prop-location="row">

<p data-prop-name="Description" data-prop-location="row">
     My content in here
</p>
```

## Specify a different data type
```html
<h2 data-prop-name="Title" data-prop-type="Richtext editor" data-prop-location="row">
```

## Use an image as a background image
```html
<header style="background-image: url('[!image!]')" data-row-name="Header" data-prop-name="Image" data-prop-location="row" data-replace-marker="[!image!]" data-replace-inner="false" data-prop-type="Image Media Picker">
     ...
</header>
```

## Use a Multi URL Picker for repeating links and use the name for the icon
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

## Create a list property inside a row
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
