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
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Extensions;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<BlockCreationService> _logger;

        public BlockCreationService(IShortStringHelper shortStringHelper, IWebHostEnvironment webHostEnvironment, 
            IDataTypeService dataTypeService, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer, 
            PropertyEditorCollection propertyEditorCollection, IContentTypeService contentTypeService, 
            ILogger<BlockCreationService> logger)
        {
            _shortStringHelper = shortStringHelper;
            _webHostEnvironment = webHostEnvironment;
            _dataTypeService = dataTypeService;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
            _propertyEditorCollection = propertyEditorCollection;
            _contentTypeService = contentTypeService;
            _logger = logger;
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

        public void CreateList(BlockListModel list, FolderStructure folderStructure, int parentDataTypeId)
        {
            var existingDataType = _dataTypeService.GetDataType(list.Name);

            if (existingDataType != null) return;

            if (list?.Rows == null || !list.Rows.Any()) return;

            foreach (var row in list.Rows)
            {
                CreateRowPartial(row);
            }

            if (list.Rows == null || !list.Rows.Any()) return;

            List<BlockListConfiguration.BlockConfiguration> blocks = CreateBlockConfigurations(list, folderStructure);

            if (blocks == null || !blocks.Any()) return;

            CreateBlockListDataType(list, blocks, parentDataTypeId);
        }

        public List<BlockListConfiguration.BlockConfiguration> CreateBlockConfigurations(BlockListModel list, FolderStructure folderStructure)
        {
            var blocks = new List<BlockListConfiguration.BlockConfiguration>();

            foreach (var row in list.Rows)
            {
                var block = CreateBlockConfiguration(row, folderStructure);

                if (block == null) continue;

                blocks.Add(block);
            }

            return blocks;
        }

        public FolderStructure CreateFolderStructure()
        {
            var componentsId = GetOrCreateFolder("Components", -1);
            var compositionsId = GetOrCreateFolder("Compositions", -1);
            var compositionsContentBlocksId = GetOrCreateFolder("Content Blocks", compositionsId, 2);
            var compositionsContentModelsId = GetOrCreateFolder("Content Models", compositionsContentBlocksId, 3);
            var compositionsSettingsModelsId = GetOrCreateFolder("Settings Models", compositionsContentBlocksId, 3);
            var elementsId = GetOrCreateFolder("Elements", -1);
            var elementsContentBlocksId = GetOrCreateFolder("Content Blocks", elementsId, 2);
            var elementsContentModelsId = GetOrCreateFolder("Content Models", elementsContentBlocksId, 3);
            var elementsSettingsModelsId = GetOrCreateFolder("Settings Models", elementsContentBlocksId, 3);
            var foldersId = GetOrCreateFolder("Folders", -1);
            var pagesId = GetOrCreateFolder("Pages", -1);

            var folderStructure = new FolderStructure();
            folderStructure.ComponentsId = componentsId;
            folderStructure.CompositionsId = compositionsId;
            folderStructure.CompositionsContentBlocksId = compositionsContentBlocksId;
            folderStructure.CompositionsContentModelsId = compositionsContentModelsId;
            folderStructure.CompositionsSettingsModelsId = compositionsSettingsModelsId;
            folderStructure.ElementsId = elementsId;
            folderStructure.ElementsContentBlocksId = elementsContentBlocksId;
            folderStructure.ElementsContentModelsId = elementsContentModelsId;
            folderStructure.ElementsSettingsModelsId = elementsSettingsModelsId;
            folderStructure.FoldersId = foldersId;
            folderStructure.PagesId = pagesId;

            return folderStructure;
        }

        private int GetOrCreateFolder(string folderName, int parentId, int level = 1)
        {
            var containers = _contentTypeService.GetAll().Where(x => x.IsContainer);

            IContentType matchingFolder = null;

            if (containers != null && containers.Any())
            {
                matchingFolder = containers.FirstOrDefault(x => x.Name == folderName && x.ParentId == parentId);
            }

            if (matchingFolder == null)
            {
                var tryCreateContainer = _contentTypeService.CreateContainer(parentId, Guid.NewGuid(), folderName);
                if(tryCreateContainer.Success)
                {
                    return tryCreateContainer.Result!.Entity!.Id;
                }
                return -1;
            }
            return matchingFolder.Id;
        }

        public BlockListConfiguration.BlockConfiguration CreateBlockConfiguration(RowModel row, FolderStructure folderStructure)
        {
            var contentDocType = _contentTypeService.Get(row.Alias);
            if (contentDocType == null)
            {
                contentDocType = CreateContentType(row.Name, row.Alias, folderStructure.ElementsContentModelsId, true, false, row.IconClass);

                if (contentDocType != null && row.Properties != null && row.Properties.Any())
                {
                    AddPropertiesToContentType(contentDocType, row.Properties, "Content");
                }
            }

            var settingsDocType = row.HasSettings ? _contentTypeService.Get(row.SettingsAlias) : null;
            if (settingsDocType == null && row.HasSettings)
            {
                settingsDocType = CreateContentType(row.SettingsName, row.SettingsAlias, folderStructure.ElementsSettingsModelsId, true, false, "icon-settings color-indigo");
                if(settingsDocType != null)
                {
                    AddCompositionsToContentType(settingsDocType, new List<string>() { "blockVisibilitySettings" });
                }
            }

            if (contentDocType == null) return null;

            return new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = contentDocType.Key,
                SettingsElementTypeKey = settingsDocType?.Key ?? null,
                Label = "{{ !title || title == '' ? '" + row.Name + " ' + $index : title }}",
                EditorSize = DefaultEditorSize,
                ForceHideContentEditorInOverlay = false,
                Stylesheet = null,
                View = null,
                IconColor = DefaultIconColour,
                BackgroundColor = DefaultBackgroundColour
            };
        }

        public void CreateSupportingContentTypes(int parentId)
        {
            CreateHideSettings(parentId);
        }

        public int CreateSupportingDataTypes()
        {
            try
            {
                var parentId = GetOrCreateQuickBlocksDataTypeContainer();
                CreateUrlPickerDataType(parentId, "Single Url Picker", 0, 1);
                return parentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when trying to create supporting Data Types");
                return -1;
            }
        }

        public int GetOrCreateQuickBlocksDataTypeContainer()
        {
            var existingDataTypes = _dataTypeService.GetContainers("QuickBlocks", 1);

            if (existingDataTypes != null && existingDataTypes.Any()) return existingDataTypes.FirstOrDefault().Id;

            var tryCreateContainer = _dataTypeService.CreateContainer(-1, Guid.NewGuid(), "QuickBlocks");
            if (tryCreateContainer.Success)
            {
                return tryCreateContainer.Result!.Entity!.Id;
            }

            return -1;
        }

        public void CreateUrlPickerDataType(int parentId, string name, int minNumber, int maxNumber)
        {
            var existingDataTypes = _dataTypeService.GetDataType(name);

            if (existingDataTypes != null) return;

            var editor = _propertyEditorCollection.First(x => x.Alias == "Umbraco.MultiUrlPicker");

            var newDataType = new DataType(editor, _configurationEditorJsonSerializer)
            {
                Name = name,
                Configuration = new MultiUrlPickerConfiguration
                {
                    MinNumber = minNumber,
                    MaxNumber= maxNumber,
                },
                ParentId = parentId
            };

            _dataTypeService.Save(newDataType);
        }

        public void CreateBlockListDataType(BlockListModel list, List<BlockListConfiguration.BlockConfiguration> blocks, int parentDataTypeId)
        {
            var editor = _propertyEditorCollection.First(x => x.Alias == "Umbraco.BlockList");

            var blockConfiguration = new BlockListConfiguration
            {
                Blocks = blocks.ToArray(),
                MaxPropertyWidth = list.MaxPropertyWidth,
                UseSingleBlockMode = list.UseSingleBlockMode,
                UseLiveEditing = list.UseLiveEditing,
                UseInlineEditingAsDefault = list.UseInlineEditingAsDefault,
            };

            if(list.ValidationLimitMin != 0 && list.ValidationLimitMax != 0)
            {
                blockConfiguration.ValidationLimit = new BlockListConfiguration.NumberRange()
                {
                    Min = list.ValidationLimitMin,
                    Max = list.ValidationLimitMax
                };
            }

            var newDataType = new DataType(editor, _configurationEditorJsonSerializer)
            {
                Name = list.Name,
                Configuration = blockConfiguration,
                ParentId = parentDataTypeId
            };
           
            _dataTypeService.Save(newDataType);
        }

        public IContentType CreateHideSettings(int parentId)
        {
            var name = "Block Visibility Settings";

            var contentType = CreateContentType(name, name.ToSafeAlias(_shortStringHelper, true), parentId, true, false, "icon-defrag color-pink");

            var properties = new List<PropertyModel>()
            {
                new PropertyModel("Hide", "True/false", null)
            };

            if(contentType != null)
            {
                AddPropertiesToContentType(contentType, properties, "Settings");
            }

            return contentType;
        }

        public IContentType CreateContentType(string name, string alias, int parentId = -1,
            bool isElement = true, bool isContainer = false, string iconClass = "icon-science", 
            bool allowedAsRoot = false, bool updateDoctype = false)
        {
            var existingDocType = _contentTypeService.Get(alias);
            if (existingDocType != null) return existingDocType;

            IContentType contentDocType;
            var contentType = new ContentType(_shortStringHelper, parentId);
            contentType.Name = name;
            contentType.Alias = alias;
            contentType.IsElement = isElement;
            contentType.IsContainer = isContainer;
            contentType.Icon = iconClass;
            contentType.AllowedAsRoot = allowedAsRoot;
            _contentTypeService.Save(contentType);
            contentDocType = _contentTypeService.Get(alias);
            return contentDocType;
        }

        public void AddCompositionsToContentType(string contentTypeAlias, List<string> compositionAliases)
        {
            IContentType? contentType = _contentTypeService.Get(contentTypeAlias);

            if (contentType != null)
            {
                _logger.LogError("Content Type is null");
                return;
            }

            AddCompositionsToContentType(contentType, compositionAliases);
        }

        public void AddCompositionsToContentType(IContentType contentType, List<string> compositionAliases)
        {
            List<IContentTypeComposition> compositions = contentType.ContentTypeComposition.ToList();
            
            foreach(var compositionAlias in compositionAliases)
            {
                IContentType? composition = _contentTypeService.Get(compositionAlias);
                

                if(composition == null)
                {
                    _logger.LogError("Composition is null");
                    continue;
                }

                compositions.Add(composition);
            }

            contentType.ContentTypeComposition = compositions;

            _contentTypeService.Save(contentType);
        }

        public void AddPropertiesToContentType(IContentType contentType, IEnumerable<PropertyModel> properties, string groupName)
        {
            if (contentType == null || properties == null) return;
            
                var success = contentType.AddPropertyGroup(groupName.ToSafeAlias(_shortStringHelper, true), groupName);
                if(success)
                {
                    var contentGroup = contentType.PropertyGroups.FirstOrDefault(x => x.Name == groupName);
                    foreach (var propertyModel in properties)
                    {
                        var dataType = _dataTypeService.GetDataType(propertyModel.PropertyType);

                        var alias = propertyModel.Name.ToSafeAlias(_shortStringHelper, true);

                        var propertyType = new PropertyType(_shortStringHelper, dataType, alias)
                        {
                            Name = propertyModel.Name,
                            Alias = alias
                        };
                        contentGroup.PropertyTypes!.Add(propertyType);
                    }
                    _contentTypeService.Save(contentType);
                }
        }
    }
}
