using HtmlAgilityPack;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Community.QuickBlocks.Models;

public class RowModel
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public string SettingsName { get; set; }
    public string SettingsAlias { get; set; }
    public IEnumerable<PropertyModel> Properties { get; set; }
    public bool IgnoreNamingConvention { get; set; }
    public bool HasSettings { get; set; }
    public string Html { get; set; }
    public string IconClass { get; set; }
    public List<BlockListModel> SubLists { get; set; }
    public string LabelProperty { get; set; }

    public RowModel(IShortStringHelper shortStringHelper, string name, HtmlNode node, 
        string settingsName, bool hasSettings = true, bool ignoreNamingConvention = false, 
        string suffix = "Row", string settingsSuffix = "Settings", string iconClass = "icon-science", string labelProperty = "Title")
    {
        IgnoreNamingConvention = ignoreNamingConvention;
        HasSettings = hasSettings;

        if (ignoreNamingConvention)
        {
            Name = name;
            SettingsName = hasSettings ? settingsName : "";
        }
        else
        {
            Name = name + " " + suffix;
            SettingsName = hasSettings ? Name + " " + settingsSuffix : "";
        }

        Alias = Name.Replace(" ", "").ToSafeAlias(shortStringHelper, true);
        SettingsAlias = hasSettings ? SettingsName.Replace(" ", "").ToSafeAlias(shortStringHelper, true) : "";
        Html = node.OuterHtml;
        IconClass = iconClass;
        LabelProperty = labelProperty.Replace(" ", "").ToSafeAlias(shortStringHelper, true);
    }
}