namespace Tavenem.Blazor.MarkdownEditor;

/// <summary>
/// The style of preview for a <see cref="MarkdownEditor"/>.
/// </summary>
public enum MarkdownPreviewStyle
{
    /// <summary>
    /// A vertically split view, with the markdown editor and a preview pane
    /// side by side.
    /// </summary>
    Split = 0,

    /// <summary>
    /// Two tabs: the markdown editor, and the preview, which can be toggle dby
    /// the user.
    /// </summary>
    Tabs = 1,
}
