namespace WFU.Core.Services;

/// <summary>
/// <see cref="IFileService"/> 的默认实现，基于 <see cref="System.IO.File"/> 的异步 API。
/// </summary>
public sealed class FileService : IFileService
{
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 为 <c>null</c> 时抛出。</exception>
    /// <exception cref="FileNotFoundException">文件不存在时抛出。</exception>
    public async Task<string> ReadFileAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!File.Exists(path))
            throw new FileNotFoundException($"文件不存在: {path}", path);

        return await File.ReadAllTextAsync(path).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 为 <c>null</c> 时抛出。</exception>
    public async Task WriteFileAsync(string path, string content)
    {
        ArgumentNullException.ThrowIfNull(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, content ?? string.Empty).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 为 <c>null</c> 时抛出。</exception>
    public bool FileExists(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return File.Exists(path);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 为 <c>null</c> 时抛出。</exception>
    public bool DirectoryExists(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Directory.Exists(path);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 为 <c>null</c> 时抛出。</exception>
    public void CreateDirectory(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 为 <c>null</c> 时抛出。</exception>
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!Directory.Exists(path))
            return Enumerable.Empty<string>();

        return Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
    }
}
