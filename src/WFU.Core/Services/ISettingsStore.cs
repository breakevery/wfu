namespace WFU.Core.Services;

/// <summary>
/// 设置存储契约。以键值对形式保存字体、主题、最近项目等设置，并支持持久化到磁盘。
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// 读取指定键的设置值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="key">设置键。</param>
    /// <param name="defaultValue">当键不存在或无法转换时返回的默认值。</param>
    /// <returns>设置值；若键不存在或转换失败则返回 <paramref name="defaultValue"/>。</returns>
    T Get<T>(string key, T defaultValue);

    /// <summary>
    /// 写入指定键的设置值（仅写入内存，需调用 <see cref="Save"/> 才会落盘）。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="key">设置键。</param>
    /// <param name="value">要保存的值。</param>
    void Set<T>(string key, T value);

    /// <summary>将当前全部设置持久化到磁盘。</summary>
    void Save();

    /// <summary>从磁盘加载设置（文件不存在时静默跳过）。</summary>
    void Load();
}
