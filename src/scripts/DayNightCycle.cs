namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using UnitsNet;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class DayNightCycle : Node {
  #region properties

  [Export]
  public float HorizonDissipationFactor { get; set; } = 1.0f;

  private GameTimeService GameTime => World.GameTimeService;

  [Export]
  private DirectionalLight3D Moon { get; set; }

  [Export]
  private Curve MoonIntensityCycleProgress { get; set; }

  [Export]
  private float MoonTemperature { get; set; } = 4100;

  private Camera3D PlayerCamera => World.PlayerCamera;

  [Export]
  private float ShiftTimeZoneHours { get; set; } = 6.0f;

  [Export]
  private DirectionalLight3D Sun { get; set; }

  [Export]
  private float SunTemperature { get; set; } = 5778;

  [Export]
  private Curve SunTemperatureFactorByVerticalPosition { get; set; }

  [Export]
  private float SunTemperatureSunriseAndSunset { get; set; } = 1850;

  [Dependency]
  private GameWorld World => this.DependOn<GameWorld>();

  private WorldEnvironment WorldEnvironment => World.WorldEnvironment;
  #endregion

  public override void _Notification(int what) => this.Notify(what);

  public override void _Process(double delta) {
    var dayPct = GameTime.DayPctProgress;
    var moonCyclePct = GameTime.MoonCyclePctProgress;

    var currentTime = Duration.FromDays(GameTime.Now.TotalDays) + Duration.FromHours(ShiftTimeZoneHours);
    var earthRotationBasis = OrbitUtility.GetSurfaceRotationalBasis(OrbitUtility.EarthParameters, Angle.FromDegrees(360.0 * dayPct), Angle.FromDegrees(3.0));
    var moonRotationBasis = OrbitUtility.GetSurfaceRotationalBasis(OrbitUtility.MoonParameters, Angle.FromDegrees(360.0 * dayPct), Angle.FromDegrees(3.0));

    Sun.FaceFromSkyBodyToSurface(currentTime, OrbitUtility.EarthParameters.RelativeToPrimaryBody, OrbitUtility.SunParameters, earthRotationBasis);
    Sun.LightEnergy = Mathf.Clamp(Sun.GlobalTransform.Basis.Z.Y * HorizonDissipationFactor, 0.0f, 1.0f);
    Sun.Visible = Sun.LightEnergy > 0.0f;
    if (Sun.Visible) {
      Sun.LightTemperature = Mathf.Lerp(SunTemperatureSunriseAndSunset, SunTemperature, SunTemperatureFactorByVerticalPosition.SampleBaked(Sun.LightEnergy));
    }

    Moon.FaceFromSkyBodyToSurface(currentTime, OrbitUtility.MoonParameters.RelativeToPrimaryBody, OrbitUtility.EarthParameters, moonRotationBasis);
    Moon.Visible = Moon.GlobalTransform.Basis.Z.Y > 0.0f;
    if (!Moon.Visible) {
      return;
    }
    Moon.LightEnergy = 1.0f - Mathf.Clamp(Sun.LightEnergy, 0.0f, 1.0f);
    Moon.LightTemperature = MoonTemperature;
  }
}
