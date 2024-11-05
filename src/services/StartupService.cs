namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using MyGameName.Exceptions;
using MyGameName.ValueObjects.Names;
using System.Threading.Tasks;

[Meta(typeof(IAutoNode))]
public partial class StartupService : Node {
  #region properties

  [Dependency]
  private ConfigService ConfigHandler => this.DependOn<ConfigService>();

  [Dependency]
  private ModLoadingService ModLoader => this.DependOn<ModLoadingService>();

  [Dependency]
  private PauseService PauseService => this.DependOn<PauseService>();

  [Dependency]
  private SaveGameService SaveGameService => this.DependOn<SaveGameService>();

  [Dependency]
  private UserInterface UserInterface => this.DependOn<UserInterface>();

  #endregion

  internal GameSession? _currentGameSession;

  public override void _Notification(int what) => this.Notify(what);

  public void Initialize() {
    _currentGameSession = new GameSession((WorldName)"Example World", (CharacterName)"Example Character");
  }

  public async void OnResolved() {
    Logger.Log(this, Logger.LogColorByPurpose.Default, "Game starting...");
    UserInterface.ShowProcessingIndicator = true;
    GetViewport().GetWindow().Disable3D = true;
    try {
      ConfigHandler.LoadFromConfigFile();
    }
    catch (ConfigLoadException error) {
      Logger.LogWarning(Name, error.Message);
    }
    ConfigHandler.ApplyConfig();
    // Save the config, in case we hadn't generated config yet
    ConfigHandler.SaveConfig();
    ModLoader.LoadModsStartup();

    UserInterface.CoreInterfaceScreensState.Start();
    await Task.Delay(1000);
    await SaveGameService.LoadGameDataAsync();
    UserInterface.ShowProcessingIndicator = false;

    GetViewport().GetWindow().Disable3D = false;

    // for fun
    var queue = new RandomOutcomeQueue<string>();
    queue.AddOutcome("Gold", new RandomOutcomeQueue<string>.OutcomeParams { Weight = 2, MaxTriesBeforeForced = 5 });
    queue.AddOutcome("Lead", new RandomOutcomeQueue<string>.OutcomeParams { Weight = 4, MaxTriesBeforeForced = 7 });
    queue.AddOutcome("Silver Ore", new RandomOutcomeQueue<string>.OutcomeParams { Weight = 4, MaxTriesBeforeForced = 8 });
    for (int i = 0; i < 20; i++) {
      Logger.Log($"It was {queue.GetNext()}");
    }
  }
}
