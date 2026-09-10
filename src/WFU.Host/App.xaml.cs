using System;
using System.Windows;
using System.Windows.Threading;

namespace WFU.Host;

/// <summary>
/// 应用程序入口。装配全局异常处理，避免未处理异常导致应用直接闪退。
/// </summary>
public partial class App : Application
{
    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 非 UI 线程的未处理异常
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // UI 线程的未处理异常（可标记为已处理，阻止闪退）
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    /// <summary>处理非 UI 线程的未处理异常，弹窗提示。</summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var message = e.ExceptionObject as Exception;
        MessageBox.Show(
            message?.ToString() ?? "发生未知异常。",
            "WFU · 未处理异常",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>处理 UI 线程的未处理异常，弹窗提示并阻止闪退。</summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            e.Exception.ToString(),
            "WFU · 未处理异常",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}
