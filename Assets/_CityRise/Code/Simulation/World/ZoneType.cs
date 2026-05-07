#nullable enable

namespace CityRise.Simulation.World;

/// <summary>
/// Tile zoning category. None means unzoned. Phase 5 introduces low-density R/C/I; medium and
/// high densities are unlocked via the population-tier unlock tree (Phase 6+). Byte-backed.
/// </summary>
public enum ZoneType : byte
{
    None = 0,
    ResidentialLow = 1,
    CommercialLow = 2,
    IndustrialLow = 3,
    ResidentialMedium = 4,
    CommercialMedium = 5,
    IndustrialMedium = 6,
    ResidentialHigh = 7,
    CommercialHigh = 8,
    IndustrialHigh = 9,
}
