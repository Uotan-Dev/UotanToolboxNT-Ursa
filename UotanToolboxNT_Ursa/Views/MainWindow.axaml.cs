using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace UotanToolboxNT_Ursa.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}