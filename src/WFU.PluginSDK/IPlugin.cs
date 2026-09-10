namespace WFU.PluginSDK;

/// <summary>
/// WFU 插件的基础契约。所有插件都必须实现该接口，或实现它的派生接口
/// （如 <see cref="ILanguagePack"/>、<see cref="IThemeProvider"/>），
/// 并用 <see cref="PluginAttribute"/> 标注元数据后才会被核心加载。
/// </summary>
/// <remarks>
/// 生命周期方法均提供<b>默认空实现</b>，因此能力型插件（语言包、主题等）无需重复编写。
/// 生命周期（由核心 / 插件加载器驱动）：
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
///     public void Execute() { /* 仅需覆写关心的生命周期方法 */ }
/// }
/// </code>
/// </example>
public interface IPlugin
{
    /// <summary>插件被加载并加入插件集时调用（默认空实现）。</summary>
    void Initialize() { }

    /// <summary>插件被用户触发执行时调用（默认空实现）。</summary>
    void Execute() { }

    /// <summary>插件被卸载或宿主退出时调用（默认空实现）。</summary>
    void Dispose() { }
}
