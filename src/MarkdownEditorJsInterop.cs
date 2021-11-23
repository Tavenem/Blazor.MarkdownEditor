using Microsoft.JSInterop;

namespace Tavenem.Blazor.MarkdownEditor;

/// <summary>
/// Provides encapsulated javascript interop for <c>Tavenem.Blazor.MarkdownEditor</c>.
/// </summary>
public class MarkdownEditorJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    private bool _disposed;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="jsRuntime">The injected <see cref="IJSRuntime"/> instance.</param>
    public MarkdownEditorJsInterop(IJSRuntime jsRuntime)
        => _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Tavenem.Blazor.MarkdownEditor/tavenem-mde.js").AsTask());

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous dispose operation.
    /// </returns>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.DisposeAsync().ConfigureAwait(false);
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes an editor instance.
    /// </summary>
    /// <param name="elementId">The id of the host HTML element.</param>
    public async ValueTask DisposeEditor(string elementId)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("disposeEditor", elementId).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the current markdown text of an editor instance.
    /// </summary>
    /// <param name="elementId">The id of the host HTML element.</param>
    public async ValueTask<string> GetEditorValue(string elementId)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        return await module.InvokeAsync<string>("getEditorValue", elementId).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the user's preferred color scheme. Defaults to <see
    /// cref="MarkdownEditorTheme.Light"/> when no preference can be detected.
    /// </summary>
    /// <returns>The user's preferred <see cref="MarkdownEditorTheme"/>.</returns>
    public async ValueTask<MarkdownEditorTheme> GetPreferredColorScheme()
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        return await module.InvokeAsync<MarkdownEditorTheme>("getPreferredColorScheme").ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes a markdown editor instance.
    /// </summary>
    /// <param name="dotNetObjectRef">
    /// A <see cref="DotNetObjectReference{T}"/> for the <see cref="MarkdownEditor"/> instance.
    /// </param>
    /// <param name="elementId">The id of the host HTML element.</param>
    /// <param name="value">The initial markdown text.</param>
    /// <param name="editMode">The initial <see cref="EditMode"/>.</param>
    /// <param name="lockEditMode">Whether to prevent the user from changing the edit mode.</param>
    /// <param name="placeholder">The placeholder text.</param>
    /// <param name="previewStyle">The <see cref="MarkdownPreviewStyle"/>.</param>
    /// <param name="theme">The initial <see cref="MarkdownEditorTheme"/>.</param>
    /// <param name="customToolbarButtons">
    /// An optional set of <see cref="MarkdownEditorButton"/> instances.
    /// </param>
    /// <returns>
    /// The user's preferred color scheme.
    /// </returns>
    public async ValueTask<MarkdownEditorTheme> InitializeEditor(
        DotNetObjectReference<MarkdownEditor> dotNetObjectRef,
        string elementId,
        string? value = null,
        EditMode editMode = EditMode.Markdown,
        bool lockEditMode = false,
        string? placeholder = null,
        MarkdownPreviewStyle previewStyle = MarkdownPreviewStyle.Split,
        MarkdownEditorTheme theme = MarkdownEditorTheme.Auto,
        List<MarkdownEditorButton>? customToolbarButtons = null)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        var options = new
        {
            editMode,
            lockEditMode,
            placeholder,
            previewStyle,
            theme,
        };

        List<CustomButton>? customButtons = null;
        if (customToolbarButtons?.Count > 0)
        {
            customButtons = new();
            foreach (var button in customToolbarButtons)
            {
                customButtons.Add(new()
                {
                    Id = button.Id,
                    Name = button.Text ?? button.Id,
                    Tooltip = button.Tooltip,
                });
            }
        }

        return await module.InvokeAsync<MarkdownEditorTheme>(
            "initializeEditor",
            dotNetObjectRef,
            elementId,
            value,
            options,
            customButtons).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes a markdown viewer instance.
    /// </summary>
    /// <param name="dotNetObjectRef">
    /// A <see cref="DotNetObjectReference{T}"/> for the <see cref="MarkdownEditor"/> instance.
    /// </param>
    /// <param name="elementId">The id of the host HTML element.</param>
    /// <param name="value">The initial markdown text.</param>
    public async ValueTask InitializeViewer(
        DotNetObjectReference<MarkdownViewer> dotNetObjectRef,
        string elementId,
        string? value = null)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync(
            "initializeViewer",
            dotNetObjectRef,
            elementId,
            value).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the edit mode of an editor instance.
    /// </summary>
    /// <param name="elementId">The id of the HTML host element.</param>
    /// <param name="value">The <see cref="EditMode"/> to set.</param>
    public async ValueTask SetEditMode(
        string elementId,
        EditMode value)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync(
            "setEditMode",
            elementId,
            value).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the preview style of an editor instance.
    /// </summary>
    /// <param name="elementId">The id of the HTML host element.</param>
    /// <param name="value">The <see cref="MarkdownPreviewStyle"/> to set.</param>
    public async ValueTask SetEditorPreviewStyle(
        string elementId,
        MarkdownPreviewStyle value)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync(
            "setEditorPreviewStyle",
            elementId,
            value).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the theme of an editor instance.
    /// </summary>
    /// <param name="elementId">The id of the HTML host element.</param>
    /// <param name="value">The <see cref="MarkdownEditorTheme"/> to set.</param>
    public async ValueTask SetEditorTheme(
        string elementId,
        MarkdownEditorTheme value)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync(
            "setEditorTheme",
            elementId,
            value).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the markdown text of an editor instance.
    /// </summary>
    /// <param name="elementId">The id of the HTML host element.</param>
    /// <param name="value">The markdown text to set.</param>
    public async ValueTask SetEditorValue(
        string elementId,
        string? value)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync(
            "setEditorValue",
            elementId,
            value).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces the current selected text in an editor instance with the given
    /// value.
    /// </summary>
    /// <param name="elementId">The id of the HTML host element.</param>
    /// <param name="value">The text with which to replace the current
    /// selection.</param>
    public async ValueTask UpdateEditorSelection(
        string elementId,
        string? value)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync(
            "updateEditorSelection",
            elementId,
            value).ConfigureAwait(false);
    }
}
