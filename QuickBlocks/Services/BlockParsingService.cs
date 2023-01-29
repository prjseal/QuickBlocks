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

        public BlockParsingService(IShortStringHelper shortStringHelper, IWebHostEnvironment webHostEnvironment, IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper, IDataValueEditorFactory dataValueEditorFactory, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        {
            _shortStringHelper = shortStringHelper;
            _webHostEnvironment = webHostEnvironment;
            _dataTypeService = dataTypeService;
            _umbracoMapper = umbracoMapper;
            _dataValueEditorFactory = dataValueEditorFactory;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
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

        public bool CreateDataType(string name)
        {
            var dataType = _dataTypeService.GetDataType(name + DateTime.Now.ToString("ddMMyy hhmmss"));
            DataTypeDisplay? dt = dataType == null ? null : _umbracoMapper.Map<IDataType, DataTypeDisplay>(dataType);

            if (dt != null) return true;

            var editor = new DataEditor(_dataValueEditorFactory);
            editor.Alias = "Umbraco.BlockList";

            var newDataType = new DataType(editor, _configurationEditorJsonSerializer);

            newDataType.Name = name;
            //var config = new Dictionary<string, object>();

            var config = new BlockListConfiguration();
            config.MaxPropertyWidth = "100%";
            config.UseSingleBlockMode= true;
            config.UseLiveEditing= true;
            config.UseInlineEditingAsDefault= false;
            config.ValidationLimit = new BlockListConfiguration.NumberRange()
            {
                Min = 0,
                Max = 100
            };

            var blocks = new List<BlockListConfiguration.BlockConfiguration>();


            var block = new BlockListConfiguration.BlockConfiguration();
            block.Label = "{{ !$title || $title == '' ? 'Image Link ' + $index : $title }} {{$settings.hide ? '[HIDDEN]' : ''}}";
            block.EditorSize = "medium";
            block.ForceHideContentEditorInOverlay = true;
            block.Stylesheet = "~/App_Plugins/QuickBlocks/quickBlocks.css";
            block.View = "~/App_Plugins/QuickBlocks/quickBlocks.html";
            block.ContentElementTypeKey = new Guid("08e05150-1fe7-4810-96d2-cc0b9fd77a40");
            block.SettingsElementTypeKey = new Guid("55876948-ac8b-440f-bbca-19ac2bb18189");
            block.IconColor = "#ffffff";
            block.BackgroundColor = "#1b264f";

            blocks.Add(block);

            config.Blocks = blocks.ToArray();

            newDataType.Configuration = config;

            //var config = new DataTypeConfigurationFieldDisplay();
            //config.

            //var blockObject = new Dictionary<string, object>();
            //blockObject.Add("contentElementTypeKey", "9bd86554-8001-4a6b-b15a-1b3a5defe24b");
            //blockObject.Add("settingsElementTypeKey", null);
            //blockObject.Add("labelTemplate", "");
            //blockObject.Add("view", null);
            //blockObject.Add("stylesheet", null);
            //blockObject.Add("editorSize", "medium");
            //blockObject.Add("iconColor", null);
            //blockObject.Add("backgroundColor", null);
            //blockObject.Add("thumbnail", null);

            //config.Add("blocks", blockObject);

            //var validationLimit = new Dictionary<string, object>();
            //validationLimit.Add("min", null);
            //validationLimit.Add("max", null);

            //config.Add("validationLimit", validationLimit);
            //config.Add("useSingleBlockMode", false);
            //config.Add("useLiveEditing", false);
            //config.Add("useInlineEditingAsDefault", false);
            //config.Add("maxPropertyWidth", null);

            //newDataType.Configuration = config;



            _dataTypeService.Save(newDataType);

            //var dataType = 

            //_dataTypeService.Save();
            return false;
        }
    }
}
