using System.Collections.Generic;

using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace QuickBlocks.Models;
public class ContentTypeModel
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public string ConventionName { get; }
    public IEnumerable<PropertyModel> Properties { get; set; }

    public ContentTypeModel(IShortStringHelper shortStringHelper, string name)
    {
        Name = name;
        Alias = Name.Replace(" ", "").ToSafeAlias(shortStringHelper, true);
    }
}
