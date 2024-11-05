namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using GTweensGodot.Extensions;
using MyGameName.ValueObjects.Time;
using System.Threading.Tasks;

[Meta(typeof(IAutoNode))]
public partial class PauseMenu : Control {
  private const double BLUR_LOD = 2.0;
  private const double FADE_DURATION = 0.25f;

  #region properties

  [Node("QuitToDesktopButton")]
  public Button QuitToDesktopButton { get; set; }

  [Node("QuitToMainMenuButton")]
  public Button QuitToMainMenuButton { get; set; }

  [Node("ResumeButton")]
  public Button ResumeButton { get; set; }

  [Node("SettingsButton")]
  public Button SettingsButton { get; set; }

  [Dependency]
  private UserInterface UserInterface => this.DependOn<UserInterface>();

  [Dependency]
  private GameWorld WorldProvider => this.DependOn<GameWorld>();

  #endregion

  #region methods

  public override void _Input(InputEvent @event) {
    if (@event.IsActionPressed("toggle_pause")) {
      ToggleMenu();
    }
  }

  public override void _Notification(int what) => this.Notify(what);

  public void OnResolved() {
    ProcessMode = ProcessModeEnum.Always;

    // Set up buttons
    ResumeButton.Pressed += OnResumeButtonPressed;
    SettingsButton.Pressed += OnSettingsButtonPressed;
    QuitToDesktopButton.Pressed += OnQuitToDesktopButtonPressed;
    QuitToMainMenuButton.Pressed += OnQuitToMainMenuButtonPressedAsync;

    SettingsButton.Disabled = true;
  }

  public void OpenMenu() {
    Modulate = Color.Color8(255, 255, 255, 0);
    Visible = true;
    AnimateEntrance();
  }

  private void AnimateEntrance() {
    this.SetFlagFromThisNode(PauseService.PAUSE_FLAG);
    this.TweenModulateAlpha(1.0f, (float)FADE_DURATION).PlayUnpausable();
    // WorldProvider.PostProcessWrapper.SetBlur(true, BLUR_LOD, (Duration)FADE_DURATION);
    Input.MouseMode = Input.MouseModeEnum.Confined;
    ResumeButton.GrabFocus();
  }

  private async Task AnimateExitAsync() {
    this.TweenModulateAlpha(0.0f, (float)FADE_DURATION).PlayUnpausable();
    // WorldProvider.PostProcessWrapper.SetBlur(false, BLUR_LOD, (Duration)FADE_DURATION);
    await this.TimerFromSelf((Duration)FADE_DURATION);
    Visible = false;
  }

  private async Task CloseMenuAsync(bool setMouseMode = true) {
    await AnimateExitAsync();
    if (setMouseMode) {
      Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    this.UnsetFlagFromThisNode(PauseService.PAUSE_FLAG);
  }

  private void OnQuitButtonPressed() {
    GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
  }

  private void OnQuitToDesktopButtonPressed() {
    // Show a pop-up asking if the player is sure
    // TODO: make a helper function for this
    GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
  }

  private async void OnQuitToMainMenuButtonPressedAsync() {
    UserInterface.MainMenu.AnimateEntrance();
    await CloseMenuAsync(false);
    this.UnsetFlagFromThisNode(PauseService.PAUSE_FLAG);
  }

  private void OnResumeButtonPressed() {
    CloseMenuAsync();
  }

  private void OnSettingsButtonPressed() {
    throw new System.NotImplementedException();
  }

  private void ToggleMenu() {
    if (!Visible) {
      OpenMenu();
    }
    else {
      CloseMenuAsync();
    }
  }

  #endregion
}
