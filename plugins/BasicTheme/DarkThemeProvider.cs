using WFU.PluginSDK;

namespace BasicTheme;

/// <summary>
/// Dark theme provider for WFU.
/// 实现 <see cref="IThemeProvider"/>（继承自 <see cref="IPlugin"/>，生命周期方法使用默认实现）。
/// </summary>
[Plugin("BasicTheme", "1.0.0", "Dark theme for WFU")]
public class DarkThemeProvider : IThemeProvider
{
    /// <inheritdoc />
    public string BackgroundColor => "#1E1E1E";

    /// <inheritdoc />
    public string ForegroundColor => "#D4D4D4";

    /// <inheritdoc />
    public string EditorBackground => "#1E1E1E";

    /// <inheritdoc />
    public string EditorForeground => "#D4D4D4";

    /// <inheritdoc />
    public string FontFamily => "Consolas";

    /// <inheritdoc />
    public double FontSize => 14.0;
}
