namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class UserInterface : Control {
  #region properties
  public CoreInterfaceScreenState CoreInterfaceScreensState { get; } = new();

  [Node]
  public Control DisclaimerScreen { get; set; } = default!;

  [Node]
  public Control HUD { get; set; } = default!;

  [Node]
  public Control LoadScreen { get; set; } = default!;

  [Node]
  public MainMenu MainMenu { get; set; } = default!;

  [Node]
  public GameOverlay Overlay { get; set; } = default!;

  [Node]
  public Control ProcessingIndicator { get; set; } = default!;

  [Node]
  public Control SaveLoadIndicator { get; set; } = default!;

  public bool ShowProcessingIndicator { get; set; }
  public bool ShowSaveLoadIndicator { get; set; }

  [Node]
  public Control SplashScreen { get; set; } = default!;

  #endregion

  public override void _Notification(int what) => this.Notify(what);
}
