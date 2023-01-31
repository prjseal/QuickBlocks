using System.Collections.Generic;

namespace QuickBlocks.Models
{
    public class BlockListModel
    {
        public string Name { get; set; }
        public IEnumerable<RowModel> Rows { get; set; }

        public BlockListModel(string name)
        {
            Name = name;
        }
    }
}