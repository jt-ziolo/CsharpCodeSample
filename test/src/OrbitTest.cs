namespace MyGameName;

using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using GodotTestDriver;
using UnitsNet;
using Shouldly;
using Vector3 = Godot.Vector3;
using Chickensoft.GoDotLog;

public class OrbitUtilsTests : TestClass {
  // Tests based on https://orbital-mechanics.space/classical-orbital-elements/orbital-elements-and-the-state-vector.html
  private readonly ILog _log = new GDLog(nameof(OrbitUtilsTests));
  private Fixture _fixture = default!;

  public OrbitUtilsTests(Node testScene) : base(testScene) { }

  [SetupAll]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
  }

  [CleanupAll]
  public void Cleanup() => _fixture.Cleanup();

  [Test]
  public void TestCalculatePerifocalDirectionAndDistance() {
    var orbit = new Orbit {
      ArgumentOfPeriapsis = Angle.FromDegrees(303.09),
      Eccentricity = 0.948,
      Inclination = Angle.FromDegrees(124.05),
      LnSpecificAngularMomentumSqKmPerSec = Mathf.Log(19646.883),
      RightAscensionOfAscendingNode = Angle.FromDegrees(190.62),
    };
    var orbitingAround = new PlanetaryParameterSet {
      LnGravitationalParameterCuKmPerSqSec = Mathf.Log(3.986e5)
    };
    var trueAnomaly = Angle.FromDegrees(159.61);
    var expectedPerifocalPosition = new Vector2Double(-8117.7120, 3017.0767);
    var expectedPerifocalDistance = expectedPerifocalPosition.Length();
    var expectedLnPerifocalDistanceKm = Mathf.Log(expectedPerifocalDistance);
    var expectedPerifocalDirection = expectedPerifocalPosition.Normalized();

    var tolerance = 0.01;

    var perifocalDirection = OrbitUtility.GetPerifocalDirection(orbit, orbitingAround, trueAnomaly);
    perifocalDirection.X.ShouldBe(expectedPerifocalDirection.X, tolerance);
    perifocalDirection.Y.ShouldBe(expectedPerifocalDirection.Y, tolerance);
    var lnPerifocalDistanceKm = OrbitUtility.GetLnPerifocalDistanceKm(orbit, orbitingAround, trueAnomaly);
    lnPerifocalDistanceKm.ShouldBe(expectedLnPerifocalDistanceKm, tolerance);
  }

  [Test]
  public void TestConvertPerifocalToInertialFrame() {
    var orbit = new Orbit {
      ArgumentOfPeriapsis = Angle.FromDegrees(303.09),
      Eccentricity = 0.948,
      Inclination = Angle.FromDegrees(124.05),
      LnSpecificAngularMomentumSqKmPerSec = Mathf.Log(19646.883),
      RightAscensionOfAscendingNode = Angle.FromDegrees(190.62),
    };
    var orbitingAround = new PlanetaryParameterSet {
      LnGravitationalParameterCuKmPerSqSec = Mathf.Log(3.986e5)
    };
    var trueAnomaly = Angle.FromDegrees(159.61);
    var expectedInertialPosition = new Vector3Double(1000.0, 5000.0, 7000.0);
    var expectedInertialDirection = expectedInertialPosition.Normalized();
    var perifocalPosition = new Vector2Double(-8117.7120, 3017.0767);
    var perifocalDistance = perifocalPosition.Length();
    var perifocalDirection = perifocalPosition.Normalized();

    var argP = (float)orbit.ArgumentOfPeriapsis.As(UnitsNet.Units.AngleUnit.Radian);
    var i = (float)orbit.Inclination.As(UnitsNet.Units.AngleUnit.Radian);
    var rAsc = (float)orbit.RightAscensionOfAscendingNode.As(UnitsNet.Units.AngleUnit.Radian);
    var expectedR1 = new Basis(
      new Vector3(Mathf.Cos(-argP), -Mathf.Sin(-argP), 0.0f),
      new Vector3(Mathf.Sin(-argP), Mathf.Cos(-argP), 0.0f),
      Vector3.Back);
    var expectedR2 = new Basis(
      Vector3.Right,
      new Vector3(0.0f, Mathf.Cos(-i), -Mathf.Sin(-i)),
      new Vector3(0.0f, Mathf.Sin(-i), Mathf.Cos(-i)));
    var expectedR3 = new Basis(
      new Vector3(Mathf.Cos(-rAsc), -Mathf.Sin(-rAsc), 0.0f),
      new Vector3(Mathf.Sin(-rAsc), Mathf.Cos(-rAsc), 0.0f),
      Vector3.Back);

    var tolerance = 0.01f;
    var r1 = Basis.Identity.Rotated(Vector3.Back, (float)orbit.ArgumentOfPeriapsis.As(UnitsNet.Units.AngleUnit.Radian));
    matrixComponentsShouldBeInTolerance(expectedR1, tolerance, r1, "R1");
    // R1 = [cos(-omega) - sin(-omega) 0; sin(-omega) cos(-omega) 0; 0 0 1];
    var r2 = Basis.Identity.Rotated(Vector3.Right, (float)orbit.Inclination.As(UnitsNet.Units.AngleUnit.Radian));
    matrixComponentsShouldBeInTolerance(expectedR2, tolerance, r2, "R2");
    // R2 = [1 0 0; 0 cos(-i) - sin(-i); 0 sin(-i) cos(-i)];
    var r3 = Basis.Identity.Rotated(Vector3.Back, (float)orbit.RightAscensionOfAscendingNode.As(UnitsNet.Units.AngleUnit.Radian));
    matrixComponentsShouldBeInTolerance(expectedR3, tolerance, r3, "R3");
    // R3 = [cos(-Omega) - sin(-Omega) 0; sin(-Omega) cos(-Omega) 0; 0 0 1];

    var expectedBasis = r3 * r2 * r1;
    var basis = OrbitUtility.GetPerifocalToInertialBasis(orbit);
    matrixComponentsShouldBeInTolerance(expectedBasis, tolerance, basis, "basis");

    var inertialDirection = OrbitUtility.GetInertialDirection(perifocalDirection, orbit);
    inertialDirection.X.ShouldBe((float)expectedInertialDirection.X, tolerance);
    inertialDirection.Y.ShouldBe((float)expectedInertialDirection.Y, tolerance);
    inertialDirection.Z.ShouldBe((float)expectedInertialDirection.Z, tolerance);

    void matrixComponentsShouldBeInTolerance(Basis expectedBasis, float tolerance, Basis basis, string msg) {
      basis.X.X.ShouldBe(expectedBasis.X.X, tolerance, msg);
      basis.X.Y.ShouldBe(expectedBasis.X.Y, tolerance, msg);
      basis.X.Z.ShouldBe(expectedBasis.X.Z, tolerance, msg);
      basis.Y.X.ShouldBe(expectedBasis.Y.X, tolerance, msg);
      basis.Y.Y.ShouldBe(expectedBasis.Y.Y, tolerance, msg);
      basis.Y.Z.ShouldBe(expectedBasis.Y.Z, tolerance, msg);
      basis.Z.X.ShouldBe(expectedBasis.Z.X, tolerance, msg);
      basis.Z.Y.ShouldBe(expectedBasis.Z.Y, tolerance, msg);
      basis.Z.Z.ShouldBe(expectedBasis.Z.Z, tolerance, msg);
    }
  }
}
