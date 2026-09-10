namespace WFU.PluginSDK;

/// <summary>
/// 语言包契约。核心遵循“零硬编码文本”（Zero Hardcoded Text）原则：
/// 界面上出现的所有显示文本都必须由语言包提供，核心本身不含任何显示字符串。
/// </summary>
/// <remarks>
/// 例如菜单项文本使用 <c>menu_file</c>、<c>menu_edit</c>、<c>menu_view</c>、
/// <c>menu_help</c> 等键，由语言包插件负责返回对应语言的实际文字。
/// </remarks>
public interface ILanguagePack
{
    /// <summary>
    /// 根据键获取本地化文本。
    /// </summary>
    /// <param name="key">文本键，例如 <c>"menu_file"</c>、<c>"menu_edit"</c>。</param>
    /// <returns>该键对应的显示文本；当键不存在时应原样返回 <paramref name="key"/>。</returns>
    string GetString(string key);
}
