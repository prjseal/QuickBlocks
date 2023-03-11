using Microsoft.AspNetCore.Hosting;
using QuickBlocks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using File = System.IO.File;
using System.Linq;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace QuickBlocks.Services
{
    public class BlockCreationService : IBlockCreationService
    {
        private const string DefaultIconColour = "#ffffff";
        private const string DefaultBackgroundColour = "#1b264f";
        private const string DefaultEditorSize = "medium";
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataTypeService _dataTypeService;
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
        private readonly PropertyEditorCollection _propertyEditorCollection;
        private readonly IContentTypeService _contentTypeService;

        public BlockCreationService(IShortStringHelper shortStringHelper, IWebHostEnvironment webHostEnvironment, IDataTypeService dataTypeService, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer, PropertyEditorCollection propertyEditorCollection, IContentTypeService contentTypeService)
        {
            _shortStringHelper = shortStringHelper;
            _webHostEnvironment = webHostEnvironment;
            _dataTypeService = dataTypeService;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
            _propertyEditorCollection = propertyEditorCollection;
            _contentTypeService = contentTypeService;
        }

        public bool CreateRowPartial(RowModel row)
        {
            string contentRootPath = _webHostEnvironment.ContentRootPath;

            var blocklistComponentsFolderPath =
                Path.Combine(contentRootPath, "Views\\", "Partials\\", "blocklist\\", "Components\\");

            var filePath = Path.Combine(blocklistComponentsFolderPath, row.Alias + ".cshtml");
            if (File.Exists(filePath)) return false;

            if (!Directory.Exists(blocklistComponentsFolderPath))
            {
                Directory.CreateDirectory(blocklistComponentsFolderPath);
            }

            using (StreamWriter outputFile = new StreamWriter(filePath))
            {
                outputFile.WriteLine(
                    "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.BlockListItem>");
                outputFile.WriteLine("");
                outputFile.WriteLine("@{");
                outputFile.WriteLine($"    var row = ({row.Name.Replace(" ", "")})Model.Content;");
                outputFile.WriteLine($"    var settings = ({row.SettingsName.Replace(" ", "")})Model.Settings;");
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

            return true;
        }

        public void CreateList(BlockListModel list)
        {
            var existingDataType = _dataTypeService.GetDataType(list.Name);

            if (existingDataType != null) return;

            if (list?.Rows == null || !list.Rows.Any()) return;

            foreach (var row in list.Rows)
            {
                CreateRowPartial(row);
            }

            if (list.Rows == null || !list.Rows.Any()) return;

            List<BlockListConfiguration.BlockConfiguration> blocks = CreateBlockConfigurations(list);

            if (blocks == null || !blocks.Any()) return;

            CreateBlockListDatType(list, blocks);
        }

        public List<BlockListConfiguration.BlockConfiguration> CreateBlockConfigurations(BlockListModel list)
        {
            var blocks = new List<BlockListConfiguration.BlockConfiguration>();

            foreach (var row in list.Rows)
            {
                var block = CreateBlockConfiguration(row);

                if (block == null) continue;

                blocks.Add(block);
            }

            return blocks;
        }

        public BlockListConfiguration.BlockConfiguration CreateBlockConfiguration(RowModel row)
        {
            var contentDocType = _contentTypeService.Get(row.Alias);
            if (contentDocType == null)
            {
                contentDocType = CreateContentType(row.Name, row.Alias);
            }

            var settingsDocType = row.HasSettings ? _contentTypeService.Get(row.SettingsAlias) : null;
            if (settingsDocType == null && row.HasSettings)
            {
                settingsDocType = CreateContentType(row.SettingsName, row.SettingsAlias);
            }

            if (contentDocType == null) return null;

            return new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = contentDocType.Key,
                SettingsElementTypeKey = settingsDocType?.Key ?? null,
                Label = "{{ !$title || $title == '' ? '" + row.Name + " ' + $index : $title }}",
                EditorSize = DefaultEditorSize,
                ForceHideContentEditorInOverlay = false,
                Stylesheet = null,
                View = null,
                IconColor = DefaultIconColour,
                BackgroundColor = DefaultBackgroundColour
            };
        }

        public void CreateBlockListDatType(BlockListModel list, List<BlockListConfiguration.BlockConfiguration> blocks)
        {
            var editor = _propertyEditorCollection.First(x => x.Alias == "Umbraco.BlockList");

            var newDataType = new DataType(editor, _configurationEditorJsonSerializer)
            {
                Name = list.Name,
                Configuration = new BlockListConfiguration
                {
                    Blocks = blocks.ToArray(),
                    MaxPropertyWidth = list.MaxPropertyWidth,
                    UseSingleBlockMode = list.UseSingleBlockMode,
                    UseLiveEditing = list.UseLiveEditing,
                    UseInlineEditingAsDefault = list.UseInlineEditingAsDefault,
                    ValidationLimit = new BlockListConfiguration.NumberRange()
                    {
                        Min = list.ValidationLimitMin,
                        Max = list.ValidationLimitMax
                    }
                }
            };

            _dataTypeService.Save(newDataType);
        }

        public IContentType CreateContentType(string name, string alias, int parentId = -1,
            bool isElement = true, bool isContainer = false, string iconClass = "icon-science")
        {
            IContentType contentDocType;
            var contentType = new ContentType(_shortStringHelper, parentId);
            contentType.Name = name;
            contentType.Alias = alias;
            contentType.IsElement = isElement;
            contentType.IsContainer = isContainer;
            contentType.Icon = iconClass;
            _contentTypeService.Save(contentType);
            contentDocType = _contentTypeService.Get(alias);
            return contentDocType;
        }
    }
}
