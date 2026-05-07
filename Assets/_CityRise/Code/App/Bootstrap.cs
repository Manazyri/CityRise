#nullable enable

using CityRise.Core;
using CityRise.Persistence;
using CityRise.Simulation.Infrastructure;
using CityRise.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CityRise.App
{
    /// <summary>
    /// Composition root. Lives in the Boot scene; runs once at session start, instantiates Core
    /// facades and sim infrastructure into a <see cref="ServiceContainer"/>, then loads the Main
    /// scene. No DI framework — services are constructed explicitly so the dependency graph is
    /// grep-friendly. Drives the <see cref="TickScheduler"/> via its Update method.
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
        private TickScheduler? _tickScheduler;
        private SaveManifest? _saveManifest;

        /// <summary>Singleton-style accessor for in-scene MonoBehaviours that need ServiceContainer.</summary>
        public static Bootstrap? Instance { get; private set; }

        /// <summary>Wired services. Read-only after Awake; never null after a successful Awake.</summary>
        public ServiceContainer? Services => _services;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);
            _services = WireServices();
            SceneManager.sceneLoaded += OnSceneLoaded;
            Log.Info(LogCategory.App, "Bootstrap complete; loading scene '" + _mainSceneName + "'.");
            LoadMainScene();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Instance = null;
            }
        }

        private void Update()
        {
            // Sim time is driven from real wall-clock independent of Time.timeScale; the
            // scheduler's own SpeedMultiplier handles in-game time scaling.
            _tickScheduler?.Update(Time.unscaledDeltaTime);
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

            _tickScheduler = new TickScheduler();
            container.Register(_tickScheduler);

            // Persistence wiring. Manifest holds the save/load order; migrations registry
            // applies schema upgrades on load. SaveService composes both.
            _saveManifest = new SaveManifest();
            var migrations = new MigrationRegistry();
            container.Register(_saveManifest);
            container.Register(migrations);
            container.Register(new SaveService(_saveManifest, migrations));

            // Time-control state: serialize TickScheduler.Speed across save/load.
            _saveManifest.Register(new TimeControlSaveState(_tickScheduler));

            return container;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != _mainSceneName) return;

            // Inject service references into UI components that opt in by being on the scene.
            // Reaching into the UI layer from App is fine — App is at the top of the layer stack.
            var panel = FindAnyObjectByType<TimeControlPanel>(FindObjectsInactive.Include);
            if (panel != null && _tickScheduler != null)
            {
                panel.Bind(_tickScheduler);
            }

            // Auto-register scene-resident ISaveable MonoBehaviours so save/load picks them up
            // without per-instance boilerplate. Phase 1 has CameraSaveState; later phases extend.
            if (_saveManifest != null)
            {
                var camera = FindAnyObjectByType<CameraSaveState>(FindObjectsInactive.Include);
                if (camera != null && _saveManifest.Find(camera.SubsystemId) == null)
                {
                    _saveManifest.Register(camera);
                }
            }
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
