using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using QuickBlocks.Models;
using QuickBlocks.Services;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace QuickBlocks.Controllers
{

    public class QuickBlocksApiController : UmbracoAuthorizedApiController
    {
        private readonly IBlockParsingService _blockParsingService;

        public QuickBlocksApiController(IBlockParsingService blockParsingService)
        {
            _blockParsingService = blockParsingService;
        }

        //https://localhost:44306/umbraco/backoffice/api/quickblocksapi/build/?url=index.html
        [HttpGet]
        public IEnumerable<BlockListModel> Build(string url)
        {
            var doc = new HtmlDocument();
            doc.Load(url);
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
