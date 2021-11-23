namespace Tavenem.Blazor.MarkdownEditor;

/// <summary>
/// The edit mode of a <see cref="MarkdownEditor"/>.
/// </summary>
public enum EditMode
{
    /// <summary>
    /// Markdown code editing
    /// </summary>
    Markdown = 0,

    /// <summary>
    /// WYSIWYG editing
    /// </summary>
    WYSIWYG = 1,
}
