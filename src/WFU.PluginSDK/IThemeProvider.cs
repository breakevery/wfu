namespace WFU.PluginSDK;

/// <summary>
/// 主题提供者契约。用于向核心提供界面与编辑器的配色及字体设置。
/// </summary>
/// <remarks>
/// 继承自 <see cref="IPlugin"/>，生命周期方法使用其默认实现。
/// 颜色统一使用十六进制字符串（形如 <c>"#1E1E1E"</c>），由核心转换为 WPF 画刷。
/// </remarks>
/// <example>
/// <code>
/// using WFU.PluginSDK;
///
/// public class DarkTheme : IThemeProvider
/// {
///     public string BackgroundColor => "#1E1E1E";
///     public string ForegroundColor => "#D4D4D4";
///     public string FontFamily => "Consolas";
///     public double FontSize => 14.0;
/// }
/// </code>
/// </example>
public interface IThemeProvider : IPlugin
{
    /// <summary>背景色，十六进制字符串，例如 <c>"#1E1E1E"</c>。</summary>
    string BackgroundColor { get; }

    /// <summary>前景（文字）色，十六进制字符串，例如 <c>"#D4D4D4"</c>。</summary>
    string ForegroundColor { get; }

    /// <summary>字体名称，例如 <c>"Consolas"</c>。</summary>
    string FontFamily { get; }

    /// <summary>字号（磅）。</summary>
    double FontSize { get; }

    /// <summary>编辑器背景色（默认复用 <see cref="BackgroundColor"/>）。</summary>
    string EditorBackground => BackgroundColor;

    /// <summary>编辑器前景色（默认复用 <see cref="ForegroundColor"/>）。</summary>
    string EditorForeground => ForegroundColor;
}
