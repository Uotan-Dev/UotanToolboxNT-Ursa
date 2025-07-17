using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Irihi.Avalonia.Shared.Contracts;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class SplashViewModel : ObservableObject, IDialogContext
{
    [ObservableProperty] private double _progress;
    private readonly SplashModel _model;

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

    public SplashViewModel()
    {
        _model = new SplashModel();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        _model.Initialize();
        await Task.Delay(1000);
        RequestClose?.Invoke(this, true);
    }


    public void Close()
    {
        RequestClose?.Invoke(this, false);
    }

    public event EventHandler<object?>? RequestClose;
}