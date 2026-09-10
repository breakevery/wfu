namespace WFU.PluginSDK;

/// <summary>
/// WFU 插件的基础契约。所有插件类都必须实现该接口，并用
/// <see cref="PluginAttribute"/> 标注元数据后才会被核心加载。
/// </summary>
/// <remarks>
/// 生命周期（由核心/插件加载器驱动）：
/// <list type="number">
/// <item><description>加载插件时调用 <see cref="Initialize"/>；</description></item>
/// <item><description>用户触发插件时调用 <see cref="Execute"/>；</description></item>
/// <item><description>卸载插件或宿主退出时调用 <see cref="Dispose"/>。</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// using WFU.PluginSDK;
///
/// [Plugin("MyPlugin", "1.0.0", "Does something useful")]
/// public class MyPlugin : IPlugin
/// {
///     public void Initialize() { /* Called on load */ }
///     public void Execute()    { /* Called on trigger */ }
///     public void Dispose()    { /* Called on unload */ }
/// }
/// </code>
/// </example>
public interface IPlugin
{
    /// <summary>
    /// 插件被加载并加入插件集时调用，用于初始化资源（例如注册命令、读取配置）。
    /// </summary>
    void Initialize();

    /// <summary>
    /// 插件被用户触发执行时调用，用于完成插件的主要工作。
    /// </summary>
    void Execute();

    /// <summary>
    /// 插件被卸载或宿主退出时调用，用于释放资源（例如注销事件、关闭文件句柄）。
    /// </summary>
    void Dispose();
}
