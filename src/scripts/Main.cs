namespace MyGameName;
using Chickensoft.GoDotTest;
using Godot;
using System.Reflection;

// This entry-point file is responsible for determining if we should run tests.
//
// If you want to edit your game's main entry-point, please see Game.tscn and
// Game.cs instead.

public partial class Main : Node2D {
  public const string GAME_SCENE_PATH = "res://assets/scenes/Game.tscn";

#if DEBUG
  public TestEnvironment Environment = default!;
#endif

  #region methods

  public override void _Ready() {
    ApplicationUtility.StoreApplicationInfo();
#if DEBUG
    // If this is a debug build, use GoDotTest to examine the
    // command line arguments and determine if we should run tests.
    Environment = TestEnvironment.From(OS.GetCmdlineArgs());
    if (Environment.ShouldRunTests) {
      CallDeferred("RunTests");
      return;
    }
#endif

    // If we don't need to run tests, we can just switch to the game scene.
    CallDeferred("RunScene");
  }

  private void RunScene()
    => GetTree().ChangeSceneToFile(GAME_SCENE_PATH);

#if DEBUG
  private void RunTests()
    => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
#endif

  #endregion
}
