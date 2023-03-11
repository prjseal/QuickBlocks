using Microsoft.AspNetCore.Hosting;
using QuickBlocks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;

namespace QuickBlocks.Services
{
    public interface IBlockCreationService
    {
        public bool CreateRowPartial(RowModel row);

        void CreateList(BlockListModel list);

        List<BlockListConfiguration.BlockConfiguration> CreateBlockConfigurations(BlockListModel list);

        BlockListConfiguration.BlockConfiguration CreateBlockConfiguration(RowModel row);

        void CreateBlockListDatType(BlockListModel list, List<BlockListConfiguration.BlockConfiguration> blocks);

        IContentType CreateContentType(string name, string alias, int parentId = -1,
            bool isElement = true, bool isContainer = false, string iconClass = "icon-science");
    }
}
