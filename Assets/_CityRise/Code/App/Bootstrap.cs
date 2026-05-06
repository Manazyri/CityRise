#nullable enable

using CityRise.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CityRise.App
{
    /// <summary>
    /// Composition root. Lives in the Boot scene; runs once at session start, instantiates Core
    /// facades into a <see cref="ServiceContainer"/>, then loads the Main scene. No DI framework —
    /// services are constructed explicitly so the dependency graph is grep-friendly.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [Header("Authored config (optional)")]
        [SerializeField] private FeatureFlags _featureFlags = null!;
        [SerializeField] private LocalizationTable _localizationTable = null!;
        [SerializeField] private AccessibilityConfig _accessibilityConfig = null!;

        [Header("Session")]
        [SerializeField] private string _mainSceneName = "Main";
        [SerializeField] private uint _initialSeed = 1u;

        private ServiceContainer? _services;

        /// <summary>Wired services. Read-only after Awake; never null after a successful Awake.</summary>
        public ServiceContainer? Services => _services;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _services = WireServices();
            Log.Info(LogCategory.App, "Bootstrap complete; loading scene '" + _mainSceneName + "'.");
            LoadMainScene();
        }

        private ServiceContainer WireServices()
        {
            var container = new ServiceContainer();

            if (_localizationTable != null)
            {
                I18n.SetProvider(_localizationTable);
            }

            var accessibility = _accessibilityConfig != null
                ? new AccessibilityService(_accessibilityConfig)
                : new AccessibilityService();
            container.Register<IAccessibilityService>(accessibility);

            if (_featureFlags != null)
            {
                container.Register(_featureFlags);
            }

            var rng = new RandomService(_initialSeed);
            container.Register<IRandom>(rng);

            container.Register(new NotificationBus());

            return container;
        }

        private void LoadMainScene()
        {
            if (string.IsNullOrEmpty(_mainSceneName))
            {
                Log.Error(LogCategory.App, "MainSceneName is empty; not loading.");
                return;
            }
            SceneManager.LoadScene(_mainSceneName, LoadSceneMode.Single);
        }
    }
}
