using WFU.PluginSDK;

namespace BasicTheme;

/// <summary>
/// Dark theme provider for WFU.
/// Implements <see cref="IThemeProvider"/> with a VSCode-inspired dark palette.
/// </summary>
[Plugin("BasicTheme", "1.0.0", "Dark theme for WFU")]
public class DarkThemeProvider : IThemeProvider
{
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
