using System.Windows;
using ICSharpCode.AvalonEdit;

namespace WFU.Host.Behaviors;

/// <summary>
/// 把视图模型的文本单向桥接到 AvalonEdit 编辑器。
/// </summary>
/// <remarks>
/// AvalonEdit 的 <see cref="TextEditor.Text"/> 是普通 CLR 属性（非依赖属性），无法在 XAML 中直接绑定。
/// 本附加属性只负责「视图模型 → 编辑器」的推送；反向的「编辑器 → 视图模型」由
/// <c>MainWindow</c> 的 <c>TextChanged</c> 事件转发处理。
/// 只有保持绑定不被本地写入覆盖，视图模型的更新（如 New / Open）才能可靠地刷新编辑器。
/// </remarks>
public static class EditorTextBinding
{
    /// <summary>单向绑定的编辑器文本附加属性。</summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(EditorTextBinding),
            new PropertyMetadata(string.Empty, OnBoundTextChanged));

    /// <summary>获取绑定的文本。</summary>
    /// <param name="element">目标元素。</param>
    /// <returns>当前绑定值。</returns>
    public static string GetText(DependencyObject element) => (string)element.GetValue(TextProperty);

    /// <summary>设置绑定的文本。</summary>
    /// <param name="element">目标元素。</param>
    /// <param name="value">要设置的值。</param>
    public static void SetText(DependencyObject element, string value) => element.SetValue(TextProperty, value);

    private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextEditor editor)
            return;

        var newText = e.NewValue as string ?? string.Empty;
        if (editor.Text != newText)
            editor.Text = newText;
    }
}
