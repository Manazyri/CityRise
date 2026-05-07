#nullable enable

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CityRise.Debug
{
    /// <summary>
    /// In-scene UI Toolkit console. Tilde/backtick toggles visibility. Builds its overlay
    /// dynamically (no UXML), so dropping the component on any UIDocument-bearing GameObject
    /// is sufficient to enable the console.
    /// </summary>
    /// <remarks>
    /// On first open the registry scans loaded assemblies for <see cref="DebugCommandAttribute"/>-marked
    /// methods. <see cref="ActiveRegistry"/> is exposed statically so commands like <c>help</c>
    /// can introspect the registry without holding their own reference.
    /// </remarks>
    /// <remarks>
    /// Input handling is done manually against the Input System's Keyboard rather than via a UI
    /// Toolkit TextField. Unity 6's TextField + new Input System combo has a known
    /// ArgumentOutOfRangeException bug in DeleteSelection when typing; the manual-Label approach
    /// is dependency-light and avoids the bug entirely.
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DebugConsole : MonoBehaviour
    {
        private const string OutputClass = "debug-console__output";
        private const string InputClass = "debug-console__input";
        private const string SuggestionsClass = "debug-console__suggestions";
        private const string SuggestionItemClass = "debug-console__suggestion";
        private const string ContainerClass = "debug-console";
        private const int MaxOutputLines = 200;
        private const int MaxSuggestions = 8;

        [SerializeField] private UIDocument _document = null!;

        /// <summary>The most recently active console's registry. Helper for static commands like <c>help</c>.</summary>
        public static DebugConsoleRegistry? ActiveRegistry { get; private set; }

        private DebugConsoleRegistry? _registry;
        private VisualElement? _container;
        private ScrollView? _outputScroll;
        private Label? _output;
        private Label? _input;
        private VisualElement? _suggestions;
        private readonly List<string> _outputLines = new();
        private readonly List<string> _historyEntries = new();
        private readonly StringBuilder _currentInput = new();
        private int _historyIndex = -1;
        private bool _open;

        private void Reset()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }
            EnsureRegistry();
            BuildOverlay();
            SetVisible(false);

            // Global text-input channel from the Input System. Fires for every typed character
            // regardless of UI Toolkit focus, handles shift/layout/dead-keys natively. Filtered
            // by _open state inside the handler.
            if (Keyboard.current != null)
            {
                Keyboard.current.onTextInput += OnTextInput;
            }
        }

        private void OnDisable()
        {
            if (Keyboard.current != null)
            {
                Keyboard.current.onTextInput -= OnTextInput;
            }
            DestroyOverlay();
        }

        private void OnTextInput(char c)
        {
            if (!_open) return;
            // The toggle key — don't append it as a character.
            if (c == '`' || c == '~') return;
            // Special keys (backspace, enter, tab, escape) come through here as control chars
            // on some platforms; we handle them via polling in Update so they don't double-fire.
            if (char.IsControl(c)) return;
            _currentInput.Append(c);
            RenderInputAndSuggestions();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Backquote (`) — global toggle, polled so it works whether or not the console
            // is open or focused.
            if (keyboard.backquoteKey.wasPressedThisFrame)
            {
                ToggleOpen();
                return;
            }

            if (!_open) return;

            // Special keys via polling — they don't go through onTextInput as printable chars.
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                Submit();
                return;
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                SetVisible(false);
                return;
            }
            if (keyboard.backspaceKey.wasPressedThisFrame)
            {
                if (_currentInput.Length > 0)
                {
                    _currentInput.Length -= 1;
                    RenderInputAndSuggestions();
                }
                return;
            }
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                ApplyTabCompletion();
                return;
            }
            if (keyboard.upArrowKey.wasPressedThisFrame && _historyEntries.Count > 0)
            {
                _historyIndex = Mathf.Max(0, _historyIndex - 1);
                ReplaceCurrentInput(_historyEntries[_historyIndex]);
                return;
            }
            if (keyboard.downArrowKey.wasPressedThisFrame && _historyEntries.Count > 0)
            {
                _historyIndex = Mathf.Min(_historyEntries.Count, _historyIndex + 1);
                ReplaceCurrentInput(_historyIndex < _historyEntries.Count
                    ? _historyEntries[_historyIndex]
                    : string.Empty);
                return;
            }

            // Caret blink — repaint every frame so the rendered text alternates.
            if (_input != null) _input.text = "> " + _currentInput + GetCaret();
        }

        /// <summary>Test helper — current open state.</summary>
        public bool IsOpen => _open;
        /// <summary>Test helper — current text in the input buffer.</summary>
        public string CurrentInput => _currentInput.ToString();


        private void Submit()
        {
            if (_registry == null) return;
            var line = _currentInput.ToString();
            if (!string.IsNullOrWhiteSpace(line))
            {
                Print($"> {line}");
                var output = _registry.Execute(line);
                if (!string.IsNullOrEmpty(output)) Print(output);
                _historyEntries.Add(line);
                _historyIndex = _historyEntries.Count;
            }
            _currentInput.Clear();
            RenderInputAndSuggestions();
        }

        private void ApplyTabCompletion()
        {
            if (_registry == null) return;
            var current = _currentInput.ToString();
            var head = HeadToken(current);
            if (head.Length == 0) return;
            var suggestions = _registry.Suggest(head);
            if (suggestions.Count == 0) return;
            var rest = current.Length > head.Length ? current.Substring(head.Length) : string.Empty;
            ReplaceCurrentInput(suggestions[0] + rest);
        }

        private void ReplaceCurrentInput(string text)
        {
            _currentInput.Clear();
            _currentInput.Append(text);
            RenderInputAndSuggestions();
        }

        private void RenderInputAndSuggestions()
        {
            if (_input != null) _input.text = "> " + _currentInput + GetCaret();
            RefreshSuggestions();
        }

        private string GetCaret()
        {
            // Half-second blink rate — purely cosmetic, no actual cursor positioning.
            return ((int)(Time.unscaledTime * 2f) & 1) == 0 ? "_" : " ";
        }

        private void EnsureRegistry()
        {
            if (_registry != null) return;
            _registry = new DebugConsoleRegistry();
            _registry.ScanLoadedAssemblies();
            ActiveRegistry = _registry;
        }

        private void BuildOverlay()
        {
            var root = _document.rootVisualElement;
            if (root == null) return;
            if (_container != null) return;

            _container = new VisualElement { name = "debug-console" };
            _container.AddToClassList(ContainerClass);
            ApplyContainerStyle(_container);

            // ScrollView wraps the output Label so long transcripts scroll cleanly instead of
            // overflowing/clipping. flex-grow:1 makes it consume the space above the input.
            _outputScroll = new ScrollView(ScrollViewMode.Vertical) { name = "debug-console-scroll" };
            ApplyOutputScrollStyle(_outputScroll);

            _output = new Label { name = "debug-console-output" };
            _output.AddToClassList(OutputClass);
            ApplyOutputStyle(_output);
            _outputScroll.Add(_output);

            _input = new Label { name = "debug-console-input", text = "> " };
            _input.AddToClassList(InputClass);
            ApplyInputStyle(_input);

            _suggestions = new VisualElement { name = "debug-console-suggestions" };
            _suggestions.AddToClassList(SuggestionsClass);
            ApplySuggestionsStyle(_suggestions);

            // Input on top so the user's typing focus is at a stable y-coordinate, suggestions
            // immediately below (so autocomplete sits next to what's being typed), then the
            // scrolling log fills the rest of the space.
            _container.Add(_input);
            _container.Add(_suggestions);
            _container.Add(_outputScroll);

            // Add as a child of hud-root (HudShell's UXML root) so we inherit the
            // -unity-font cascade from .hud-root in HudShell.uss. Falling back to the panel
            // root would render glyph-less Labels (font wouldn't cascade).
            var fontHost = root.Q("hud-root") ?? root;
            fontHost.Add(_container);
        }

        private void DestroyOverlay()
        {
            _container?.RemoveFromHierarchy();
            _container = null;
            _outputScroll = null;
            _output = null;
            _input = null;
            _suggestions = null;
        }

        private void ToggleOpen() => SetVisible(!_open);

        private void SetVisible(bool open)
        {
            _open = open;
            CityRise.Core.InputContext.SuppressGameplayHotkeys = open;
            if (_container == null) return;
            _container.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            if (open)
            {
                _currentInput.Clear();
                RenderInputAndSuggestions();
                // No Focus() needed — input arrives via Keyboard.current.onTextInput which
                // is independent of UI Toolkit focus.
            }
        }

        private void RefreshSuggestions()
        {
            if (_suggestions == null || _registry == null) return;
            _suggestions.Clear();
            var head = HeadToken(_currentInput.ToString());
            if (head.Length == 0) return;
            var matches = _registry.Suggest(head);
            for (int i = 0; i < matches.Count && i < MaxSuggestions; i++)
            {
                var item = new Label(matches[i]);
                item.AddToClassList(SuggestionItemClass);
                ApplySuggestionItemStyle(item);
                _suggestions.Add(item);
            }
        }

        private void Print(string line)
        {
            _outputLines.Add(line);
            while (_outputLines.Count > MaxOutputLines) _outputLines.RemoveAt(0);
            if (_output != null) _output.text = string.Join("\n", _outputLines);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (_outputScroll == null) return;
            // Defer until next layout pass so the new line's height is accounted for in
            // contentContainer.layout — without the schedule, scrollOffset gets clamped to the
            // pre-update content extent.
            _outputScroll.schedule.Execute(() =>
            {
                if (_outputScroll == null) return;
                var contentHeight = _outputScroll.contentContainer.layout.height;
                _outputScroll.scrollOffset = new Vector2(0f, contentHeight);
            }).StartingIn(0);
        }

        private static string HeadToken(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (char.IsWhiteSpace(line[i])) return line.Substring(0, i);
            }
            return line;
        }

        private static void ApplyContainerStyle(VisualElement v)
        {
            v.style.position = Position.Absolute;
            v.style.left = 0;
            v.style.right = 0;
            v.style.top = 48; // sit just below the top-bar
            v.style.height = new Length(40, LengthUnit.Percent);
            v.style.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            v.style.borderBottomWidth = 1;
            v.style.borderBottomColor = new Color(1f, 1f, 1f, 0.10f);
            v.style.flexDirection = FlexDirection.Column;
            v.style.paddingLeft = 8;
            v.style.paddingRight = 8;
            v.style.paddingTop = 8;
            v.style.paddingBottom = 8;
        }

        private static void ApplyOutputScrollStyle(ScrollView scroll)
        {
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 0; // allow shrink in flex parent
        }

        private static void ApplyOutputStyle(Label label)
        {
            // Lives inside a ScrollView. Natural top-down flow: oldest at top, newest at
            // bottom; ScrollView auto-scrolls to bottom on each Print so the newest line is
            // always visible.
            label.style.color = new Color(0.91f, 0.89f, 0.82f, 1f);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.unityTextAlign = TextAnchor.UpperLeft;
            label.style.fontSize = 13;
        }

        private static void ApplyInputStyle(Label label)
        {
            label.style.flexShrink = 0;
            label.style.height = 28;
            label.style.marginTop = 4;
            label.style.fontSize = 13;
            label.style.color = new Color(0.95f, 0.92f, 0.84f, 1f);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.backgroundColor = new Color(0.02f, 0.04f, 0.07f, 0.6f);
            label.style.paddingLeft = 6;
            label.style.paddingRight = 6;
        }

        private static void ApplySuggestionsStyle(VisualElement v)
        {
            v.style.flexShrink = 0;
            v.style.flexDirection = FlexDirection.Row;
            v.style.flexWrap = Wrap.Wrap;
            v.style.marginTop = 4;
        }

        private static void ApplySuggestionItemStyle(Label l)
        {
            l.style.color = new Color(0.65f, 0.78f, 0.95f, 1f);
            l.style.marginRight = 12;
            l.style.fontSize = 12;
        }
    }
}
