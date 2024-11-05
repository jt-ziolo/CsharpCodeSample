namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using CLSS;
using Godot;
using MyGameName.Enums;
using MyGameName.ValueObjects.FileSystem;
using MyGameName.ValueObjects.Time;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

[Meta(typeof(IAutoNode))]
public partial class SaveGameService : Node {
  #region properties

  [Dependency]
  private GameSession GameSession => this.DependOn<GameSession>();

  [Dependency]
  private PauseService PauseService => this.DependOn<PauseService>();

  [Dependency]
  private UserInterface UserInterface => this.DependOn<UserInterface>();

  #endregion

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public GlobalizedPathToFile GetSaveFilePathNoExtension(GameSession session) {
    var savePath = PathUtility.GetGlobalizedPathSafe((GodotPath)"user://saves/");
    if (!Directory.Exists(savePath.Value)) {
      Directory.CreateDirectory(savePath.Value);
    }
    var path = (GlobalizedPathToFile)PathUtility.GetGlobalizedPathSafe((GodotPath)$"user://saves/{PathUtility.JoinAndReplaceWhitespace(session.WorldName.Value, session.CharacterName.Value)}").Value;
    return path;
  }

  public async Task LoadGameDataAsync() {
    if (GameSession == null) {
      return;
    }
    UserInterface.ShowSaveLoadIndicator = true;
    var saveFilePath = GetSaveFilePathNoExtension(GameSession);
    if (!File.Exists($"{saveFilePath}.br")) {
      return;
    }
    var tempSaveDir = (GlobalizedPath)Directory.CreateTempSubdirectory($"{ProjectSettings.GetSetting("application/config/name")}_Load_").FullName;
    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Using temporary directory: {tempSaveDir}");
    await DecompressAndUnzipAsync(tempSaveDir, saveFilePath);
    this.CallGroup(GodotGroup.Persist, "OnPreLoad");
    this.CallGroup(GodotGroup.ModPersist, "OnPreLoad");

    this.SetFlagFromThisNode(PauseService.PAUSE_FLAG);
    var groupNodes = GetTree().GetNodesInGroup(GodotGroup.Persist.ToString());
    groupNodes.ForEach(async node => await LoadDataFromDirectoryAsync(node, tempSaveDir));
    groupNodes = GetTree().GetNodesInGroup(GodotGroup.ModPersist.ToString());
    groupNodes.ForEach(async node => await LoadDataFromDirectoryAsync(node, (GlobalizedPath)Path.Join(tempSaveDir.Value, "ModSave")));
    await this.TimerFromSelf((Duration)0.5f);
    this.UnsetFlagFromThisNode(PauseService.PAUSE_FLAG);

    this.CallGroup(GodotGroup.Persist, "OnPostLoad");
    this.CallGroup(GodotGroup.ModPersist, "OnPostLoad");
    await Task.Delay(1000);
    UserInterface.ShowSaveLoadIndicator = false;
  }

  public async Task SaveGameDataAsync(bool isResponseToQuit = false) {
    if (GameSession == null) {
      return;
    }
    UserInterface.ShowSaveLoadIndicator = true;
    var tempSaveDir = (GlobalizedPath)Directory.CreateTempSubdirectory($"{ProjectSettings.GetSetting("application/config/name")}_Save_").FullName;
    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Using temporary directory: {tempSaveDir}");
    this.CallGroup(GodotGroup.Persist, "OnPreSave");
    this.CallGroup(GodotGroup.ModPersist, "OnPreSave");

    this.SetFlagFromThisNode(PauseService.PAUSE_FLAG);
    var groupNodes = GetTree().GetNodesInGroup(GodotGroup.Persist.ToString());
    groupNodes.ForEach(async node => await SaveDataToDirectoryAsync(node, tempSaveDir));
    groupNodes = GetTree().GetNodesInGroup(GodotGroup.ModPersist.ToString());
    groupNodes.ForEach(async node => await SaveDataToDirectoryAsync(node, (GlobalizedPath)Path.Join(tempSaveDir.Value, "ModSave")));
    this.UnsetFlagFromThisNode(PauseService.PAUSE_FLAG);

    await FinalizeSaveAsync(tempSaveDir, GetSaveFilePathNoExtension(GameSession));
    this.CallGroup(GodotGroup.Persist, "OnPostSave");
    this.CallGroup(GodotGroup.ModPersist, "OnPostSave");

    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Finished saving.");
    UserInterface.ShowSaveLoadIndicator = false;
    if (!isResponseToQuit) {
      return;
    }
    GetTree().Quit(0);
  }

  private static async Task DecompressAndUnzipAsync(GlobalizedPath tempSaveDir, GlobalizedPathToFile saveFilePath) {
    await PathUtility.DecompressBrotliToZipAsync(new FileInfo($"{saveFilePath}.br"));
    var zipFileName = (GlobalizedPathToFileWithExtension)$"{saveFilePath}.zip";
    ZipFile.ExtractToDirectory(zipFileName.Value, tempSaveDir.Value);
    // Delete the zip file
    File.Delete(zipFileName.Value);
  }

  private async Task FinalizeSaveAsync(GlobalizedPath tempSaveDir, GlobalizedPathToFile saveFileName) {
    // Create zip file in save directory
    var zipFileName = (GlobalizedPathToFileWithExtension)$"{saveFileName}.zip";
    ZipFile.CreateFromDirectory(tempSaveDir.Value, zipFileName.Value, CompressionLevel.NoCompression, false);
    // Apply Brotli compression to the zip file
    var fileInfo = new FileInfo(zipFileName.Value);
    var brotliFileName = await PathUtility.CompressZipToBrotliAsync(fileInfo);
    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Created save file: {brotliFileName}");
    // Delete the zip file
    File.Delete(zipFileName.Value);
  }

  private async Task LoadDataFromDirectoryAsync(Node node, GlobalizedPath saveDir) {
    var saveNode = (ISaveNode)node;
    var path = (GlobalizedPathToFileWithExtension)Path.Join(saveDir.Value, $"{saveNode.GetSaveName()}.json");
    if (!File.Exists(path.Value)) {
      return;
    }
    var jsonString = await File.ReadAllTextAsync(path.Value);
    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Loading data from {path}");
    var dictionary = (Godot.Collections.Dictionary<string, Variant>)Json.ParseString(jsonString);
    saveNode.LoadDataFromDictionary(dictionary);
    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Finished loading.");
  }

  private async Task SaveDataToDirectoryAsync(Node node, GlobalizedPath saveDir) {
    var saveNode = (ISaveNode)node;
    var path = Path.Join(saveDir.Value, $"{saveNode.GetSaveName()}.json");
    Logger.Log(this, Logger.LogColorByPurpose.FileOperation, $"Saving data to {path}");
    var dictionary = new Godot.Collections.Dictionary<string, Variant>();
    saveNode.AddDataToDictionary(dictionary);
    var jsonText = Json.Stringify(dictionary, fullPrecision: true);
    await File.WriteAllTextAsync(path, jsonText);
  }

  #endregion
}
