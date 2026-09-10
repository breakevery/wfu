namespace WFU.Core.Services;

/// <summary>
/// 文件读写服务契约。负责以异步方式读写工程文件（<c>.html</c> / <c>.js</c> / <c>.css</c>），
/// 是微内核「读写」能力的最小实现，不做任何解析。
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 异步读取文本文件的全部内容。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <returns>文件的全部文本内容。</returns>
    /// <exception cref="FileNotFoundException">当 <paramref name="path"/> 指向的文件不存在时抛出。</exception>
    Task<string> ReadFileAsync(string path);

    /// <summary>
    /// 异步写入文本文件。若目标目录不存在，会自动创建。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <param name="content">要写入的内容。</param>
    Task WriteFileAsync(string path, string content);

    /// <summary>
    /// 判断指定文件是否存在。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <returns>存在返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    bool FileExists(string path);
}
