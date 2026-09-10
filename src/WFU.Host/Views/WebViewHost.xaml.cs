using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using WFU.Bridge;

namespace WFU.Host.Views;

/// <summary>
/// 承载 WebView2 的预览宿主控件：负责异步初始化，并把桥接对象暴露给 JavaScript。
/// </summary>
/// <remarks>
/// WebView2 是异步初始化的，<see cref="Microsoft.Web.WebView2.Wpf.WebView2.EnsureCoreWebView2Async"/> 只能调用一次；
/// 本类用 <c>_initialized</c> + 锁保证幂等，并在未初始化时拒绝访问 <c>CoreWebView2</c>。
/// </remarks>
public partial class WebViewHost : UserControl
{
    private readonly object _initLock = new();
    private bool _initialized;
    private WfuBridgeObject? _bridgeObject;

    /// <summary>初始化 <see cref="WebViewHost"/>。</summary>
    public WebViewHost()
    {
        InitializeComponent();
    }

    /// <summary>是否已完成 WebView2 初始化。</summary>
    public bool IsWebViewInitialized
    {
        get { lock (_initLock) { return _initialized; } }
    }

    /// <summary>
    /// 异步初始化 WebView2。可安全重复调用（内部幂等）。
    /// </summary>
    /// <exception cref="InvalidOperationException">WebView2 Runtime 缺失或初始化失败时抛出。</exception>
    public async Task InitializeAsync()
    {
        lock (_initLock)
        {
            if (_initialized)
                return;
        }

        try
        {
            await PreviewWebView.EnsureCoreWebView2Async();

            var core = PreviewWebView.CoreWebView2
                       ?? throw new InvalidOperationException("CoreWebView2 初始化失败（返回 null）。");

            // 暴露桥接对象：JS 侧 window.chrome.webview.hostObjects.wfu.xxx(...)
            _bridgeObject = new WfuBridgeObject();
            core.AddHostObjectToScript("wfu", _bridgeObject);

            // 开发期设置
            core.Settings.AreDevToolsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;

            lock (_initLock)
            {
                _initialized = true;
            }

            Debug.WriteLine("[WebViewHost] WebView2 初始化完成。");
            Console.WriteLine("[WebViewHost] WebView2 初始化完成。");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WebViewHost] 初始化失败: {ex}");
            Console.WriteLine($"[WebViewHost] 初始化失败: {ex.Message}");

            throw new InvalidOperationException(
                "WebView2 初始化失败。请确认已安装 WebView2 Runtime，且 WFU.Host 的目标框架为 net8.0-windows。", ex);
        }
    }

    /// <summary>加载 HTML 字符串内容。</summary>
    /// <param name="html">HTML 文本。</param>
    /// <exception cref="InvalidOperationException">尚未初始化时抛出。</exception>
    public void LoadHtml(string html)
    {
        EnsureInitialized();
        PreviewWebView.CoreWebView2.NavigateToString(html);
    }

    /// <summary>加载指定 URL。</summary>
    /// <param name="url">目标地址。</param>
    /// <exception cref="InvalidOperationException">尚未初始化时抛出。</exception>
    public void LoadUrl(string url)
    {
        EnsureInitialized();
        PreviewWebView.CoreWebView2.Navigate(url);
    }

    /// <summary>在页面中执行 JavaScript（C# → JS 反向通信）。</summary>
    /// <param name="script">要执行的脚本。</param>
    /// <returns>脚本执行结果（JSON 字符串）。</returns>
    /// <exception cref="InvalidOperationException">尚未初始化时抛出。</exception>
    public async Task<string> ExecuteScriptAsync(string script)
    {
        EnsureInitialized();
        return await PreviewWebView.CoreWebView2.ExecuteScriptAsync(script);
    }

    /// <summary>确保已初始化，否则抛出友好异常。</summary>
    private void EnsureInitialized()
    {
        lock (_initLock)
        {
            if (_initialized)
                return;
        }

        throw new InvalidOperationException("WebView2 尚未初始化，请先调用 InitializeAsync()。");
    }
}
