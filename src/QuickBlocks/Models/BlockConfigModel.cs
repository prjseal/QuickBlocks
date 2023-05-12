namespace Umbraco.Community.QuickBlocks.Models;

public class BlockListConfigurationModel
{
    public Block[] blocks { get; set; }
    public Validationlimit validationLimit { get; set; }
    public bool useSingleBlockMode { get; set; }
    public bool useLiveEditing { get; set; }
    public bool useInlineEditingAsDefault { get; set; }
    public string maxPropertyWidth { get; set; }
}

public class Validationlimit
{
}

public class Block
{
    public string backgroundColor { get; set; }
    public string iconColor { get; set; }
    public string contentElementTypeKey { get; set; }
    public string settingsElementTypeKey { get; set; }
    public string view { get; set; }
    public string stylesheet { get; set; }
    public string label { get; set; }
    public string editorSize { get; set; }
    public bool forceHideContentEditorInOverlay { get; set; }
}
