namespace Tavenem.Blazor.MarkdownEditor;

/// <summary>
/// The display color scheme of a <see cref="MarkdownEditor"/> or <see
/// cref="MarkdownViewer"/>.
/// </summary>
public enum MarkdownEditorTheme
{
    /// <summary>
    /// Attempts to match the user's configured preference.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// A light theme (bright background and dark text and controls).
    /// </summary>
    Light = 1,

    /// <summary>
    /// A dark theme (dark background and light text and controls).
    /// </summary>
    Dark = 2,
}
