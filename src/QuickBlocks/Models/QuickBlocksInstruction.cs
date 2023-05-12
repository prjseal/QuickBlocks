using Microsoft.AspNetCore.Html;

namespace Umbraco.Community.QuickBlocks.Models;

public class QuickBlocksInstruction
{
    public string Url { get; set; }
    public string HtmlBody { get; set; }
    public bool ReadOnly { get; set; }
}