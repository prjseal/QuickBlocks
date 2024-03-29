﻿using System.Collections.Generic;

namespace Umbraco.Community.QuickBlocks.Models;

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
    public string Html { get; set; }
    public string PreviewView { get; set; }
    public string PreviewCss { get; set; }

    public BlockListModel(string name, bool useCommunityPreview = false, 
        string previewCss = "", string previewView = "")
    {
        Name = name;

        PreviewCss = previewCss;
        PreviewView = !string.IsNullOrWhiteSpace(PreviewView) ? previewView : "";

        if (useCommunityPreview && string.IsNullOrWhiteSpace(PreviewView))
        {
            PreviewView = "~/App_Plugins/Umbraco.Community.BlockPreview/views/block-preview.html";
        }
    }
}