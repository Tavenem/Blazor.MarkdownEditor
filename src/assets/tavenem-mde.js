import Editor from '@toast-ui/editor';
import colorSyntax from '@toast-ui/editor-plugin-color-syntax';

window.tavenem = window.tavenem || {};
window.tavenem.blazor = window.tavenem.blazor || {};
window.tavenem.blazor.markdownEditor = window.tavenem.blazor.markdownEditor || {};

export function disposeEditor(elementId) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor && editor.editor) {
            editor.editor.destroy();
        }
        delete window.tavenem.blazor.markdownEditor._editorInstances[elementId];
    }
}

export function getEditorValue(elementId) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor && editor.editor && editor.editor.getMarkdown) {
            return editor.editor.getMarkdown();
        }
    }
    return null;
}

export function getPreferredColorScheme() {
    if (window.matchMedia) {
        if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 2;
        } else {
            return 1;
        }
    }
    return 1;
}

export function initializeEditor(dotNetObjectRef, elementId, value, options, customButtons) {
    const preferredScheme = getPreferredColorScheme();

    const element = document.getElementById(elementId);
    if (element) {
        options = options || {};

        let editorOptions = {
            el: element,
            autofocus: false,
            extendedAutolinks: true,
            height: '100%',
            hideModeSwitch: options.lockEditMode || false,
            initialEditType: options.editMode === 1 ? 'wysiwyg' : 'markdown',
            initialValue: value || '',
            placeholder: options.placeholder || '',
            previewStyle: options.previewStyle === 1 ? 'tab' : 'vertical',
            plugins: [colorSyntax],
            events: {
                blur: function () {
                    dotNetObjectRef.invokeMethodAsync("UpdateComponentValue");
                }
            }
        };

        if (customButtons && customButtons.length) {
            let buttons = [];
            for (let i = 0; i < customButtons.length; i++) {
                const el = document.createElement('button');
                el.id = customButtons[i].id;
                el.editorId = elementId;
                el.className = 'toastui-editor-toolbar-icons';
                el.type = 'button';
                el.style = 'background-image: none;color: inherit;margin: 0;padding-left: .5rem;padding-right: .5rem;width: auto';
                el.textContent = customButtons[i].name;
                el.addEventListener('click', function (e) {
                    if (e.target.editorId && window.tavenem.blazor.markdownEditor._editorInstances) {
                        const editor = window.tavenem.blazor.markdownEditor._editorInstances[e.target.editorId];
                        if (editor && editor.dotNetObjectRef && editor.editor) {
                            const text = editor.editor.getSelectedText();
                            editor.dotNetObjectRef.invokeMethodAsync("InvokeCustomButton", e.target.id, text);
                        }
                    }
                });
                buttons.push({
                    name: customButtons[i].name,
                    tooltip: customButtons[i].tooltip,
                    el: el,
                });
            }
            editorOptions.toolbarItems = [
                ['heading', 'bold', 'italic', 'strike'],
                ['hr', 'quote'],
                ['ul', 'ol', 'task', 'indent', 'outdent'],
                ['table', 'image', 'link'],
                ['code', 'codeblock'],
                buttons,
                ['scrollSync'],
            ];
        }

        const editor = new Editor(editorOptions);

        window.tavenem.blazor.markdownEditor._editorInstances = window.tavenem.blazor.markdownEditor._editorInstances || {};
        window.tavenem.blazor.markdownEditor._editorInstances[elementId] = {
            dotNetObjectRef: dotNetObjectRef,
            elementId: elementId,
            editor: editor,
            theme: options.theme || 0,
        };
    }

    return preferredScheme;
}

export function initializeViewer(dotNetObjectRef, elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
        const viewer = Editor.factory({
            el: document.getElementById(elementId),
            extendedAutolinks: true,
            height: 'auto',
            initialValue: value || '',
            viewer: true,
            plugins: [colorSyntax],
        });

        window.tavenem.blazor.markdownEditor._editorInstances = window.tavenem.blazor.markdownEditor._editorInstances || {};
        window.tavenem.blazor.markdownEditor._editorInstances[elementId] = {
            dotNetObjectRef: dotNetObjectRef,
            elementId: elementId,
            editor: viewer,
        };
    }
}

export function setEditMode(elementId, value) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor && editor.editor) {
            editor.editor.changeMode(value === 1 ? 'wysiwyg' : 'markdown');
        }
    }
}

export function setEditorPreviewStyle(elementId, value) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor && editor.editor) {
            editor.editor.changePreviewStyle(value === 1 ? 'tab' : 'vertical');
        }
    }
}

export function setEditorTheme(elementId, value) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor) {
            editor.theme = value;
        }
    }
}

export function setEditorValue(elementId, value) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor && editor.editor) {
            editor.editor.setMarkdown(value || '');
        }
    }
}

export function updateEditorSelection(elementId, value) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
        if (editor && editor.editor) {
            editor.editor.replaceSelection(value || '');
        }
    }
}

function setColorScheme(theme) {
    if (window.tavenem.blazor.markdownEditor._editorInstances) {
        for (const elementId in window.tavenem.blazor.markdownEditor._editorInstances) {
            const editor = window.tavenem.blazor.markdownEditor._editorInstances[elementId];
            if (editor && editor.dotNetObjectRef && editor.editor && !editor.editor.isViewer() && editor.theme === 0) {
                editor.dotNetObjectRef.invokeMethodAsync("UpdateComponentTheme", theme);
            }
        }
    }
}

if (window.matchMedia) {
    var colorSchemeQuery = window.matchMedia('(prefers-color-scheme: dark)');
    colorSchemeQuery.addEventListener('change', setColorScheme(getPreferredColorScheme()));
}
