# Modifying the default configuration

## Adding DataType mappers

QuickBlocks includes a [collection](https://docs.umbraco.com/umbraco-cms/implementation/composing#collections) of mappers used to link html element to default Umbraco data types.

For example, when the parser finds an `h1` element, it creates a property that uses a `textstring` datatype for it. You can modify this behaviour by modifying the mappers collection.

First we need to create a new mapper. To do so, create a new class that implements `IDataTypeMapper`.

```csharp
public class TextareaDataTypeMapper : IDataTypeMapper
{
    public IEnumerable<string> HtmlElements => new[] {"h1"} ;

    public string DataTypeName => "textarea";
}
```

Next, we need to add our mapper to the QuickBlocks collection. Y For this to work you will need to create a [composer](https://docs.umbraco.com/umbraco-cms/implementation/composing).

Then add the type mapper using the `QuickBlockDataTypeMappers` extension.

> ℹ️ Note that QuickBlocks come with a `HeadersDataTypeMapper` that maps `h1` to a `textstring` datatype. In order for the new mapper to be able to override the default configuration, we need to append it to the collection after `HeadersDataTypeMapper` using `InsertAfter`


```csharp

internal class MyDataTypeMappersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {        

        builder.QuickBlockDataTypeMappers()
                    .InsertAfter<HeadersDataTypeMapper, TextareaDataTypeMapper>();               
    }
}

```

## Changing the default data type mapper
If a mapper is not found for a given html element, QuickBlocks will create a property using a `textstring` data type.

This datatype can be modify. To do this, you need to go to your `Startup.cs` file and in the `ConfigureServices` method, you can configure the default options.

```csharp   
services.Configure<QuickBlocksDefaultOptions>(cfg =>
{
    cfg.DefaultDataTypeName = "Textarea";
});
```

