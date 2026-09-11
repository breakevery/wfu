using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WFU.Core.Services;
using WFU.PluginSDK;

namespace WFU.Host.ViewModels;

/// <summary>
/// 主窗口视图模型。承载编辑器状态与文件操作命令，遵循 MVVM：
/// 界面事件只在代码后置中转发到这里，业务逻辑全部在本类中。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly ISettingsStore _settingsStore;
    private ILanguagePack? _languagePack;

    /// <summary>当前打开的文件路径；未保存的新文件为 "Untitled"。</summary>
    [ObservableProperty]
    private string _currentFilePath = "Untitled";

    /// <summary>编辑器当前文本内容。</summary>
    [ObservableProperty]
    private string _editorContent = string.Empty;

    /// <summary>状态栏文本。</summary>
    [ObservableProperty]
    private string _statusText = "Ready";

    /// <summary>光标当前行号（从 1 开始）。</summary>
    [ObservableProperty]
    private int _currentLine = 1;

    /// <summary>光标当前列号（从 1 开始）。</summary>
    [ObservableProperty]
    private int _currentColumn = 1;

    /// <summary>当前内容是否有未保存的修改。</summary>
    [ObservableProperty]
    private bool _isModified;

    /// <summary>
    /// 使用文件服务与设置存储初始化 <see cref="MainViewModel"/>。
    /// </summary>
    /// <param name="fileService">文件读写服务。</param>
    /// <param name="settingsStore">设置存储。</param>
    /// <exception cref="ArgumentNullException">任一依赖为 <c>null</c> 时抛出。</exception>
    public MainViewModel(IFileService fileService, ISettingsStore settingsStore)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    }

    /// <summary>由 <c>MainWindow</c> 在插件加载完成后注入语言包（可为 null，表示降级）。</summary>
    /// <param name="pack">当前生效的语言包，或 <c>null</c>。</param>
    public void SetLanguagePack(ILanguagePack? pack)
    {
        _languagePack = pack;
        RefreshLanguage();
    }

    /// <summary>本地化辅助：语言包未加载时回退到 <paramref name="fallback"/>。</summary>
    /// <param name="key">语言包键。</param>
    /// <param name="fallback">降级文本。</param>
    /// <returns>本地化文本或降级文本。</returns>
    private string T(string key, string fallback)
        => _languagePack?.GetString(key) ?? fallback;

    /// <summary>语言包加载/切换后调用，重算所有由语言包驱动的显示文本。</summary>
    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(SaveStateText));
    }

    /// <summary>状态栏「已保存 / ● 已修改」文本（供 XAML 直接绑定）。</summary>
    public string SaveStateText => IsModified
        ? T("status_modified", "● 已修改")
        : T("status_saved", "已保存");

    /// <summary>IsModified 变化时同步通知 <see cref="SaveStateText"/>。</summary>
    /// <param name="value">新的修改状态。</param>
    partial void OnIsModifiedChanged(bool value)
    {
        OnPropertyChanged(nameof(SaveStateText));
    }

    /// <summary>当前项目根目录（<c>null</c> 表示未打开项目）。</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    private string? _currentProjectPath;

    /// <summary>当前项目下的文件列表（仅文件名）。</summary>
    public ObservableCollection<string> ProjectFiles { get; } = new();

    /// <summary>项目是否已打开。</summary>
    public bool HasProject => !string.IsNullOrEmpty(CurrentProjectPath);

    /// <summary>新建文件：清空内容并重置为未命名状态。</summary>
    [RelayCommand]
    private void NewFile()
    {
        EditorContent = string.Empty;
        CurrentFilePath = "Untitled";
        IsModified = false;
        StatusText = T("status_new_file", "新建文件");
    }

    /// <summary>打开文件：弹出打开对话框，读取所选文件内容到编辑器。</summary>
    [RelayCommand]
    private async Task OpenFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "打开文件",
            Filter = "网页与脚本 (*.html;*.htm;*.js;*.css)|*.html;*.htm;*.js;*.css|所有文件 (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            EditorContent = await _fileService.ReadFileAsync(dialog.FileName);
            CurrentFilePath = dialog.FileName;
            IsModified = false;
            StatusText = $"{T("status_opened", "已打开")}: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            StatusText = $"{T("status_open_failed", "打开失败")}: {ex.Message}";
            MessageBox.Show(ex.Message, "打开失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>保存文件：已有路径则直接保存，否则弹出“另存为”对话框。</summary>
    [RelayCommand]
    private async Task SaveFile()
    {
        var path = CurrentFilePath;
        if (string.IsNullOrEmpty(path) || path == "Untitled")
        {
            var dialog = new SaveFileDialog
            {
                Title = "保存文件",
                Filter = "网页与脚本 (*.html;*.htm;*.js;*.css)|*.html;*.htm;*.js;*.css|所有文件 (*.*)|*.*",
                FileName = "index.html"
            };

            if (dialog.ShowDialog() != true)
                return;

            path = dialog.FileName;
        }

        try
        {
            await _fileService.WriteFileAsync(path, EditorContent);
            CurrentFilePath = path;
            IsModified = false;
            StatusText = $"{T("status_saved", "已保存")}: {path}";
        }
        catch (Exception ex)
        {
            StatusText = $"{T("status_save_failed", "保存失败")}: {ex.Message}";
            MessageBox.Show(ex.Message, "保存失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>新建项目：在用户选定目录生成模板骨架，并作为当前工作区打开。</summary>
    [RelayCommand]
    private async Task NewProject()
    {
        var dialog = new SaveFileDialog
        {
            Title = T("dialog_new_project_title", "创建新项目"),
            Filter = "HTML (*.html)|*.html",
            FileName = "index.html",
            OverwritePrompt = false
        };

        if (dialog.ShowDialog() != true) return;

        // 用户选择的路径形如 C:\Projects\MyExp\index.html，取其目录作为项目根
        var targetDir = Path.GetDirectoryName(dialog.FileName);
        if (string.IsNullOrEmpty(targetDir)) return;

        try
        {
            // 1. 创建项目目录
            _fileService.CreateDirectory(targetDir);

            // 2. 复制模板文件
            var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
            if (!_fileService.DirectoryExists(templatesDir))
            {
                throw new DirectoryNotFoundException($"Templates 目录不存在: {templatesDir}");
            }

            foreach (var templateFile in _fileService.EnumerateFiles(templatesDir, "*.*"))
            {
                var fileName = Path.GetFileName(templateFile);
                var content = await _fileService.ReadFileAsync(templateFile);
                var targetPath = Path.Combine(targetDir, fileName);
                await _fileService.WriteFileAsync(targetPath, content);
            }

            // 3. 设为当前项目
            CurrentProjectPath = targetDir;
            RefreshProjectFiles();

            // 4. 自动打开 index.html
            var indexPath = Path.Combine(targetDir, "index.html");
            if (_fileService.FileExists(indexPath))
            {
                EditorContent = await _fileService.ReadFileAsync(indexPath);
                CurrentFilePath = indexPath;
                IsModified = false;
            }

            StatusText = $"{T("status_project_created", "项目已创建")}: {targetDir}";
            OnPropertyChanged(nameof(HasProject));
        }
        catch (Exception ex)
        {
            StatusText = $"{T("status_project_failed", "项目创建失败")}: {ex.Message}";
            MessageBox.Show(ex.Message, T("status_project_failed", "项目创建失败"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>刷新 <see cref="ProjectFiles"/> 集合（枚举项目根下的 .html/.htm/.js/.css）。</summary>
    private void RefreshProjectFiles()
    {
        ProjectFiles.Clear();
        if (string.IsNullOrEmpty(CurrentProjectPath)) return;
        if (!_fileService.DirectoryExists(CurrentProjectPath)) return;

        var extensions = new[] { "*.html", "*.htm", "*.js", "*.css" };
        var allFiles = extensions
            .SelectMany(ext => _fileService.EnumerateFiles(CurrentProjectPath, ext))
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .OrderBy(name => name);

        foreach (var file in allFiles)
        {
            ProjectFiles.Add(file!);
        }
    }

    /// <summary>根据文件名拼出完整路径。</summary>
    /// <param name="fileName">文件名。</param>
    /// <returns>完整路径；未打开项目时返回 <c>null</c>。</returns>
    public string? GetFullPath(string fileName)
        => string.IsNullOrEmpty(CurrentProjectPath)
            ? null
            : Path.Combine(CurrentProjectPath, fileName);

    /// <summary>从文件树打开文件。</summary>
    /// <param name="fileName">文件名（不含路径）。</param>
    public async Task OpenProjectFileAsync(string fileName)
    {
        var fullPath = GetFullPath(fileName);
        if (string.IsNullOrEmpty(fullPath) || !_fileService.FileExists(fullPath)) return;

        try
        {
            EditorContent = await _fileService.ReadFileAsync(fullPath);
            CurrentFilePath = fullPath;
            IsModified = false;
            StatusText = $"{T("status_project_opened", "已打开")}: {fileName}";
        }
        catch (Exception ex)
        {
            StatusText = $"{T("status_open_failed", "打开失败")}: {ex.Message}";
        }
    }

    /// <summary>Export 命令的 CanExecute：只有打开项目后才可用。</summary>
    /// <returns>已打开项目返回 <c>true</c>。</returns>
    private bool CanExport() => HasProject;

    /// <summary>导出当前项目为 ZIP（外层套一层 <c>{项目名}/</c> 目录）。</summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task Export()
    {
        if (string.IsNullOrEmpty(CurrentProjectPath))
        {
            StatusText = T("status_no_project", "未打开项目");
            return;
        }

        var projectName = Path.GetFileName(CurrentProjectPath.TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrEmpty(projectName))
            projectName = "project";

        var dialog = new SaveFileDialog
        {
            Title = T("dialog_export_title", "导出项目为 ZIP"),
            Filter = "ZIP (*.zip)|*.zip",
            FileName = $"{projectName}.zip"
        };
        if (dialog.ShowDialog() != true) return;

        var zipPath = dialog.FileName;

        try
        {
            // 若目标 zip 已存在，先删除（避免追加）
            if (File.Exists(zipPath)) File.Delete(zipPath);

            // 用 ZipArchive 逐文件添加，外层套 {projectName}/
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                var projectRoot = CurrentProjectPath!;
                foreach (var filePath in Directory.EnumerateFiles(
                    projectRoot, "*.*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(projectRoot, filePath)
                        .Replace('\\', '/');
                    var entryName = $"{projectName}/{relative}";
                    zip.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
                }
            }

            StatusText = $"{T("status_export_success", "已导出")}: {zipPath}";
        }
        catch (Exception ex)
        {
            StatusText = $"{T("status_export_failed", "导出失败")}: {ex.Message}";
            MessageBox.Show(ex.Message, T("status_export_failed", "导出失败"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        await Task.CompletedTask; // 保持 async 签名，未来可扩展
    }

    /// <summary>导入已导出的 zip 包，解压到 zip 同级同名目录并设为当前项目。</summary>
    [RelayCommand]
    private async Task Import()
    {
        var dialog = new OpenFileDialog
        {
            Title = T("dialog_import_title", "从 ZIP 导入项目"),
            Filter = "ZIP (*.zip)|*.zip|所有文件 (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var zipPath = dialog.FileName;

        // 目标目录 = zip 同级同名目录（去扩展名）
        var targetDir = Path.Combine(
            Path.GetDirectoryName(zipPath) ?? "",
            Path.GetFileNameWithoutExtension(zipPath));

        try
        {
            // 目标目录已存在 → 确认合并
            if (Directory.Exists(targetDir))
            {
                var confirm = MessageBox.Show(
                    $"{T("confirm_import_overwrite_msg", "目标目录已存在，是否合并并继续？")}\n\n{targetDir}",
                    T("confirm_import_overwrite_title", "目录已存在"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;
            }
            else
            {
                Directory.CreateDirectory(targetDir);
            }

            // 解压（zip 内可能有一层 {项目名}/ 目录）
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    // 跳过目录条目
                    if (string.IsNullOrEmpty(entry.Name)) continue;

                    var destPath = Path.Combine(targetDir, entry.FullName);

                    // 安全检查：防止 zip slip（../ 逃逸）
                    var destFull = Path.GetFullPath(destPath);
                    var targetFull = Path.GetFullPath(targetDir) + Path.DirectorySeparatorChar;
                    if (!destFull.StartsWith(targetFull, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"检测到不安全的 zip 条目: {entry.FullName}");
                    }

                    // 确保目标目录存在
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);

                    entry.ExtractToFile(destPath, overwrite: true);
                }
            }

            // 判断项目根：如果解压后只有 1 个子目录且它含 index.html，则用它
            var projectRoot = ResolveProjectRoot(targetDir);

            // 设为当前项目
            CurrentProjectPath = projectRoot;
            RefreshProjectFiles();

            // 自动打开 index.html
            var indexPath = Path.Combine(projectRoot, "index.html");
            if (_fileService.FileExists(indexPath))
            {
                EditorContent = await _fileService.ReadFileAsync(indexPath);
                CurrentFilePath = indexPath;
                IsModified = false;
            }

            StatusText = $"{T("status_import_success", "已导入")}: {projectRoot}";
            OnPropertyChanged(nameof(HasProject));
        }
        catch (Exception ex)
        {
            StatusText = $"{T("status_import_failed", "导入失败")}: {ex.Message}";
            MessageBox.Show(ex.Message, T("status_import_failed", "导入失败"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>如果 <paramref name="targetDir"/> 下只有一个子目录且含 index.html，返回该子目录；否则返回原目录。</summary>
    /// <param name="targetDir">解压目标目录。</param>
    /// <returns>项目根目录。</returns>
    private static string ResolveProjectRoot(string targetDir)
    {
        if (File.Exists(Path.Combine(targetDir, "index.html")))
            return targetDir;

        var subDirs = Directory.GetDirectories(targetDir);
        if (subDirs.Length == 1 &&
            File.Exists(Path.Combine(subDirs[0], "index.html")))
        {
            return subDirs[0];
        }

        return targetDir;
    }

    /// <summary>退出应用。</summary>
    [RelayCommand]
    private void ExitApp()
    {
        Application.Current.Shutdown();
    }

    /// <summary>编辑器文本发生变化时由界面事件转发调用，标记内容已修改。</summary>
    public void NotifyEditorTextChanged()
    {
        IsModified = true;
    }

    /// <summary>光标位置变化时由界面事件转发调用，更新行列号。</summary>
    /// <param name="line">行号。</param>
    /// <param name="column">列号。</param>
    public void NotifyCaretPositionChanged(int line, int column)
    {
        CurrentLine = line;
        CurrentColumn = column;
    }

    // ---- 以下为占位命令：Edit / View 菜单，后续接入真实逻辑 ----

    /// <summary>撤销（占位）。</summary>
    [RelayCommand]
    private void Undo()
    {
    }

    /// <summary>重做（占位）。</summary>
    [RelayCommand]
    private void Redo()
    {
    }

    /// <summary>剪切（占位）。</summary>
    [RelayCommand]
    private void Cut()
    {
    }

    /// <summary>复制（占位）。</summary>
    [RelayCommand]
    private void Copy()
    {
    }

    /// <summary>粘贴（占位）。</summary>
    [RelayCommand]
    private void Paste()
    {
    }

    /// <summary>切换左侧面板显示（占位）。</summary>
    [RelayCommand]
    private void ToggleLeftPanel()
    {
    }

    /// <summary>切换右侧面板显示（占位）。</summary>
    [RelayCommand]
    private void ToggleRightPanel()
    {
    }

    /// <summary>显示“关于”信息。</summary>
    [RelayCommand]
    private void About()
    {
        MessageBox.Show("WFU — WebView For Unity\nM2 视觉层骨架", "关于",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
