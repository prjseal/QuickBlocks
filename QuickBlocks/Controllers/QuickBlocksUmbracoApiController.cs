using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuickBlocks.Models;
using QuickBlocks.Services;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;

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

            var folderStructure = _blockCreationService.CreateFolderStructure();
            var parentDataTypeId = _blockCreationService.CreateSupportingDataTypes();
            _blockCreationService.CreateSupportingContentTypes(folderStructure.CompositionsSettingsModelsId);

            var lists = _blockParsingService.GetLists(doc.DocumentNode, true);

            lists.AddRange(_blockParsingService.GetLists(doc.DocumentNode, false));

            if (!lists.Any()) return lists;

            foreach (var list in lists)
            {
                _blockCreationService.CreateList(list, folderStructure, parentDataTypeId);
            }


            var contentType = _blockParsingService.GetContentType(doc.DocumentNode);

            if(contentType != null)
            {
                var newContentType = _blockCreationService.CreateContentType(contentType.Name, contentType.Alias, folderStructure.PagesId, false, false, iconClass: "icon-home", true);

                var properties = _blockParsingService.GetProperties(doc.DocumentNode, "page");

                if(newContentType != null && properties != null && properties.Any()) { 
                    _blockCreationService.AddPropertiesToContentType(newContentType, properties, "Content");
                }

                var tryCreateTemplate = _fileService.CreateTemplateForContentType("homePage", "Home Page");
                if(tryCreateTemplate.Success)
                {
                    var template = tryCreateTemplate.Result.Entity;
                    if (template != null)
                    {
                        newContentType.SetDefaultTemplate(template);
                        _contentTypeService.Save(newContentType);
                    }
                }

                
            }

            return lists;
        }
    }
}
