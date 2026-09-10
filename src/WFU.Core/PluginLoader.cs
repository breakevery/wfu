using System.Reflection;
using WFU.PluginSDK;

namespace WFU.Core;

/// <summary>
/// 插件加载器。扫描指定目录下的插件程序集，加载其中实现了 <see cref="IPlugin"/> 的类型，
/// 实例化并调用其 <see cref="IPlugin.Initialize"/>。
/// </summary>
/// <remarks>
/// 加载对每个 DLL、每个类型单独捕获异常：单个插件失败不会中断整体加载流程；
/// 所有错误信息通过 <see cref="Console.WriteLine(string)"/> 输出，便于在调试面板中查看。
/// </remarks>
public sealed class PluginLoader
{
    private readonly List<IPlugin> _plugins = new();

    /// <summary>当前已成功加载的插件实例。</summary>
    public IReadOnlyList<IPlugin> Plugins => _plugins;

    /// <summary>
    /// 扫描目录下所有 <c>*.dll</c>，加载并初始化其中的插件。
    /// </summary>
    /// <param name="directory">插件目录。目录不存在时记录日志并直接返回。</param>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> 为 <c>null</c> 时抛出。</exception>
    public void LoadPlugins(string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);

        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"[PluginLoader] 插件目录不存在: {directory}");
            return;
        }

        foreach (var dllPath in Directory.GetFiles(directory, "*.dll"))
        {
            try
            {
                LoadPluginFromAssembly(dllPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginLoader] 加载失败: {dllPath} -> {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 卸载全部插件：逆序逐个调用 <see cref="IPlugin.Dispose"/>，随后清空已加载列表。
    /// </summary>
    public void UnloadAll()
    {
        for (var i = _plugins.Count - 1; i >= 0; i--)
        {
            try
            {
                _plugins[i].Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginLoader] 卸载失败: {ex.Message}");
            }
        }

        _plugins.Clear();
    }

    private void LoadPluginFromAssembly(string dllPath)
    {
        var assembly = Assembly.LoadFrom(dllPath);

        foreach (var type in GetLoadableTypes(assembly))
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                continue;

            try
            {
                if (Activator.CreateInstance(type) is not IPlugin plugin)
                    continue;

                plugin.Initialize();
                _plugins.Add(plugin);
                Console.WriteLine($"[PluginLoader] 已加载插件: {type.FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginLoader] 实例化/初始化失败: {type.FullName} -> {ex.Message}");
            }
        }
    }

    /// <summary>GetTypes 的容错版本：跳过无法加载的类型，避免单个类型失败导致整个程序集报废。</summary>
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var types = new List<Type>();
            foreach (var t in ex.Types)
                if (t is not null)
                    types.Add(t);
            return types;
        }
    }
}
