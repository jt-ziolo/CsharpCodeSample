namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using MyGameName.ValueObjects.Names;

[Meta(typeof(IAutoNode))]
public partial class Game : Node, IProvide<StartupService>, IProvide<ConfigService>, IProvide<ModLoadingService>, IProvide<SaveGameService>, IProvide<PauseService>, IProvide<GameSession>, IProvide<UserInterface>, IProvide<GameWorld>, IProvide<TemplateLabelLookupService> {
  #region properties
  private bool ApplicationIsQuitting { get; set; }
  private ConfigService ConfigService { get; set; }
  private GameSession GameSession { get; set; }

  [Node]
  private GameWorld GameWorld { get; set; }

  private ModLoadingService ModLoadingService { get; set; }
  private PauseService PauseService { get; set; }
  private SaveGameService SaveGameService { get; set; }
  private StartupService StartupService { get; set; }
  private TemplateLabelLookupService TemplateLabelLookupService { get; set; }

  [Node("CanvasLayer/UserInterface")]
  private UserInterface UserInterface { get; set; }

  #endregion

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public void Initialize() {
    Logger.Log(this, Logger.LogColorByPurpose.Application, "Initialize is running (not testing)");
    GetTree().AutoAcceptQuit = false;
    GameSession = new GameSession((WorldName)"Example World", (CharacterName)"Example Character");
    ConfigService = this.GetOrAddChildOfType<ConfigService>();
    ModLoadingService = this.GetOrAddChildOfType<ModLoadingService>();
    PauseService = this.GetOrAddChildOfType<PauseService>();
    SaveGameService = this.GetOrAddChildOfType<SaveGameService>();
    StartupService = this.GetOrAddChildOfType<StartupService>();
    TemplateLabelLookupService = this.GetOrAddChildOfType<TemplateLabelLookupService>();
  }

  public void OnExitTree() {
    // Quit
    if (!ApplicationIsQuitting) {
      Logger.LogError(this, "Game script's node was removed from the tree, quitting...");
      GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
    }
    base._ExitTree();
  }

  public void OnNotification(int what) {
    if (what == NotificationWMCloseRequest) {
      // Quitting
      ApplicationIsQuitting = true;
      Logger.Log(this, Logger.LogColorByPurpose.Application, "Received quit request!");
      GetViewport().GetWindow().Disable3D = true;
      SaveGameService.SaveGameDataAsync(true);
    }
    base._Notification(what);
  }

  public void OnReady() {
    Logger.Log(this, Logger.LogColorByPurpose.Default, "Injecting services...");
    this.Provide();
  }

  ConfigService IProvide<ConfigService>.Value() => ConfigService;

  GameSession IProvide<GameSession>.Value() => GameSession;

  GameWorld IProvide<GameWorld>.Value() => GameWorld;

  ModLoadingService IProvide<ModLoadingService>.Value() => ModLoadingService;

  PauseService IProvide<PauseService>.Value() => PauseService;

  SaveGameService IProvide<SaveGameService>.Value() => SaveGameService;

  StartupService IProvide<StartupService>.Value() => StartupService;

  TemplateLabelLookupService IProvide<TemplateLabelLookupService>.Value() => TemplateLabelLookupService;

  UserInterface IProvide<UserInterface>.Value() => UserInterface;
  #endregion
}
