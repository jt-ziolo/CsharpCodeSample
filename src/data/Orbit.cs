namespace MyGameName;

/// <summary>
/// See: https://orbital-mechanics.space/classical-orbital-elements/classical-orbital-elements.html
/// </summary>
public readonly struct Orbit {
  public readonly UnitsNet.Angle ArgumentOfPeriapsis { get; init; }
  public readonly double Eccentricity { get; init; }
  public readonly UnitsNet.Angle Inclination { get; init; }
  public readonly double LnSpecificAngularMomentumSqKmPerSec { get; init; }
  public readonly UnitsNet.Duration Period { get; init; }
  public readonly UnitsNet.Angle RightAscensionOfAscendingNode { get; init; }
}
