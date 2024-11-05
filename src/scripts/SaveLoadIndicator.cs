namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using GTweens.Tweens;
using GTweensGodot.Extensions;

[Meta(typeof(IAutoNode))]
public partial class SaveLoadIndicator : TextureRect {
  private const float FADE_IN_DURATION = 1.0f;
  private const float FADE_OUT_DURATION = 2.0f;

  [Dependency]
  public UserInterface UserInterface => this.DependOn<UserInterface>();

  private bool _isShowing = true;
  private GTween _tween;

  public override void _Notification(int what) => this.Notify(what);

  public override void _Process(double delta) {
    if (UserInterface.ShowSaveLoadIndicator == _isShowing) {
      return;
    }
    if (!UserInterface.ShowSaveLoadIndicator) {
      _isShowing = false;
    }
    else {
      _isShowing = true;
    }
    if (_tween != null && !_tween.IsCompletedOrKilled) {
      _tween.Kill();
    }
    _tween = this.TweenModulateAlpha(_isShowing ? 1.0f : 0.0f, _isShowing ? FADE_IN_DURATION : FADE_OUT_DURATION);
    _tween.PlayUnpausable();
  }
}
