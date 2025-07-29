using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Irihi.Avalonia.Shared.Contracts;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class SplashWindowViewModel : ObservableObject, IDialogContext
{
    [ObservableProperty] private double _progress;
    private readonly SplashWindowModel _model;

    public string StatusText
    {
        get => _model.StatusText;
        set
        {
            if (_model.StatusText != value)
            {
                _model.StatusText = value;
                OnPropertyChanged();
            }
        }
    }

    public SplashWindowViewModel()
    {
        _model = new SplashWindowModel();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        SplashWindowModel.Initialize();
        await Task.Delay(1000);
        RequestClose?.Invoke(this, true);
    }


    public void Close() => RequestClose?.Invoke(this, false);

    public event EventHandler<object?>? RequestClose;
}