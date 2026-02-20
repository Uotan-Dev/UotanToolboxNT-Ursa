using System;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class GlobalLogViewModel : ViewModelBase, IRecipient<LogUpdateMessage>, IRecipient<HardwareInfoUpdateMessage>
{
    [ObservableProperty] private string _logContent = "";
    [ObservableProperty] private string _logContent1 = "";
    [ObservableProperty] private string _logContent2 = "";
    [ObservableProperty] private string _logContent3 = "";

    public GlobalLogViewModel()
    {
        // 注册消息接收
        WeakReferenceMessenger.Default.Register<LogUpdateMessage>(this);
        WeakReferenceMessenger.Default.Register<HardwareInfoUpdateMessage>(this);

        // 初始化加载现有日志内容
        LoadInitialData();
    }

    private void LoadInitialData()
    {
        try
        {
            // 加载日志文件内容
            if (File.Exists(Global.LatestLogFile.FullName))
            {
                using var fs = new FileStream(Global.LatestLogFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs);
                LogContent = reader.ReadToEnd();
            }

            // 初始化硬件卡片信息
            LogContent1 = GlobalLogModel.GetMemoryInfo();
            LogContent2 = GlobalLogModel.GetSystemInfo();
            LogContent3 = GlobalLogModel.GetGpuInfo();
        }
        catch (Exception ex)
        {
            LogContent = $"加载初始日志失败: {ex.Message}";
        }
    }

    public void Receive(LogUpdateMessage message)
    {
        // 确保在UI线程更新绑定的属性
        Dispatcher.UIThread.Post(() =>
        {
            LogContent += message.LogContent;
        });
    }

    public void Receive(HardwareInfoUpdateMessage message)
    {
        // 确保在UI线程更新绑定的属性
        Dispatcher.UIThread.Post(() =>
        {
            switch (message.CardIndex)
            {
                case 1: LogContent1 = message.Info; break;
                case 2: LogContent2 = message.Info; break;
                case 3: LogContent3 = message.Info; break;
            }
        });
    }
}
