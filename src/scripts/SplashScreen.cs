namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using GTweens.Builders;
using GTweens.Tweens;
using GTweensGodot.Extensions;
using System;
using System.Threading;

[Meta(typeof(IAutoNode))]
public partial class SplashScreen : Control {
  [Dependency]
  private UserInterface UserInterface => this.DependOn<UserInterface>();

  #region fields
  private Chickensoft.LogicBlocks.LogicBlock<CoreInterfaceScreenState.State>.IBinding _binding;
  private CancellationTokenSource _ctsSplash;
  private bool _isListeningForInput;
  #endregion

  #region methods

  public override void _ExitTree() {
    _binding.Dispose();
    base._ExitTree();
  }

  public override void _Notification(int what) => this.Notify(what);

  public override void _Process(double delta) {
    if (!_isListeningForInput) {
      return;
    }
    if (Input.IsAnythingPressed()) {
      _isListeningForInput = false;
      _ctsSplash.Cancel();
      UserInterface.CoreInterfaceScreensState.Input(new CoreInterfaceScreenState.Input.AnyButtonPressed());
    }
  }

  public void OnResolved() {
    _binding = UserInterface.CoreInterfaceScreensState.Bind();
    _binding.Handle((in CoreInterfaceScreenState.Output.SplashScreen output) => {
      HandleSplashScreenOutputChanged(output);
    });
  }

  private void HandleSplashScreenOutputChanged(CoreInterfaceScreenState.Output.SplashScreen output) {
    GTween tween;
    if (output.IsVisible) {
      _isListeningForInput = true;
      Modulate = Color.Color8(255, 255, 255, 0);
      tween = GTweenSequenceBuilder.New()
        .AppendCallback(() => Visible = true)
        .Append(this.TweenModulateAlpha(1.0f, 0.5f))
        .Build();
      _ctsSplash = new();
      AsyncUtility.RunAfterWaitUnlessCancelledAsync(SplashTimeout, TimeSpan.FromSeconds(3.0), _ctsSplash);
      tween.PlayUnpausableAsync(_ctsSplash.Token);
      return;
    }
    tween = GTweenSequenceBuilder.New()
      .Append(this.TweenModulateAlpha(0.0f, 0.1f))
      .JoinTime(0.1f)
      .AppendCallback(() => Visible = false)
      .Build();
    tween.PlayUnpausable();
  }

  private void SplashTimeout() {
    if (_isListeningForInput) {
      _isListeningForInput = false;
      UserInterface.CoreInterfaceScreensState.Input(new CoreInterfaceScreenState.Input.Progressing());
    }
  }

  #endregion
}
