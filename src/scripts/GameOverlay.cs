namespace MyGameName;
using Godot;
using GTweensGodot.Extensions;
using MyGameName.ValueObjects.Time;

public partial class GameOverlay : ColorRect {
  [Export]
  public double FadeInDuration { get; set; }

  #region methods

  public override void _Ready() {
    ProcessMode = ProcessModeEnum.Always;
  }

  public Duration AnimateFadeIn() {
    this.TweenModulateAlpha(1.0f, 1.0f).SetEasing(GTweens.Easings.Easing.Linear).Play();
    return (Duration)2.0f;
  }

  public Duration AnimateFadeOut() {
    this.TweenModulateAlpha(0.0f, 1.0f).SetEasing(GTweens.Easings.Easing.Linear).Play();
    return (Duration)1.0f;
  }

  #endregion
}
