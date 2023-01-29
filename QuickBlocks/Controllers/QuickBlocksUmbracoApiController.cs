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

        [HttpGet]
        public IEnumerable<RowModel> Build(string url)
        {
            var doc = new HtmlDocument();
            doc.Load(url);
            var rows = _blockParsingService.GetRows(doc.DocumentNode);

            if (rows != null && rows.Any())
            {
                foreach (var row in rows)
                {
                    _blockParsingService.CreateRow(row);
                }
            }
            return rows;
        }
    }
}
