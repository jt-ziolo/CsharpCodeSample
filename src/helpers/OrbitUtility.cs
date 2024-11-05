namespace MyGameName;
using Godot;
using System;
using UnitsNet;

public static class OrbitUtility {
  #region properties

  /// <summary>
  /// https://orbital-mechanics.space/reference/planetary-parameters.html#sec-planetary-parameters
  /// Radii equate to the sea level
  /// </summary>
  public static PlanetaryParameterSet EarthParameters { get; } = new PlanetaryParameterSet {
    EquatorialRadius = Length.FromKilometers(6378.1366),
    LnGravitationalParameterCuKmPerSqSec = 12.8957136867608313962081942413,
    MeanRadius = Length.FromKilometers(6371.0084),
    PolarRadius = Length.FromKilometers(6356.7519),
    RelativeToPrimaryBody = new Orbit {
      // AI gave me this so it's probably wrong
      LnSpecificAngularMomentumSqKmPerSec = 64.129892294886503205797232354307,
      ArgumentOfPeriapsis = Angle.FromDegrees(90.0),
      Eccentricity = 0.01670,
      Inclination = Angle.FromRadians(0.0),
      RightAscensionOfAscendingNode = Angle.FromDegrees(100.47),
      Period = Duration.FromDays(365.26)
    }
  };

  /// <summary>
  /// https://orbital-mechanics.space/reference/planetary-parameters.html#sec-planetary-parameters
  /// https://en.wikipedia.org/wiki/Orbit_of_the_Moon
  /// https://physics.stackexchange.com/questions/286540/does-the-moons-argument-of-periapsis-and-right-ascension-of-the-ascending-node
  /// </summary>
  public static PlanetaryParameterSet MoonParameters { get; } = new PlanetaryParameterSet {
    LnGravitationalParameterCuKmPerSqSec = 8.4975617494670100909399624646017,
    MeanRadius = Length.FromKilometers(1737.4),
    PolarRadius = Length.FromKilometers(1737.4),
    EquatorialRadius = Length.FromKilometers(1737.4),
    RelativeToPrimaryBody = new Orbit {
      // AI gave me this so it's probably wrong
      LnSpecificAngularMomentumSqKmPerSec = 12.881564890851398287598643612145,
      ArgumentOfPeriapsis = Angle.FromDegrees(318.15),
      // Just going to use the mean values for eccentricity and inclination
      Eccentricity = 0.0549,
      Inclination = Angle.FromDegrees(5.15),
      RightAscensionOfAscendingNode = Angle.FromDegrees(125.08),
      Period = Duration.FromDays(27.322)
    }
  };

  /// <summary>
  /// https://orbital-mechanics.space/reference/planetary-parameters.html#sec-planetary-parameters
  /// </summary>
  public static PlanetaryParameterSet SunParameters { get; } = new PlanetaryParameterSet {
    EquatorialRadius = Length.FromKilometers(695700.0),
    LnGravitationalParameterCuKmPerSqSec = 25.611447203736385083182879540767,
  };

  #endregion

  #region methods

  // public static void FaceFromSkyBodyToSurface(this DirectionalLight3D light, Duration timeSincePeriapsis, Orbit orbit, PlanetaryParameterSet orbitingAround, Basis surfaceRotationBasis) {
  //   var directionFacingSkyBody = GetTopocentricDirectionAtTime(timeSincePeriapsis, orbit, orbitingAround);
  //   directionFacingSkyBody = surfaceRotationBasis * directionFacingSkyBody;
  //   var transform = light.Transform;
  //   transform = transform.LookingAt(-directionFacingSkyBody).Orthonormalized();
  //   light.Transform = transform;
  // }

  public static void FaceFromSkyBodyToSurface(this DirectionalLight3D light, Duration timeSincePeriapsis, Orbit orbit, PlanetaryParameterSet orbitingAround, Basis surfaceRotationBasis) {
    var directionFacingSkyBody = GetTopocentricDirectionAtTime(timeSincePeriapsis, orbit, orbitingAround);
    directionFacingSkyBody = surfaceRotationBasis * directionFacingSkyBody;
    var transform = light.Transform;
    transform = transform.LookingAt(-directionFacingSkyBody).Orthonormalized();
    light.Transform = transform;
  }

  public static Vector3 GetInertialDirection(Vector2Double perifocalDirection, Orbit orbit) {
    // Inertial Direction (ECI)
    // https://orbital-mechanics.space/intro/reference-frames.html
    // +X (along intersection of equatorial and ecliptic planes, at ECI == ECEF this
    // is at the intersection of the equator and prime meridian)
    // +Y (defined by right hand rule, X/Z)
    // +Z (North Pole)
    var basis = GetPerifocalToInertialBasis(orbit);
    var result = basis * new Vector3((float)perifocalDirection.X, (float)perifocalDirection.Y, 0.0f);
    return result;
  }

  public static double GetLnPerifocalDistanceKm(Orbit orbit, PlanetaryParameterSet orbitingAround, Angle trueAnomaly) {
    var tRad = trueAnomaly.As(UnitsNet.Units.AngleUnit.Radian);
    var cosTrueAnomaly = Mathf.Cos(tRad);
    var sinTrueAnomaly = Mathf.Sin(tRad);
    var lnDistanceKm = (2.0 * orbit.LnSpecificAngularMomentumSqKmPerSec) - orbitingAround.LnGravitationalParameterCuKmPerSqSec - Mathf.Log(1.0 + (orbit.Eccentricity * cosTrueAnomaly));
    return lnDistanceKm;
  }

  public static Vector2Double GetPerifocalDirection(Orbit orbit, PlanetaryParameterSet orbitingAround, UnitsNet.Angle trueAnomaly) {
    var tRad = trueAnomaly.As(UnitsNet.Units.AngleUnit.Radian);
    var cosTrueAnomaly = Mathf.Cos(tRad);
    var sinTrueAnomaly = Mathf.Sin(tRad);
    return new Vector2Double(cosTrueAnomaly, sinTrueAnomaly);
  }

  public static Vector3 GetTopocentricDirection(Vector2Double perifocalDirection, Orbit orbit) {
    // Topocentric Direction
    // https://orbital-mechanics.space/intro/reference-frames.html
    // +X (East)
    // +Y (Zenith)
    // -Z (North/Latitude)
    //
    // We assume that ECI == ECEF (Earth/planet hasn't rotated) for the given
    // perifocal direction, and that we are standing on the surface at 0N,
    // 90W/270E. The planet's radius is assumed to be very small (wait until
    // after this transformation and planetary rotation to account for terrain
    // height etc.).
    //
    // We need East/North/Zenith to align with the definitions above:
    // - We need to reverse inertial Y to yield +Y/Zenith
    // - We need to reverse inertial Z to yield -Z/North
    // - X remains East
    // I chose to do it this way to preserve X.
    return GetInertialDirection(perifocalDirection, orbit) * new Vector3(1.0f, -1.0f, -1.0f);
  }

  public static Vector3 GetTopocentricDirectionAtTime(Duration timeSincePeriapsis, Orbit orbit, PlanetaryParameterSet orbitingAround) {
    var trueAnomaly = GetTrueAnomalyCircularOrbit(timeSincePeriapsis, orbit);
    var perifocalDirection = GetPerifocalDirection(orbit, orbitingAround, trueAnomaly);
    var direction = GetTopocentricDirection(perifocalDirection, orbit);
    return direction;
  }

  public static Angle GetTrueAnomalyCircularOrbit(Duration timeSincePeriapsis, Orbit orbit) {
    var ratio = timeSincePeriapsis.As(UnitsNet.Units.DurationUnit.Day) / orbit.Period.As(UnitsNet.Units.DurationUnit.Day);
    var trueAnomaly = Angle.FromRadians(2.0 * Math.PI) * (ratio % 1.0);
    return trueAnomaly;
  }

  /// <summary>
  /// See: https://orbital-mechanics.space/classical-orbital-elements/orbital-elements-and-the-state-vector.html#orbital-elements-state-vector
  /// </summary>
  /// <param name="orbit"></param>
  /// <param name="orbitingAround"></param>
  /// <returns>Distance unit is kilometer</returns>
  internal static Basis GetPerifocalToInertialBasis(Orbit orbit) {
    // Basis is a convenient Godot struct for rotations
    // order of rotations is +Z, +X, +Z
    // R1 = [cos(-omega) - sin(-omega) 0; sin(-omega) cos(-omega) 0; 0 0 1];
    var r1 = Basis.Identity.Rotated(Vector3.Back, (float)orbit.ArgumentOfPeriapsis.As(UnitsNet.Units.AngleUnit.Radian));
    // R2 = [1 0 0; 0 cos(-i) - sin(-i); 0 sin(-i) cos(-i)];
    var r2 = Basis.Identity.Rotated(Vector3.Right, (float)orbit.Inclination.As(UnitsNet.Units.AngleUnit.Radian));
    // R3 = [cos(-Omega) - sin(-Omega) 0; sin(-Omega) cos(-Omega) 0; 0 0 1];
    var r3 = Basis.Identity.Rotated(Vector3.Back, (float)orbit.RightAscensionOfAscendingNode.As(UnitsNet.Units.AngleUnit.Radian));
    var basis = r3 * r2 * r1;
    return basis;
  }

  internal static Basis GetSurfaceRotationalBasis(PlanetaryParameterSet parameters, Angle longitude, Angle latitude) {
    var basis = Basis.Identity
      .Rotated(Vector3.Right, (float)parameters.Obliquity.As(UnitsNet.Units.AngleUnit.Radian))
      .Rotated(Vector3.Forward, (float)-longitude.As(UnitsNet.Units.AngleUnit.Radian))
      .Rotated(Vector3.Right, (float)latitude.As(UnitsNet.Units.AngleUnit.Radian));
    return basis;
  }

  #endregion
}
