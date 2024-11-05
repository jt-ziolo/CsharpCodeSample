namespace MyGameName;
using Arch.Core;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta]
public partial class GameWorld : Node3D {
  #region properties
  public World EcsWorld { get; private set; }
  public GameTimeService GameTimeService => this.GetFirstChildOfType<GameTimeService>();
  public PlayerCharacterBody3D Player => this.GetFirstChildOfType<PlayerCharacterBody3D>();
  public Camera3D PlayerCamera => Player.Camera;
  public CameraLightLevelDetection PlayerLightDetection => this.GetFirstChildOfType<CameraLightLevelDetection>();
  public WorldEnvironment WorldEnvironment => this.GetFirstChildOfType<WorldEnvironment>();
  #endregion

  #region methods

  public override void _EnterTree() {
    if (EcsWorld != null) {
      return;
    }
    EcsWorld = World.Create();
  }

  public override void _ExitTree() {
    EcsWorld?.Dispose();
  }

  public override void _Notification(int what) => this.Notify(what);
  #endregion
}
