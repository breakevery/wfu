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
        // Edit 菜单项（M5 补全）
        { "menu_edit_undo", "Undo" },
        { "menu_edit_redo", "Redo" },
        { "menu_edit_cut", "Cut" },
        { "menu_edit_copy", "Copy" },
        { "menu_edit_paste", "Paste" },
        // View 菜单项（M5 补全）
        { "menu_view_toggle_left", "Toggle Left Panel" },
        { "menu_view_toggle_right", "Toggle Right Panel" },
        // 工具栏（M5 补全）
        { "toolbar_bridge_test", "Bridge Test" },
        // 状态栏消息（M5-3 补全）
        { "status_new_file", "New File" },
        { "status_open_failed", "Open failed" },
        { "status_save_failed", "Save failed" },
        // 启动状态（M5-3 收尾）
        { "status_webview_ready", "WebView2 Ready" },
        // 项目系统（M6-1b-2）
        { "menu_file_new_project", "New Project..." },
        { "toolbar_new_project", "New Project" },
        { "status_project_created", "Project created" },
        { "status_project_failed", "Project creation failed" },
        { "status_project_opened", "Opened" },
        { "dialog_new_project_title", "Create New Project" },
        // Export（M6-2）
        { "menu_file_export", "Export as ZIP..." },
        { "dialog_export_title", "Export Project as ZIP" },
        { "status_export_success", "Exported" },
        { "status_export_failed", "Export failed" },
        { "status_no_project", "No project opened" },
    };

    /// <inheritdoc />
    public string GetString(string key)
        => Strings.TryGetValue(key, out var value) ? value : key;
}
