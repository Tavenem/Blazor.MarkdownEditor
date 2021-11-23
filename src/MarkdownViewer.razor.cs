using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavenem.Blazor.MarkdownEditor;

/// <summary>
/// <para>
/// A markdown viewer.
/// </para>
/// <para>
/// Use CSS class "tavenem-markdownViewer" for custom styling.
/// </para>
/// </summary>
public partial class MarkdownViewer : IAsyncDisposable
{
    private bool _disposed;
    private DotNetObjectReference<MarkdownViewer>? _dotNetObjectRef;
    private bool _initialized;

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
    /// The raw markdown text.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    [Inject] private MarkdownEditorJsInterop? JsInterop { get; set; }

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
        string? newValue = null;
        var pendingValue = false;
        if (_initialized
            && parameters.TryGetValue<string>(nameof(Value), out newValue)
            && newValue != Value)
        {
            pendingValue = true;
        }
        await base.SetParametersAsync(parameters)
            .ConfigureAwait(false);

        if (pendingValue)
        {
            await SetValueAsync(newValue)
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
            await JsInterop
                .InitializeViewer(
                    _dotNetObjectRef,
                    Id,
                    Value)
                .ConfigureAwait(false);
            _initialized = true;
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
}
