using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Tavenem.Blazor.MarkdownEditor;

/// <summary>
/// <para>
/// A markdown editor with WYSIWYG, and side-by-side code and preview modes.
/// </para>
/// <para>
/// Use CSS class "tavenem-markdownEditor" for custom styling.
/// </para>
/// </summary>
public partial class MarkdownEditor : IAsyncDisposable
{
    private bool _disposed;
    private DotNetObjectReference<MarkdownEditor>? _dotNetObjectRef;
    private bool _initialized;

    /// <summary>
    /// Invoked after the raw markdown text changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This can be used to hook save-on-update logic, or any other process
    /// which must be invoked after any update.
    /// </para>
    /// <para>
    /// It is always invoked directly after <see cref="ValueChanged"/>, and is
    /// provided as a separate event to enable easy property binding, with an
    /// additional event hook.
    /// </para>
    /// </remarks>
    [Parameter] public EventCallback AfterChange { get; set; }

    /// <summary>
    /// <para>
    /// An optional set of custom toolbar buttons.
    /// </para>
    /// <para>
    /// Changes to this property are ignored after initialization.
    /// </para>
    /// </summary>
    [Parameter] public List<MarkdownEditorButton>? CustomToolbarButtons { get; set; }

    /// <summary>
    /// <para>
    /// Controls whether the editor is in Markdown or WYSIWYG mode.
    /// </para>
    /// <para>
    /// Note: this parameter does not update when the user switches modes using
    /// the built-in UI. You may, however, update it after initialization to
    /// force a mode switch.
    /// </para>
    /// </summary>
    [Parameter] public EditMode EditMode { get; set; }

    /// <summary>
    /// <para>
    /// The id of the host HTML element.
    /// </para>
    /// <para>
    /// Will be set to a random GUID if left unset.
    /// </para>
    /// </summary>
    /// <remarks>
    /// In most situations this can be left unset. It can be manually configured
    /// when you intend to provide custom CSS for this specific editor instance.
    /// </remarks>
    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Attribute splatting property.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> InputAttributes { get; set; } = new();

    /// <summary>
    /// <para>
    /// If <see langword="true"/> the user will not be able to toggle between
    /// markdown and WYSIWYG modes with the built-in UI. You can still toggle
    /// modes programatically.
    /// </para>
    /// <para>
    /// Changes to this property are ignored after initialization.
    /// </para>
    /// </summary>
    [Parameter] public bool LockEditMode { get; set; }

    /// <summary>
    /// <para>
    /// The placeholder text displayed when the editor is empty.
    /// </para>
    /// <para>
    /// Changes to this property are ignored after initialization.
    /// </para>
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Whether the editor shows a preview of the rendered HTML in a tab, or a
    /// vertical split view, when in Markdown editing mode.
    /// </summary>
    [Parameter] public MarkdownPreviewStyle PreviewStyle { get; set; }

    /// <summary>
    /// <para>
    /// The color theme of the component.
    /// </para>
    /// <para>
    /// Default is "Auto," which sets light or dark mode based on the user's OS
    /// preference (and stays updated if the user changes modes).
    /// </para>
    /// </summary>
    [Parameter] public MarkdownEditorTheme Theme { get; set; }

    /// <summary>
    /// The raw markdown text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: this value is updated when the editor's <c>onblur</c> event fires,
    /// and not in real time, to avoid performance issues.
    /// </para>
    /// <para>
    /// To force an immediate update and get the current value, call <see
    /// cref="GetValueAsync"/>.
    /// </para>
    /// </remarks>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Invoked when the raw markdown text changes.
    /// </summary>
    /// <remarks>
    /// Note: this event is invoked after the editor's <c>onblur</c> event
    /// fires, and not in real time, to avoid performance issues.
    /// </remarks>
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    [Inject] private MarkdownEditorJsInterop? JsInterop { get; set; }

    private MarkdownEditorTheme CurrentTheme { get; set; }

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
            if (JsInterop is not null)
            {
                await JsInterop
                    .DisposeEditor(Id)
                    .ConfigureAwait(false);
            }
            _dotNetObjectRef?.Dispose();
            _dotNetObjectRef = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Sets parameters supplied by the component's parent in the render tree.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>A <see cref="Task" /> that completes when the component has
    /// finished updating and rendering itself.</returns>
    /// <remarks>
    /// <para>
    /// Parameters are passed when <see
    /// cref="ComponentBase.SetParametersAsync(ParameterView)" /> is called. It
    /// is not required that the caller supply a parameter value for all of the
    /// parameters that are logically understood by the component.
    /// </para>
    /// <para>
    /// The default implementation of <see
    /// cref="ComponentBase.SetParametersAsync(ParameterView)" /> will set the
    /// value of each property decorated with <see cref="ParameterAttribute" /> or <see
    /// cref="CascadingParameterAttribute" /> that has a corresponding value in the <see
    /// cref="ParameterView" />. Parameters that do not have a corresponding
    /// value will be unchanged.
    /// </para>
    /// </remarks>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        string? newValue;
        var pendingValue = false;

        MarkdownEditorTheme newTheme;
        var pendingTheme = false;

        EditMode newMode;
        var pendingMode = false;

        MarkdownPreviewStyle newStyle;
        var pendingStyle = false;

        if (_initialized)
        {
            if (parameters.TryGetValue<string>(nameof(Value), out newValue)
                && newValue != Value)
            {
                pendingValue = true;
            }

            if (parameters.TryGetValue(nameof(Theme), out newTheme)
                && newTheme != Theme)
            {
                pendingTheme = true;
            }

            if (parameters.TryGetValue(nameof(EditMode), out newMode))
            {
                pendingMode = true;
            }

            if (parameters.TryGetValue(nameof(PreviewStyle), out newStyle)
                && newStyle != PreviewStyle)
            {
                pendingStyle = true;
            }
        }
        else
        {
            if (parameters.TryGetValue<string>(nameof(Value), out newValue)
                && newValue != Value)
            {
                Value = newValue;
            }

            if (parameters.TryGetValue(nameof(Theme), out newTheme)
                && newTheme != Theme)
            {
                Theme = newTheme;
                CurrentTheme = newTheme;
            }

            if (parameters.TryGetValue(nameof(EditMode), out newMode))
            {
                EditMode = newMode;
            }

            if (parameters.TryGetValue(nameof(PreviewStyle), out newStyle)
                && newStyle != PreviewStyle)
            {
                PreviewStyle = newStyle;
            }
        }

        await base.SetParametersAsync(parameters)
            .ConfigureAwait(false);

        if (pendingValue)
        {
            await SetValueAsync(newValue)
                .ConfigureAwait(false);
        }

        if (pendingTheme)
        {
            await SetThemeAsync(newTheme)
                .ConfigureAwait(false);
        }

        if (pendingMode)
        {
            await SetEditModeAsync(newMode)
                .ConfigureAwait(false);
        }

        if (pendingStyle)
        {
            await SetEditorPreviewStyle(newStyle)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Method invoked after each time the component has been rendered. Note
    /// that the component does not automatically re-render after the completion
    /// of any returned <see cref="Task" />, because that would cause an
    /// infinite render loop.
    /// </summary>
    /// <param name="firstRender">
    /// Set to <see langword="true"/> if this is the first time <see
    /// cref="ComponentBase.OnAfterRender(bool)" /> has been invoked on this
    /// component instance; otherwise <see langword="false"/>.
    /// </param>
    /// <returns>A <see cref="Task" /> representing any asynchronous
    /// operation.</returns>
    /// <remarks>
    /// The <see cref="ComponentBase.OnAfterRender(bool)" /> and <see
    /// cref="ComponentBase.OnAfterRenderAsync(bool)" /> lifecycle methods are
    /// useful for performing interop, or interacting with values received from
    /// <c>@ref</c>. Use the <paramref name="firstRender" /> parameter to ensure
    /// that initialization work is only performed once.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && JsInterop is not null)
        {
            _dotNetObjectRef ??= DotNetObjectReference.Create(this);
            var theme = await JsInterop
                .InitializeEditor(
                    _dotNetObjectRef,
                    Id,
                    Value,
                    EditMode,
                    LockEditMode,
                    Placeholder,
                    PreviewStyle,
                    Theme,
                    CustomToolbarButtons)
                .ConfigureAwait(false);
            CurrentTheme = Theme == MarkdownEditorTheme.Auto
                ? theme
                : Theme;
            _initialized = true;
        }
    }

    /// <summary>
    /// <para>
    /// Gets the raw markdown text.
    /// </para>
    /// <para>
    /// Also forces an update of <see cref="Value"/> and invokes <see
    /// cref="ValueChanged"/>, if the value has changed.
    /// </para>
    /// </summary>
    public async ValueTask<string?> GetValueAsync()
    {
        if (!_initialized || JsInterop is null)
        {
            return Value;
        }

        var value = await JsInterop
            .GetEditorValue(Id)
            .ConfigureAwait(false);
        if (Value != value)
        {
            Value = value;
            await ValueChanged.InvokeAsync(Value).ConfigureAwait(false);
            await AfterChange.InvokeAsync().ConfigureAwait(false);
        }
        return value;
    }

    /// <summary>
    /// <para>
    /// Invokes the custom toolbar button with the given element id.
    /// </para>
    /// <para>
    /// This method is invoked by internal javascript, and is not intended to be
    /// called directly.
    /// </para>
    /// </summary>
    [JSInvokable]
    public async Task InvokeCustomButton(string id, string? text)
    {
        if (JsInterop is null)
        {
            return;
        }

        var button = CustomToolbarButtons?.Find(x => x.Id == id);
        if (button is null
            || (button.Action is null
            && button.AsyncAction is null))
        {
            return;
        }

        var newText = text;
        if (button.Action is not null)
        {
            newText = button.Action.Invoke(text);
        }
        if (button.AsyncAction is not null)
        {
            newText = await button
                .AsyncAction
                .Invoke(newText)
                .ConfigureAwait(false);
        }

        if (!string.Equals(newText, text, StringComparison.Ordinal))
        {
            await JsInterop.UpdateEditorSelection(Id, newText)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sets the raw markdown text.
    /// </summary>
    /// <param name="value">The raw markdown text.</param>
    public async Task SetValueAsync(string? value)
    {
        if (!_initialized || JsInterop is null)
        {
            return;
        }

        await JsInterop
            .SetEditorValue(Id, value)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <para>
    /// Prompts the component to synchronize its <see cref="Value"/> parameter
    /// with the editor.
    /// </para>
    /// <para>
    /// This method is invoked by internal javascript, and is not intended to be
    /// called directly.
    /// </para>
    /// </summary>
    [JSInvokable]
    public async Task UpdateComponentValue() => _ = await GetValueAsync()
        .ConfigureAwait(false);

    /// <summary>
    /// <para>
    /// Updates the auto theme.
    /// </para>
    /// <para>
    /// This method is invoked by internal javascript, and is not intended to be
    /// called directly. Doing so may cause the editor and the parameter value
    /// to become out of sync.
    /// </para>
    /// </summary>
    /// <param name="value">The updated theme.</param>
    [JSInvokable]
    public void UpdateComponentTheme(MarkdownEditorTheme value)
    {
        CurrentTheme = value;
        StateHasChanged();
    }

    private async Task SetEditModeAsync(EditMode value)
    {
        if (!_initialized || JsInterop is null)
        {
            return;
        }

        await JsInterop
            .SetEditMode(Id, value)
            .ConfigureAwait(false);
    }

    private async Task SetEditorPreviewStyle(MarkdownPreviewStyle value)
    {
        if (!_initialized || JsInterop is null)
        {
            return;
        }

        await JsInterop
            .SetEditorPreviewStyle(Id, value)
            .ConfigureAwait(false);
    }

    private async Task SetThemeAsync(MarkdownEditorTheme value)
    {
        if (!_initialized || JsInterop is null)
        {
            return;
        }

        if (value == MarkdownEditorTheme.Auto)
        {
            CurrentTheme = await JsInterop
                .GetPreferredColorScheme()
                .ConfigureAwait(false);
        }
        else
        {
            CurrentTheme = value;
        }

        await JsInterop
            .SetEditorTheme(Id, value)
            .ConfigureAwait(false);
    }
}
