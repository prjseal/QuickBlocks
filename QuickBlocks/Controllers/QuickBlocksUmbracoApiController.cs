using System;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using QuickBlocks.Models;
using QuickBlocks.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace QuickBlocks.Controllers
{

    public class QuickBlocksApiController : UmbracoAuthorizedApiController
    {
        private readonly IBlockParsingService _blockParsingService;
        private readonly ILogger<QuickBlocksApiController> _logger;

        public QuickBlocksApiController(IBlockParsingService blockParsingService, ILogger<QuickBlocksApiController> logger)
        {
            _blockParsingService = blockParsingService;
            _logger = logger;
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
            var lists = _blockParsingService.GetLists(doc.DocumentNode);

            if (lists == null || !lists.Any()) return lists;
            
            foreach (var list in lists)
            {
                _blockParsingService.CreateList(list);
            }

            return lists;
        }
    }
}
