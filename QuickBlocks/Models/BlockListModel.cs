using System.Collections.Generic;

namespace QuickBlocks.Models
{
    public class BlockListModel
    {
        public string Name { get; set; }
        public IEnumerable<RowModel> Rows { get; set; }
        public string MaxPropertyWidth { get; set; }
        public bool UseSingleBlockMode { get; set; }
        public bool UseLiveEditing { get; set; }
        public bool UseInlineEditingAsDefault { get; set; }
        public int ValidationLimitMin { get; set; }
        public int ValidationLimitMax { get; set; }

        public BlockListModel(string name, string prefix = "[BlockList] ")
        {
            Name = prefix + name;
        }
    }
}