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
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            var dataObject = new DataObject();
            if (button.Content != null)
            {
                var text = button.Content.ToString();
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