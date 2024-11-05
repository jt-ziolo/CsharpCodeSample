namespace MyGameName;

using UnitsNet;

/// <summary>
/// See: https://orbital-mechanics.space/reference/planetary-parameters.html#sec-planetary-parameters
/// </summary>
public readonly struct PlanetaryParameterSet {
  public readonly Angle Obliquity { get; init; }
  public readonly Length EquatorialRadius { get; init; }
  public readonly double LnGravitationalParameterCuKmPerSqSec { get; init; }
  public readonly Length MeanRadius { get; init; }
  public readonly Length PolarRadius { get; init; }
  public readonly Orbit RelativeToPrimaryBody { get; init; }
}
