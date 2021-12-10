namespace Tavenem.Blazor.MarkdownEditor.Sample.Pages;

public partial class Index
{
    private MarkdownEditorTheme BoundTheme { get; set; } = MarkdownEditorTheme.Dark;

    private List<MarkdownEditorButton> CustomButtons { get; } = new()
    {
        new()
        {
            Text = "Wikify",
            Tooltip = "Turn into a link to the Wikipedia article for the selected text",
            Action = text => string.IsNullOrWhiteSpace(text)
                ? text
                : $"[{text}](https://wikipedia.org/wiki/{text})",
        }
    };

    private string? Status { get; set; } = "Waiting for changes...";

    private string? EditorMarkdown { get; set; }

    private void OnChange() => Status = "Change event fired!";
}
