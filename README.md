![build](https://img.shields.io/github/workflow/status/Tavenem/Blazor.MarkdownEditor/publish/main) [![NuGet downloads](https://img.shields.io/nuget/dt/Tavenem.Blazor.MarkdownEditor)](https://www.nuget.org/packages/Tavenem.Blazor.MarkdownEditor/)

Tavenem.Blazor.MarkdownEditor
==

Tavenem.Blazor.MarkdownEditor is a [Razor class
library](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class) (RCL) containing a
[Razor component](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries).
It allows displaying a [WYSIWYG](https://en.wikipedia.org/wiki/WYSIWYG) editor for Markdown text,
or an editor with preview (and the ability to toggle between modes). It also includes a markdown
viewer which renders markdown as HTML.

It is a wrapper for the [TOAST UI Editor](https://ui.toast.com/tui-editor) javascript library.

## Installation

Tavenem.Blazor.MarkdownEditor is available as a [NuGet package](https://www.nuget.org/packages/Tavenem.Blazor.MarkdownEditor/).

## Use

1. Call the `AddMarkdownEditor()` extension method on your `IServiceCollection`.

1. Add a link to the stylesheet in your `index.html` or `_Host.cshtml` file:

```
    <link href="_content/Tavenem.Blazor.MarkdownEditor/styles.css" rel="stylesheet" />
```

1. Include a `MarkdownEditor` or `MarkdownViewer` component on a page.

The current markdown text is bound to the `Value` property. This property updates on blur.

The value can also be set programatically at any time with the `SetValueAsync` method.

If the value must be retrieved before the blur event, the `GetValueAsync` method forces an update, and also returns the current markdown text.

## Customization

The starting edit mode (markdown or WYSIWYG) can be configured with the
`EditMode` property. It can be changed at any time programatically by setting
the property to a new value, and can also be changed by the user unless
`LockEditMode` has been set to true.

Note that the `EditMode` property does *not* update when changed by the user, so
it can't be used to identify the current mode. It is a one-way binding.

The preview mode (tabs or vertical split view) can be set with the
`PreviewStyle` property, and changed programatically at any time by updating the
value of the property. There is no built-in UI which allows the user to toggle
preview styles.

### Theme

The editor supports light and dark modes with its `Theme` property. The theme
can be assigned a specific value, or you can select `Auto` to use the user's
preferred theme (this is the default behavior). When set to `Auto` the theme
will also switch automatically if the user updates their preference. The theme
can also be updated programatically at any time by changing the value of the
`Theme` property.

The viewer uses a transparent background and inherits foreground color from its
parent, in order to render its content seamlessly in context.

### Customizing the toolbar

You can provide one or more `MarkdownEditorButton` instances to a
`MarkdownEditor` component's `CustomToolbarButtons` property to add custom
buttons to the editor's toolbar.

Each button defines the `Text` displayed on the button and an optional `Tooltip`
shown on hover. The `MarkdownEditorButton` class also provides both the `Action`
and `AsyncAction` properties. Either one should be assigned a function which
accepts the currently selected text and returns the new text to be inserted in
its place.

If *both* `Action` and `AsyncAction` are assigned functions, `Action` will be
invoked first, then `AsyncAction` will be invoked using the result of `Action`
as its input.

## Roadmap

Updates to Tavenem.Blazor.MarkdownEditor may be released whenever new releases
of the underlying [TOAST UI Editor](https://ui.toast.com/tui-editor) library are
made.

## Contributing

Contributions are always welcome. Please carefully read the [contributing](docs/CONTRIBUTING.md) document to learn more before submitting issues or pull requests.

## Code of conduct

Please read the [code of conduct](docs/CODE_OF_CONDUCT.md) before engaging with our community, including but not limited to submitting or replying to an issue or pull request.