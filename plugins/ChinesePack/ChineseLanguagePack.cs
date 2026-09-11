using WFU.PluginSDK;

namespace ChinesePack;

/// <summary>
/// 中文语言包 for WFU.
/// 实现 <see cref="ILanguagePack"/>（继承自 <see cref="IPlugin"/>，生命周期方法使用默认实现）。
/// </summary>
[Plugin("ChinesePack", "1.0.0", "Chinese language pack")]
public class ChineseLanguagePack : ILanguagePack
{
    private static readonly Dictionary<string, string> Strings = new()
    {
        // 菜单标题
        { "menu_file", "文件" },
        { "menu_edit", "编辑" },
        { "menu_view", "视图" },
        { "menu_help", "帮助" },
        // File 菜单项
        { "menu_file_new", "新建" },
        { "menu_file_open", "打开..." },
        { "menu_file_save", "保存" },
        { "menu_file_exit", "退出" },
        { "menu_file_new_project", "新建项目..." },
        { "menu_file_export", "导出为 ZIP..." },
        { "menu_file_import", "导入项目..." },
        // Edit 菜单项
        { "menu_edit_undo", "撤销" },
        { "menu_edit_redo", "重做" },
        { "menu_edit_cut", "剪切" },
        { "menu_edit_copy", "复制" },
        { "menu_edit_paste", "粘贴" },
        // View 菜单项
        { "menu_view_toggle_left", "显示/隐藏左侧面板" },
        { "menu_view_toggle_right", "显示/隐藏右侧面板" },
        { "menu_view_test_bridge", "测试 C# → JS" },
        // Help 菜单项
        { "menu_help_about", "关于 WFU" },
        // 工具栏
        { "toolbar_new", "新建" },
        { "toolbar_open", "打开" },
        { "toolbar_save", "保存" },
        { "toolbar_preview", "预览" },
        { "toolbar_export", "导出" },
        { "toolbar_new_project", "新建项目" },
        { "toolbar_import", "导入" },
        { "toolbar_bridge_test", "桥接测试" },
        // 状态栏
        { "status_ready", "就绪" },
        { "status_untitled", "未命名" },
        { "status_opened", "已打开" },
        { "status_saved", "已保存" },
        { "status_modified", "● 已修改" },
        { "status_preview_refreshed", "预览已刷新" },
        { "status_line", "行" },
        { "status_col", "列" },
        { "status_new_file", "新建文件" },
        { "status_open_failed", "打开失败" },
        { "status_save_failed", "保存失败" },
        { "status_webview_ready", "WebView2 已就绪" },
        { "status_project_created", "项目已创建" },
        { "status_project_failed", "项目创建失败" },
        { "status_project_opened", "已打开" },
        { "status_export_success", "已导出" },
        { "status_export_failed", "导出失败" },
        { "status_no_project", "未打开项目" },
        { "status_import_success", "已导入" },
        { "status_import_failed", "导入失败" },
        // 对话框
        { "dialog_new_project_title", "创建新项目" },
        { "dialog_export_title", "导出项目为 ZIP" },
        { "dialog_import_title", "从 ZIP 导入项目" },
        { "confirm_import_overwrite_title", "目录已存在" },
        { "confirm_import_overwrite_msg", "目标目录已存在，是否合并并继续？" },
    };

    /// <inheritdoc />
    public string GetString(string key)
        => Strings.TryGetValue(key, out var value) ? value : key;
}
