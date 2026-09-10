using System.Runtime.InteropServices;

namespace WFU.Bridge;

/// <summary>
/// 暴露给 JavaScript 的 C# 桥接对象。
/// JS 侧调用方式：<c>window.chrome.webview.hostObjects.wfu.方法名(...)</c>
/// </summary>
/// <remarks>
/// 必须标记 <see cref="ComVisibleAttribute"/>，否则 JavaScript 侧看不到该对象。
/// 异步方法须返回 <see cref="Task"/> / <see cref="Task{TResult}"/>，不能是 <c>async void</c>，否则 JS 无法 await。
/// </remarks>
[ComVisible(true)]
public class WfuBridgeObject
{
    /// <summary>JS 调用此方法输出一条消息（最小闭环测试用）。</summary>
    /// <param name="message">要输出的消息文本。</param>
    public void ShowMessage(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[JS→C#] {message}");
        Console.WriteLine($"[JS→C#] {message}");
    }

    /// <summary>JS 调用此方法获取当前时间戳（验证返回值）。</summary>
    /// <returns>格式为 <c>yyyy-MM-dd HH:mm:ss</c> 的当前本地时间。</returns>
    public string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>JS 调用此方法把文本保存到指定文件（验证参数传递与异步返回）。</summary>
    /// <param name="path">目标文件路径。</param>
    /// <param name="content">要写入的内容。</param>
    /// <returns>保存成功返回 <c>true</c>，失败返回 <c>false</c>。</returns>
    public async Task<bool> SaveFileAsync(string path, string content)
    {
        try
        {
            await File.WriteAllTextAsync(path, content);
            Console.WriteLine($"[Bridge] SaveFileAsync 成功: {path}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Bridge] SaveFileAsync 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>桥接对象自检：返回固定字符串，用于确认 C# 对象已被 JS 正确加载。</summary>
    /// <returns>固定标识字符串。</returns>
    public string Ping()
    {
        return "wfu-bridge-ok";
    }
}
