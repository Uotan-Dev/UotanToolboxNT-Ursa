using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace UotanToolboxNT_Ursa.Pages;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();
    }

    public async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            Avalonia.Input.Platform.IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            DataObject dataObject = new DataObject();
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    dataObject.Set(DataFormats.Text, text);
                }
            }
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(dataObject);
            }
        }
    }
}