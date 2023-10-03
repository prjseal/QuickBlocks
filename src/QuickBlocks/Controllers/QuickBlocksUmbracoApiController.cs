using System.Text;

using HtmlAgilityPack;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Community.QuickBlocks.Models;
using Umbraco.Community.QuickBlocks.Services;

namespace Umbraco.Community.QuickBlocks.Controllers;

public class QuickBlocksApiController : UmbracoAuthorizedApiController
{
    private readonly IBlockParsingService _blockParsingService;
    private readonly IBlockCreationService _blockCreationService;
    private readonly ILogger<QuickBlocksApiController> _logger;
    private readonly IFileService _fileService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IShortStringHelper _shortStringHelper;


    public QuickBlocksApiController(IBlockCreationService blockCreationService,
        IBlockParsingService blockParsingService,
        ILogger<QuickBlocksApiController> logger,
        IFileService fileService,
        IContentTypeService contentTypeService,
        IWebHostEnvironment webHostEnvironment,
        IShortStringHelper shortStringHelper)
    {
        _blockCreationService = blockCreationService;
        _blockParsingService = blockParsingService;
        _logger = logger;
        _fileService = fileService;
        _contentTypeService = contentTypeService;
        _webHostEnvironment = webHostEnvironment;
        _shortStringHelper = shortStringHelper;
    }

    //https://localhost:44306/umbraco/backoffice/api/quickblocksapi/build/
    [HttpPost]
    public ActionResult<ContentTypeModel> Build(QuickBlocksInstruction quickBlocksInstruction)
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
            string contentRootPath = _webHostEnvironment.ContentRootPath;
            if (!System.IO.File.Exists(Path.Combine(contentRootPath,quickBlocksInstruction.Url)))
            {
                return new ActionResult<ContentTypeModel>(new ContentTypeModel(_shortStringHelper,"",""){Message = "The specified file does not exist"});
            }
            doc.Load(quickBlocksInstruction.Url);
        }

        var folderStructure = _blockCreationService.CreateFolderStructure();
        var parentDataTypeId = _blockCreationService.CreateSupportingDataTypes();
        _blockCreationService.CreateSupportingContentTypes(folderStructure.CompositionsSettingsModelsId);

        var contentType = _blockParsingService.GetContentType(doc.DocumentNode);

        var lists = _blockParsingService.GetLists(contentType.Html, false);

        foreach (var list in lists)
        {
            var rows = _blockParsingService.GetRows(list.Html, false);
            list.Rows = rows;
            foreach (var row in rows)
            {
                var sublists = _blockParsingService.GetLists(row.Html, true);
                row.SubLists = sublists;
                foreach (var sublist in row.SubLists)
                {
                    var subRows = _blockParsingService.GetRows(sublist.Html, true);
                    sublist.Rows = subRows;
                    foreach (var subRow in sublist.Rows)
                    {
                        var subRowProperties = _blockParsingService.GetProperties(subRow.Html);
                        subRow.Properties = subRowProperties;
                    }

                    if (!quickBlocksInstruction.ReadOnly)
                    {
                        _blockCreationService.CreateList(sublist, folderStructure, parentDataTypeId);
                    }
                }
                var rowProperties = _blockParsingService.GetProperties(row.Html);
                row.Properties = rowProperties;
            }
            if (!quickBlocksInstruction.ReadOnly)
            {
                _blockCreationService.CreateList(list, folderStructure, parentDataTypeId);
            }
        }

        var pageProperties = _blockParsingService.GetProperties(contentType.Html);
        contentType.Properties = pageProperties;

        contentType.Lists = lists;

        if (quickBlocksInstruction.ReadOnly) return contentType;

        var partialViews = _blockParsingService.GetPartialViews(doc.DocumentNode);
        _blockCreationService.CreatePartialViews(partialViews);

        if (!lists.Any()) return contentType;

        foreach (var list in lists)
        {
            _blockCreationService.CreateList(list, folderStructure, parentDataTypeId);
        }

        if (contentType != null)
        {
            var newContentType = _blockCreationService.CreateContentType(contentType.Name, contentType.Alias, folderStructure.PagesId, false, false, iconClass: "icon-home", true);

            if (newContentType != null && contentType.Properties != null && contentType.Properties.Any())
            {
                _blockCreationService.AddPropertiesToContentType(newContentType, contentType.Properties, "Content");
            }

            var masterTemplate = _fileService.CreateTemplateWithIdentity("Master", "master", doc.Text);

            var masterDoc = new HtmlDocument();

            masterDoc.LoadHtml(doc.DocumentNode.OuterHtml);

            var mainBody = masterDoc.DocumentNode.SelectNodes("//*[@data-content-type-name]").FirstOrDefault();

            if (mainBody != null)
            {
                var textNode = HtmlTextNode.CreateNode("@RenderBody()");
                mainBody.ParentNode.ReplaceChild(textNode, mainBody);
            }

            _blockCreationService.ReplaceAllPartialAttributesWithCalls(masterDoc);

            _blockCreationService.RemoveAllQuickBlocksAttributes(masterDoc);


            masterTemplate.Content = masterTemplate.Content + Environment.NewLine + masterDoc.DocumentNode.OuterHtml;
            _fileService.SaveTemplate((masterTemplate));

            var tryCreateTemplate = _fileService.CreateTemplateForContentType(contentType.Alias, contentType.Name);
            if (tryCreateTemplate.Success)
            {
                var template = tryCreateTemplate.Result.Entity;
                if (template != null)
                {
                    newContentType.SetDefaultTemplate(template);
                    _contentTypeService.Save(newContentType);
                }
                
                template.SetMasterTemplate(masterTemplate);
                _fileService.SaveTemplate(template);

                var contentTypeDoc = new HtmlDocument();
                contentTypeDoc.LoadHtml(contentType.Html);

                
                var templateContent = new StringBuilder();
                templateContent.AppendLine("@using Umbraco.Cms.Web.Common.PublishedModels;");
                templateContent.AppendLine($"@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.{template.Alias}>");
                templateContent.AppendLine("    @using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;");
                templateContent.AppendLine("@{");
                templateContent.AppendLine("    Layout = \"master.cshtml\";");
                templateContent.AppendLine("}");
                templateContent.AppendLine();

                var listProperties = contentTypeDoc.DocumentNode.SelectNodes("//*[@data-list-name]");

                _blockCreationService.RenderListPropertyCalls(listProperties, "Model");

                var subListProperties = contentTypeDoc.DocumentNode.SelectNodes("//*[@data-sub-list-name]");

                _blockCreationService.RenderListPropertyCalls(subListProperties, "Model");

                var properties = contentTypeDoc.DocumentNode.SelectNodes("//*[@data-prop-name]");

                _blockCreationService.RenderProperties(properties, "Model");

                _blockCreationService.RemoveAllQuickBlocksAttributes(contentTypeDoc);

                templateContent.AppendLine(contentTypeDoc.DocumentNode.OuterHtml);

                template.Content = templateContent.ToString();
                _fileService.SaveTemplate(template);
            }


        }

        return contentType;
    }
}