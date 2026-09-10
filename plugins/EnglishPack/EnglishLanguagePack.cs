using WFU.PluginSDK;

namespace EnglishPack;

/// <summary>
/// English language pack for WFU.
/// 实现 <see cref="ILanguagePack"/>（继承自 <see cref="IPlugin"/>，生命周期方法使用默认实现）。
/// </summary>
[Plugin("EnglishPack", "1.0.0", "English language pack")]
public class EnglishLanguagePack : ILanguagePack
{
    private static readonly Dictionary<string, string> Strings = new()
    {
        // 菜单标题
        { "menu_file", "File" },
        { "menu_edit", "Edit" },
        { "menu_view", "View" },
        { "menu_help", "Help" },
        // File 菜单项
        { "menu_file_new", "New" },
        { "menu_file_open", "Open..." },
        { "menu_file_save", "Save" },
        { "menu_file_exit", "Exit" },
        // View 菜单项
        { "menu_view_test_bridge", "Test C# → JS" },
        // Help 菜单项
        { "menu_help_about", "About WFU" },
        // 工具栏
        { "toolbar_new", "New" },
        { "toolbar_open", "Open" },
        { "toolbar_save", "Save" },
        { "toolbar_preview", "Preview" },
        { "toolbar_export", "Export" },
        // 状态栏
        { "status_ready", "Ready" },
        { "status_untitled", "Untitled" },
        { "status_opened", "Opened" },
        { "status_saved", "Saved" },
        { "status_modified", "● Modified" },
        { "status_preview_refreshed", "Preview refreshed" },
        { "status_line", "Ln" },
        { "status_col", "Col" },
    };

    /// <inheritdoc />
    public string GetString(string key)
        => Strings.TryGetValue(key, out var value) ? value : key;
}
