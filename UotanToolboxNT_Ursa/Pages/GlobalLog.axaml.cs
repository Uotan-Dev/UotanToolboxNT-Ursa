using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.Pages;

public partial class GlobalLog : UserControl
{
    public GlobalLog()
    {
        InitializeComponent();
    }

    private async Task<string> ExecuteTestCommand(string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = "parted";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            return !string.IsNullOrEmpty(error) ? error : output;
        }
        catch (Exception ex)
        {
            GlobalLogModel.AddLog($"执行命令失败：{ex.Message}", GlobalLogModel.LogLevel.Warning);
            return string.Empty;
        }
    }

    private async void TestBut_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (OperatingSystem.IsLinux())
        {
            LogTextBox.Text += "Linux";
        }
        else
        {
            LogTextBox.Text += "Not Linux";
        }
        LogTextBox.Text += $" - {RuntimeInformation.OSDescription}";
        LogTextBox.Text += $" - {RuntimeInformation.OSArchitecture}";
        LogTextBox.Text += $" - {RuntimeInformation.FrameworkDescription}";
        LogTextBox.Text += $" - {RuntimeInformation.ProcessArchitecture}";
        LogTextBox.Text += $" - {RuntimeInformation.RuntimeIdentifier}";
        //string result = await ExecuteTestCommand("-h");
        //LogTextBox.Text += result;
    }
}