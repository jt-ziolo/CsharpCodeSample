namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using GTweensGodot.Extensions;
using MyGameName.ValueObjects.Time;
using System;
using System.Threading.Tasks;

[Meta(typeof(IAutoNode))]
public partial class MainMenu : Control {
  #region properties

  [Node("LoadButton")]
  public Button LoadButton { get; set; }

  [Node("QuitButton")]
  public Button QuitButton { get; set; }

  [Node("SettingsButton")]
  public Button SettingsButton { get; set; }

  [Node("StartButton")]
  public Button StartButton { get; set; }

  [Export]
  public double StartAnimDelay { get; set; } = 0.25;

  [Dependency]
  private UserInterface UserInterface => this.DependOn<UserInterface>();

  [Dependency]
  private GameWorld WorldProvider => this.DependOn<GameWorld>();

  #endregion

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public void AnimateEntrance() {
    Visible = true;
    this.TweenModulateAlpha(1.0f, 0.5f).PlayUnpausable();
    Input.MouseMode = Input.MouseModeEnum.Confined;
    WorldProvider.ProcessMode = ProcessModeEnum.Disabled;
    WorldProvider.Visible = false;
    UserInterface.Overlay.AnimateFadeIn();
  }

  public async Task AnimateExitAsync(bool animateOverlay = true) {
    this.TweenModulateAlpha(0.0f, 0.5f).PlayUnpausable();
    WorldProvider.ProcessMode = ProcessModeEnum.Inherit;
    WorldProvider.Visible = true;

    Duration duration = (Duration)0.5;
    if (animateOverlay) {
      duration = (Duration)Math.Max(UserInterface.Overlay.AnimateFadeOut().Value, 1.5);
    }

    await this.TimerFromSelf(duration);
    Visible = false;
    Input.MouseMode = Input.MouseModeEnum.Captured;
  }

  public void OnResolved() {
    // Set up buttons
    StartButton.Pressed += OnStartButtonPressed;
    LoadButton.Pressed += OnLoadButtonPressed;
    SettingsButton.Pressed += OnSettingsButtonPressed;
    QuitButton.Pressed += async () => await OnQuitButtonPressedAsync();

    LoadButton.Disabled = true;
    SettingsButton.Disabled = true;
  }

  private void OnLoadButtonPressed() {
    throw new System.NotImplementedException();
  }

  private async Task OnQuitButtonPressedAsync() {
    // await AnimateExitAsync(false);
    GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
  }

  private void OnSettingsButtonPressed() {
    throw new System.NotImplementedException();
  }

  private void OnStartButtonPressed() {
    AnimateExitAsync();
  }

  #endregion
}
