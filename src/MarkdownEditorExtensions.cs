using Tavenem.Blazor.MarkdownEditor;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension to <c>Microsoft.Extensions.DependencyInjection</c> for
/// <c>Tavenem.Blazor.MarkdownEditor</c>.
/// </summary>
public static class MarkdownEditorExtensions
{
    /// <summary>
    /// Add the required service for <see cref="MarkdownEditor"/>.
    /// </summary>
    /// <param name="services">Your <see cref="IServiceCollection"/> instance.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMarkdownEditor(this IServiceCollection services)
    {
        services.AddScoped<MarkdownEditorJsInterop>();
        return services;
    }
}
