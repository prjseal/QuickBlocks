using System;

using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuickBlocks.Models;
using QuickBlocks.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using static System.Net.Mime.MediaTypeNames;
using static Umbraco.Cms.Core.Constants.HttpContext;
using NUglify.JavaScript.Syntax;

namespace QuickBlocks.Controllers
{

    public class QuickBlocksApiController : UmbracoAuthorizedApiController
    {
        private readonly IBlockParsingService _blockParsingService;
        private readonly IBlockCreationService _blockCreationService;
        private readonly ILogger<QuickBlocksApiController> _logger;
        private readonly IFileService _fileService;
        private readonly IContentTypeService _contentTypeService;

        public QuickBlocksApiController(IBlockCreationService blockCreationService,
            IBlockParsingService blockParsingService,
            ILogger<QuickBlocksApiController> logger,
            IFileService fileService,
            IContentTypeService contentTypeService)
        {
            _blockCreationService = blockCreationService;
            _blockParsingService = blockParsingService;
            _logger = logger;
            _fileService = fileService;
            _contentTypeService = contentTypeService;
        }

        //https://localhost:44306/umbraco/backoffice/api/quickblocksapi/build/
        [HttpPost]
        public ActionResult<IEnumerable<BlockListModel>> Build(QuickBlocksInstruction quickBlocksInstruction)
        {
            if (quickBlocksInstruction == null ||
                (string.IsNullOrWhiteSpace(quickBlocksInstruction.Url ?? "")
                    && string.IsNullOrWhiteSpace(quickBlocksInstruction.HtmlBody ?? "")))
                return BadRequest("Missing Url Parameter or HtmlBody Parameter in API Request");

            var doc = new HtmlDocument();

            if (!string.IsNullOrWhiteSpace(quickBlocksInstruction.HtmlBody))
            {
                doc.LoadHtml(quickBlocksInstruction.HtmlBody);
            }
            else
            {
                doc.Load(quickBlocksInstruction.Url);
            }

            var partialViews = _blockParsingService.GetPartialViews(doc.DocumentNode);

            _blockCreationService.CreatePartialViews(partialViews);

            var folderStructure = _blockCreationService.CreateFolderStructure();
            var parentDataTypeId = _blockCreationService.CreateSupportingDataTypes();
            _blockCreationService.CreateSupportingContentTypes(folderStructure.CompositionsSettingsModelsId);

            var contentType = _blockParsingService.GetContentType(doc.DocumentNode);

            var lists = _blockParsingService.GetLists(contentType.Html, false);

            foreach(var list in lists)
            {
                var rows = _blockParsingService.GetRows(list.Html, false);
                foreach(var row in rows)
                {
                    var sublists = _blockParsingService.GetLists(row.Html, true);

                    foreach(var sublist in sublists)
                    {
                        var subRows = _blockParsingService.GetRows(list.Html, true);
                        foreach (var subRow in sublist.Rows)
                        {
                            var subRowProperties = _blockParsingService.GetProperties(subRow.Html, "");
                            subRow.Properties = subRowProperties;
                        }

                    }
                    var rowProperties = _blockParsingService.GetProperties(row.Html, "row");
                    row.Properties = rowProperties;

                    row.SubLists = sublists;
                }
                list.Rows = rows;
            }

            return lists;

            if (!lists.Any()) return lists;

            foreach (var list in lists)
            {
                _blockCreationService.CreateList(list, folderStructure, parentDataTypeId);
            }

            if(contentType != null)
            {
                var newContentType = _blockCreationService.CreateContentType(contentType.Name, contentType.Alias, folderStructure.PagesId, false, false, iconClass: "icon-home", true);

                var properties = _blockParsingService.GetProperties(doc.DocumentNode.OuterHtml, "page");

                if(newContentType != null && properties != null && properties.Any()) { 
                    _blockCreationService.AddPropertiesToContentType(newContentType, properties, "Content");
                }

                var masterTemplate = _fileService.CreateTemplateWithIdentity("Master", "master", doc.Text);

                var masterDoc = new HtmlDocument();

                masterDoc.LoadHtml(doc.DocumentNode.OuterHtml);

                var listProperties = masterDoc.DocumentNode.SelectNodes("//*[@data-list-name]");

                foreach(var property in listProperties)
                {
                    var itemName = property.Attributes["data-list-name"].Value;
                    var textNode = HtmlTextNode.CreateNode("@RenderBody()");
                    property.ParentNode.ReplaceChild(textNode, property);
                }

                _blockCreationService.RemoveAllQuickBlocksAttributes(masterDoc);

                masterTemplate.Content = masterTemplate.Content + Environment.NewLine + masterDoc.DocumentNode.OuterHtml;
                _fileService.SaveTemplate((masterTemplate));
                
                var tryCreateTemplate = _fileService.CreateTemplateForContentType("homePage", "Home Page");
                if(tryCreateTemplate.Success)
                {
                    var template = tryCreateTemplate.Result.Entity;
                    if (template != null)
                    {
                        newContentType.SetDefaultTemplate(template);
                        _contentTypeService.Save(newContentType);
                    }
                    
                    template.SetMasterTemplate(masterTemplate);
                    _fileService.SaveTemplate(template);
                    
                    var templateContent = new StringBuilder();
                    templateContent.AppendLine("@using Umbraco.Cms.Web.Common.PublishedModels;");
                    templateContent.AppendLine("@using Umbraco.Cms.Web.Common.PublishedModels;");
                    templateContent.AppendLine("@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.HomePage>");
                    templateContent.AppendLine("    @using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;");
                    templateContent.AppendLine("@{");
                    templateContent.AppendLine("    Layout = \"master.cshtml\";");
                    templateContent.AppendLine("}");
                    templateContent.AppendLine();
                    templateContent.AppendLine("@Html.GetBlockListHtml(Model.MainContent)");

                    template.Content = templateContent.ToString();
                    _fileService.SaveTemplate(template);
                }

                
            }

            return lists;
        }
    }
}
