namespace WFU.PluginSDK;

/// <summary>
/// 标记插件的元数据。核心通过反射读取该特性，从而在实例化插件之前即可获得
/// 其名称、版本与描述，用于列表展示、排序与兼容性判断。
/// </summary>
/// <remarks>
/// 用法：<c>[Plugin("MyPlugin", "1.0.0", "Does something useful")]</c>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PluginAttribute : Attribute
{
    /// <summary>
    /// 使用插件名称、版本与描述初始化元数据。
    /// </summary>
    /// <param name="name">插件名称（显示名）。</param>
    /// <param name="version">插件版本号，建议使用语义化版本（如 <c>1.0.0</c>）。</param>
    /// <param name="description">插件用途的一句话描述，可为空。</param>
    public PluginAttribute(string name, string version, string description = "")
    {
        Name = name;
        Version = version;
        Description = description;
    }

    /// <summary>插件名称（显示名）。</summary>
    public string Name { get; }

    /// <summary>插件版本号。</summary>
    public string Version { get; }

    /// <summary>插件用途的一句话描述。</summary>
    public string Description { get; }
}
