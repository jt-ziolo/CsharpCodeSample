namespace MyGameName;
using EnumFastToStringGenerated;
using Godot;
using MyGameName.Enums;
using MyGameName.Exceptions;
using MyGameName.ValueObjects.FileSystem;
using System.Collections.Generic;
using System.IO;

public partial class ConfigService : Node {
  #region properties

  public List<IConfigSetting> ConfigSettings { get; } = new List<IConfigSetting> {
    new WindowModeConfigSetting()
  };

  private static GodotPath CONFIG_CMD_LINE_ARGS_FILE_PATH => GodotPath.From("user://cmdlineargs.txt");
  private static GodotPath ConfigFilePath => GodotPath.From("user://settings.cfg");
  #endregion

  internal ConfigFile _config = new();
  private static readonly List<string> _commandLineArgs = new();

  #region methods

  internal void ApplyConfig() {
    var platform = ApplicationUtility.AppPlatform;
    var isHeadless = ApplicationUtility.IsHeadlessOrServer;
    foreach (var setting in ConfigSettings) {
      setting.ApplyConfigSetting(platform, isHeadless, _config, GetTree().Root);
      _commandLineArgs.Add(setting.CommandLineArg);
    }
    Logger.Log(this, Logger.LogColorByPurpose.Config, "Writing command line args to cmdlineargs.txt");
    File.WriteAllLines(PathUtility.GetGlobalizedPathSafe(CONFIG_CMD_LINE_ARGS_FILE_PATH).Value, _commandLineArgs);
    _commandLineArgs.Clear();
  }

  internal void LoadFromConfigFile() {
    var entries = new Godot.Collections.Dictionary();

    // Load data from a file
    var error = _config.Load(ConfigFilePath.Value);

    // If the file didn't load, throw
    if (error != Error.Ok) {
      throw new ConfigLoadException(ConfigFilePath.Value, error);
    }
  }

  internal void SaveConfig() {
    GetTree().CallGroup(GodotGroup.ModPersist.ToStringFast(), "SaveConfig", "user://ModConfig/");
    _config.Save(ConfigFilePath.Value);
  }

  #endregion
}
