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
    public IEnumerable<BlockListModel> Lists { get; set; }
    public string Html { get; set; }

    public ContentTypeModel(IShortStringHelper shortStringHelper, string name, string html)
    {
        Name = name;
        Alias = Name.ToSafeAlias(shortStringHelper, true);
        Html = html;
    }
}
