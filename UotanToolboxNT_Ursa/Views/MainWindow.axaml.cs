using System;
using Avalonia;
using Avalonia.Interactivity;
using Irihi.Avalonia.Shared.Helpers;
using Ursa.Controls;

namespace UotanToolboxNT_Ursa.Views;

public partial class MainWindow : UrsaWindow
{
    public MainWindow()
    {
        InitializeComponent();
        _ = this.GetObservable(ClientSizeProperty).Subscribe(OnClientSizeChanged);
    }

    private const double AspectRatio = 1197.0 / 825.0; // 设定目标宽高比（如16:9）
    private Size _lastSize = new(1197, 825);

    private void OnClientSizeChanged(Size newSize)
    {
        var deltaWidth = Math.Abs(newSize.Width - _lastSize.Width);
        var deltaHeight = Math.Abs(newSize.Height - _lastSize.Height);

        if (deltaWidth > deltaHeight)
        {
            // 用户主要在拖动宽度
            var expectedHeight = newSize.Width / AspectRatio;
            if (Math.Abs(newSize.Height - expectedHeight) > 1)
            {
                Height = expectedHeight;
            }
        }
        else
        {
            // 用户主要在拖动高度
            var expectedWidth = newSize.Height * AspectRatio;
            if (Math.Abs(newSize.Width - expectedWidth) > 1)
            {
                Width = expectedWidth;
            }
        }

        _lastSize = new Size(Width, Height);
    }
}