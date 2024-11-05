namespace MyGameName;
using Godot;

public partial class PauseService : Node {
  public const string PAUSE_FLAG = "PauseFlag";
  private const string SHADER_PARAM_NAME = "is_paused";

  private bool _wasPausedLastFrame;

  public override void _Process(double delta) {
    var isPaused = FlagState.GetAnyFlagsSet(PAUSE_FLAG);
    if ((_wasPausedLastFrame && isPaused) || (!_wasPausedLastFrame && !isPaused)) {
      // Setting the global parameter value is expensive, so don't do it if it isn't necessary
      return;
    }
    GetTree().Paused = isPaused;
    RenderingServer.GlobalShaderParameterSetOverride(SHADER_PARAM_NAME, isPaused);
    _wasPausedLastFrame = isPaused;
  }

  public override void _Ready() {
    ProcessMode = ProcessModeEnum.Always;
  }
}
