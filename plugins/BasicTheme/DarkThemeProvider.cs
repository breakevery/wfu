using WFU.PluginSDK;

namespace BasicTheme;

/// <summary>
/// Dark theme provider for WFU.
/// 实现 <see cref="IPlugin"/>（供 PluginLoader 识别）+ <see cref="IThemeProvider"/>（能力接口）。
/// 生命周期方法留空——主题无需初始化/执行/清理逻辑。
/// </summary>
[Plugin("BasicTheme", "1.0.0", "Dark theme for WFU")]
public class DarkThemeProvider : IPlugin, IThemeProvider
{
    /// <inheritdoc />
    public void Initialize() { /* 主题无需初始化 */ }

    /// <inheritdoc />
    public void Execute() { /* 主题无需执行逻辑 */ }

    /// <inheritdoc />
    public void Dispose() { /* 主题无需清理资源 */ }

    /// <inheritdoc />
    public string BackgroundColor => "#1E1E1E";

    /// <inheritdoc />
    public string ForegroundColor => "#D4D4D4";

    /// <summary>编辑器背景色（接口外扩展属性，供后续迭代使用）。</summary>
    public string EditorBackground => "#1E1E1E";

    /// <summary>编辑器前景色（接口外扩展属性，供后续迭代使用）。</summary>
    public string EditorForeground => "#D4D4D4";

    /// <inheritdoc />
    public string FontFamily => "Consolas";

    /// <inheritdoc />
    public double FontSize => 14.0;
}
