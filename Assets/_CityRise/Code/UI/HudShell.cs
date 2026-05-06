#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace CityRise.UI
{
    /// <summary>
    /// Phase 1 HUD shell. Loads the HudShell UXML on a UIDocument and resolves the named
    /// regions (top bar, bottom toolbar, right panel) for later phases to populate.
    /// No content yet — that arrives with the time-control UI (P1.D6) and the tool panels (P1.D7+).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HudShell : MonoBehaviour
    {
        [SerializeField] private UIDocument _document = null!;

        private VisualElement? _root;
        private VisualElement? _topBar;
        private VisualElement? _topBarStats;
        private VisualElement? _bottomToolbar;
        private VisualElement? _bottomToolbarSlots;
        private VisualElement? _rightPanel;

        public VisualElement? TopBar => _topBar;
        public VisualElement? TopBarStats => _topBarStats;
        public VisualElement? BottomToolbar => _bottomToolbar;
        public VisualElement? BottomToolbarSlots => _bottomToolbarSlots;
        public VisualElement? RightPanel => _rightPanel;

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

            _root = _document.rootVisualElement;
            if (_root == null)
            {
                Debug.LogWarning("[CityRise.UI] HudShell: UIDocument has no rootVisualElement; UXML missing or not yet built.");
                return;
            }

            _topBar = _root.Q<VisualElement>("top-bar");
            _topBarStats = _root.Q<VisualElement>("top-bar__stats");
            _bottomToolbar = _root.Q<VisualElement>("bottom-toolbar");
            _bottomToolbarSlots = _root.Q<VisualElement>("bottom-toolbar__slots");
            _rightPanel = _root.Q<VisualElement>("right-panel");
        }

        private void OnDisable()
        {
            _root = null;
            _topBar = null;
            _topBarStats = null;
            _bottomToolbar = null;
            _bottomToolbarSlots = null;
            _rightPanel = null;
        }
    }
}
