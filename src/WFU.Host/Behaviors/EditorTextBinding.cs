using System.Windows;
using ICSharpCode.AvalonEdit;

namespace WFU.Host.Behaviors;

/// <summary>
/// 为 AvalonEdit 的 <see cref="TextEditor"/> 提供可双向绑定的 <c>Text</c> 附加属性。
/// </summary>
/// <remarks>
/// AvalonEdit 的 <see cref="TextEditor.Text"/> 是普通 CLR 属性而非依赖属性，无法在 XAML 中
/// 直接写 <c>Text="{Binding ...}"</c>（会抛 <see cref="System.Windows.Markup.XamlParseException"/>）。
/// 本附加属性作为桥接层，把视图模型的文本与编辑器文本保持同步，从而在不破坏 MVVM 的前提下完成绑定。
/// </remarks>
public static class EditorTextBinding
{
    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IsHooked",
            typeof(bool),
            typeof(EditorTextBinding),
            new PropertyMetadata(false));

    /// <summary>可双向绑定的编辑器文本附加属性。</summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(EditorTextBinding),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundTextChanged));

    /// <summary>获取编辑器文本。</summary>
    /// <param name="element">目标元素。</param>
    /// <returns>当前绑定的文本。</returns>
    public static string GetText(DependencyObject element) => (string)element.GetValue(TextProperty);

    /// <summary>设置编辑器文本。</summary>
    /// <param name="element">目标元素。</param>
    /// <param name="value">要设置的值。</param>
    public static void SetText(DependencyObject element, string value) => element.SetValue(TextProperty, value);

    private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextEditor editor)
            return;

        HookEditor(editor);

        var newText = e.NewValue as string ?? string.Empty;
        if (editor.Text != newText)
            editor.Text = newText;
    }

    /// <summary>首次绑定时订阅编辑器的 TextChanged，将用户输入回写到附加属性（避免循环）。</summary>
    private static void HookEditor(TextEditor editor)
    {
        if (editor.GetValue(IsHookedProperty) is true)
            return;

        editor.SetValue(IsHookedProperty, true);
        editor.TextChanged += (_, _) =>
        {
            if (GetText(editor) != editor.Text)
                SetText(editor, editor.Text);
        };
    }
}
