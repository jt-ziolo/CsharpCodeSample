namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class NightVision : OmniLight3D {
  [Dependency]
  public GameWorld WorldProvider => this.DependOn<GameWorld>();

  #region fields

  [Export]
  public Curve EffectStrengthCurve;

  [Export(PropertyHint.Range, "1.0,100.0")]
  public float MaxBlueLightRange = 4.0f;

  [Export]
  public float MaxFilmGrainAmount = 50.0f;

  [Export(PropertyHint.Range, "1.0,2.0")]
  public float MaxGlowStrengthAmount = 1.5f;

  #endregion

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public void OnResolved() {
    WorldProvider.PlayerLightDetection.OnLightLevelChanged += OnLightLevelChanged;
  }

  private void OnLightLevelChanged(object? sender, float lightLevel) {
    var factor = EffectStrengthCurve.SampleBaked(lightLevel);
    LightEnergy = factor;
    OmniRange = factor * MaxBlueLightRange;
    // TODO
    // WorldProvider.PostProcessWrapper.SetFilmGrain(true, MaxFilmGrainAmount * factor);
    WorldProvider.WorldEnvironment.Environment.GlowStrength = 1.0f + (factor * (MaxGlowStrengthAmount - 1.0f));
  }

  #endregion
}
