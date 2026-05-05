#nullable enable

using System;
using UnityEngine;

namespace CityRise.Core;

/// <summary>
/// Per-phase feature flags for incomplete systems. All flags default off so a fresh clone runs
/// the empty-world Bootstrap without firing into half-built systems. Runtime-mutable via the debug
/// console (Tech Roadmap section 2). Flip flags during a session — listeners react via <see cref="Changed"/>.
/// </summary>
[CreateAssetMenu(fileName = "FeatureFlags", menuName = "CityRise/Core/Feature Flags", order = 2)]
public sealed class FeatureFlags : ScriptableObject
{
    [Header("Phase 1 — core framework")]
    [SerializeField] private bool _replayRecorderEnabled;
    [SerializeField] private bool _integrityCheckerEnabled;

    [Header("Phase 6+ — gameplay systems")]
    [SerializeField] private bool _growthEnabled;
    [SerializeField] private bool _budgetEnabled;
    [SerializeField] private bool _coverageEnabled;
    [SerializeField] private bool _ordinancesEnabled;
    [SerializeField] private bool _powerEnabled;
    [SerializeField] private bool _waterEnabled;

    [Header("Post-MVP")]
    [SerializeField] private bool _agentsEnabled;
    [SerializeField] private bool _disastersEnabled;

    /// <summary>Fired when any flag changes. Subscribers re-read all relevant flags.</summary>
    public event Action? Changed;

    public bool ReplayRecorderEnabled    { get => _replayRecorderEnabled;    set => SetFlag(ref _replayRecorderEnabled, value); }
    public bool IntegrityCheckerEnabled  { get => _integrityCheckerEnabled;  set => SetFlag(ref _integrityCheckerEnabled, value); }
    public bool GrowthEnabled            { get => _growthEnabled;            set => SetFlag(ref _growthEnabled, value); }
    public bool BudgetEnabled            { get => _budgetEnabled;            set => SetFlag(ref _budgetEnabled, value); }
    public bool CoverageEnabled          { get => _coverageEnabled;          set => SetFlag(ref _coverageEnabled, value); }
    public bool OrdinancesEnabled        { get => _ordinancesEnabled;        set => SetFlag(ref _ordinancesEnabled, value); }
    public bool PowerEnabled             { get => _powerEnabled;             set => SetFlag(ref _powerEnabled, value); }
    public bool WaterEnabled             { get => _waterEnabled;             set => SetFlag(ref _waterEnabled, value); }
    public bool AgentsEnabled            { get => _agentsEnabled;            set => SetFlag(ref _agentsEnabled, value); }
    public bool DisastersEnabled         { get => _disastersEnabled;         set => SetFlag(ref _disastersEnabled, value); }

    private void SetFlag(ref bool field, bool value)
    {
        if (field == value) return;
        field = value;
        Changed?.Invoke();
    }

    private void OnValidate() => Changed?.Invoke();
}
