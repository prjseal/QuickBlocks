using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Umbraco.Cms.Core.Strings;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using QuickBlocks.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using File = System.IO.File;
using System.Data;
using System.Text.Json;

namespace QuickBlocks.Services
{
    public class BlockParsingService : IBlockParsingService
    {
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataTypeService _dataTypeService;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IDataValueEditorFactory _dataValueEditorFactory;
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
        private readonly PropertyEditorCollection _propertyEditorCollection;
        private readonly IContentTypeService _contentTypeService;

        public BlockParsingService(IShortStringHelper shortStringHelper, IWebHostEnvironment webHostEnvironment, IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper, IDataValueEditorFactory dataValueEditorFactory, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer, PropertyEditorCollection propertyEditorCollection, IContentTypeService contentTypeService)
        {
            _shortStringHelper = shortStringHelper;
            _webHostEnvironment = webHostEnvironment;
            _dataTypeService = dataTypeService;
            _umbracoMapper = umbracoMapper;
            _dataValueEditorFactory = dataValueEditorFactory;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
            _propertyEditorCollection = propertyEditorCollection;
            _contentTypeService = contentTypeService;
        }

        public List<RowModel> GetRows(HtmlNode node)
        {
            var rows = new List<RowModel>();

            var rowNodes = node.SelectNodes("//*[@data-row-name]");

            if (rowNodes == null || !rowNodes.Any()) return rows;

            foreach (var rowNode in rowNodes)
            {
                var rowName = rowNode.GetAttributeValue("data-row-name", "");
                var row = new RowModel(_shortStringHelper, rowName, rowNode);
                var blocks = this.GetBlocks(rowNode, rowName);
                row.Blocks = blocks;


                var properties = GetProperties(rowNode);
                row.Properties = properties;

                rows.Add(row);
            }

            return rows;
        }

        public List<BlockItemModel> GetBlocks(HtmlNode node, string rowName)
        {
            var blocks = new List<BlockItemModel>();

            var descendants = node.Descendants();
            if (descendants == null || !descendants.Any()) return blocks;

            foreach (var descendant in descendants)
            {
                var itemName = descendant.GetAttributeValue("data-item-name", "");
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    var item = new BlockItemModel(_shortStringHelper, itemName, descendant);

                    var properties = GetProperties(descendant);
                    item.Properties = properties;

                    blocks.Add(item);
                }
            }

            return blocks;
        }

        public List<PropertyModel> GetProperties(HtmlNode node)
        {
            var properties = new List<PropertyModel>();

            var descendants = node.Descendants();
            if (descendants == null || !descendants.Any()) return properties;

            foreach (var descendant in descendants)
            {
                var itemName = descendant.GetAttributeValue("data-prop-name", "");
                var itemType = descendant.GetAttributeValue("data-prop-type", "Textstring");
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    var item = new PropertyModel(itemName, itemType, descendant);
                    properties.Add(item);
                }
            }

            return properties;
        }

        public bool CreateRow(RowModel row)
        {
            // Set a variable to the Documents path.
            string contentRootPath = _webHostEnvironment.ContentRootPath;

            var blocklistComponentsFolderPath =
                Path.Combine(contentRootPath, "Views\\", "Partials\\", "blocklist\\", "Components\\");

            if (!File.Exists(blocklistComponentsFolderPath))
            {
                Directory.CreateDirectory(blocklistComponentsFolderPath);
            }

            var path = Path.Combine(contentRootPath, "Views\\", "Partials\\", "blocklist\\", "Components\\");

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, row.Alias + ".cshtml")))
            {
                outputFile.WriteLine("@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.BlockListItem>");
                outputFile.WriteLine("");
                outputFile.WriteLine("@{");
                outputFile.WriteLine($"    var row = ({row.ConventionName})Model.Content;");
                outputFile.WriteLine($"    var settings = ({row.ConventionName}Settings)Model.Settings;");
                outputFile.WriteLine("");
                outputFile.WriteLine("    if (settings.Hide) { return; }");
                outputFile.WriteLine("}");
                outputFile.WriteLine("");

                var lines = row.Html.Split("\n");
                var lastLine = lines.LastOrDefault();
                int spaces = lastLine.TakeWhile(Char.IsWhiteSpace).Count();

                var spacesToAdd = lastLine.Substring(0, spaces >= 0 ? spaces : 0);

                outputFile.WriteLine(spacesToAdd + row.Html);
            }

            return true; ;
        }

        public void CreateDataType(string name)
        {
            var textRow = _contentTypeService.Get("textRow");
            var textRowSettings = _contentTypeService.Get("textRowSettings");


            var editor = _propertyEditorCollection.First(x => x.Alias == "Umbraco.BlockList");
            var newDataType = new DataType(editor, _configurationEditorJsonSerializer)
            {
                Name = name,
                Configuration = new BlockListConfiguration
                {
                    Blocks = new[]
            {
                new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = textRow.Key,
                    SettingsElementTypeKey = textRowSettings.Key,
                    Label = "{{ !$title || $title == '' ? 'Test ' + $index : $title }}",
                    EditorSize = "medium",
                    ForceHideContentEditorInOverlay = false,
                    Stylesheet = "",
                    View = "",
                    IconColor = "#ffffff",
                    BackgroundColor = "#1b264f"
                },
            },
                    MaxPropertyWidth = "100%",
                    UseSingleBlockMode = false,
                    UseLiveEditing = false,
                    UseInlineEditingAsDefault = false,
                    ValidationLimit = new BlockListConfiguration.NumberRange()
                    {
                        Min = 0,
                        Max = 10
                    }
                }
            };

            _dataTypeService.Save(newDataType);
        }
    }
}
