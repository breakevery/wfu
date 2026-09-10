using System.Text.Json;

namespace WFU.Core.Services;

/// <summary>
/// <see cref="ISettingsStore"/> 的默认实现，使用 <see cref="System.Text.Json"/> 将设置
/// 序列化到本地 <c>settings.json</c>。
/// </summary>
/// <remarks>
/// 内部以 <see cref="Dictionary{TKey,TValue}"/> 保存键值；键名不区分大小写。
/// <see cref="Load"/> 后得到的值以 <see cref="JsonElement"/> 形式缓存，<see cref="Get{T}"/>
/// 会自动将其反序列化为请求的类型。
/// </remarks>
public sealed class SettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsFilePath;
    private readonly Dictionary<string, object?> _values = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 使用设置文件路径初始化 <see cref="SettingsStore"/>。
    /// </summary>
    /// <param name="settingsFilePath">settings.json 的完整路径。</param>
    /// <exception cref="ArgumentNullException"><paramref name="settingsFilePath"/> 为 <c>null</c> 时抛出。</exception>
    public SettingsStore(string settingsFilePath)
    {
        ArgumentNullException.ThrowIfNull(settingsFilePath);
        _settingsFilePath = settingsFilePath;
    }

    /// <summary>设置文件的完整路径。</summary>
    public string SettingsFilePath => _settingsFilePath;

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <c>null</c> 时抛出。</exception>
    public T Get<T>(string key, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_values.TryGetValue(key, out var value) || value is null)
            return defaultValue;

        try
        {
            if (value is T typed)
                return typed;

            if (value is JsonElement element)
                return element.Deserialize<T>(SerializerOptions) ?? defaultValue;

            var json = JsonSerializer.Serialize(value, SerializerOptions);
            return JsonSerializer.Deserialize<T>(json, SerializerOptions) ?? defaultValue;
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <c>null</c> 时抛出。</exception>
    public void Set<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _values[key] = value;
    }

    /// <inheritdoc />
    public void Save()
    {
        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_values, SerializerOptions);
        File.WriteAllText(_settingsFilePath, json);
    }

    /// <inheritdoc />
    public void Load()
    {
        if (!File.Exists(_settingsFilePath))
            return;

        var json = File.ReadAllText(_settingsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return;

        var loaded = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, SerializerOptions);
        if (loaded is null)
            return;

        _values.Clear();
        foreach (var pair in loaded)
            _values[pair.Key] = pair.Value;
    }
}
