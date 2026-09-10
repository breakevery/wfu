namespace WFU.PluginSDK;

/// <summary>
/// 语言包插件契约。核心遵循「零硬编码文本」（Zero Hardcoded Text）原则：
/// 界面上出现的所有显示文本都必须由语言包提供，核心本身不含任何显示字符串。
/// </summary>
/// <remarks>
/// 继承自 <see cref="IPlugin"/>，生命周期方法使用其默认实现，实现者只需提供
/// <see cref="GetString"/>。例如菜单项文本使用 <c>menu_file</c>、<c>menu_edit</c> 等键。
/// </remarks>
public interface ILanguagePack : IPlugin
{
    /// <summary>
    /// 根据键获取本地化文本。
    /// </summary>
    /// <param name="key">文本键，例如 <c>"menu_file"</c>、<c>"menu_edit"</c>。</param>
    /// <returns>该键对应的显示文本；当键不存在时应原样返回 <paramref name="key"/>。</returns>
    string GetString(string key);
}
