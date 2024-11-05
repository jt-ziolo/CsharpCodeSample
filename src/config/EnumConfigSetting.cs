namespace MyGameName;
using Godot;
using MyGameName.ValueObjects.Strings;
using System;
using System.Linq;

public abstract class EnumConfigSetting<TEnum> : IConfigSetting where TEnum : struct, System.Enum {
  #region properties
  public string CommandLineArg { get; protected set; }
  public abstract Variant DefaultValue { get; }
  public abstract StringNoWhitespace Key { get; }
  public abstract StringNoWhitespace Section { get; }
  #endregion

  #region methods

  public virtual void ApplyConfigSetting(ApplicationUtility.Platform platform, bool isHeadless, ConfigFile configFile, Node rootNode) {
    ApplyConfigSettingStatic_Private(configFile, rootNode);
    CommandLineArg += $" (default: {DefaultValue})";
    var enumValues = Enum.GetValues<TEnum>();
    CommandLineArg += $" (accepts: {string.Join(", ", enumValues.Select((value, idx) => $"{idx} => {value}"))})";
  }

  protected abstract void ApplyValue(Node rootNode, long variantValue);

  protected abstract long GetCurrentlyAppliedValue(Node rootNode);

  protected static bool TryGetCmdLineValue(StringNoWhitespace section, StringNoWhitespace key, out string cmdLineArgAndValue, out Variant value) {
    var cmdLineArg = $"--{section.Value.ToLowerInvariant()}-{key.Value.ToLowerInvariant()}";
    var notSetEntry = $"{cmdLineArg} [not set, type=Enum as long]";

    var argList = OS.GetCmdlineUserArgs().Where(arg => arg.StartsWith(cmdLineArg, StringComparison.OrdinalIgnoreCase) || arg.Equals(cmdLineArg, StringComparison.OrdinalIgnoreCase));
    if (argList.Any()) {
      var match = argList.First();
      cmdLineArgAndValue = match;
      var cmdLineValue = cmdLineArgAndValue[(cmdLineArgAndValue.IndexOf('=') + 1)..].Trim();
      Variant parsedVariant;
      if (long.TryParse(cmdLineValue, out var longResult)) {
        parsedVariant = Variant.From(longResult);
      }
      else {
        Logger.LogError("Config", $"Incorrect value passed for config ({cmdLineValue}). Expected type: Enum as long.");
        cmdLineArgAndValue += $" [incorrect type, expected=Enum as long]";
        value = default;
        return false;
      }
      value = parsedVariant;
      return true;
    }
    cmdLineArgAndValue = notSetEntry;
    value = string.Empty;
    return false;
  }

  private void ApplyConfigSettingStatic_Private(ConfigFile configFile, Node rootNode) {
    var currentValue = GetCurrentlyAppliedValue(rootNode);
    // Override with the command line arg, if provided
    if (TryGetCmdLineValue(Section, Key, out var cmdLineArgAndValue, out var result)) {
      Logger.Log("Config", Logger.LogColorByPurpose.Config, $"Detected cmd line entry: '{cmdLineArgAndValue}'.");
      CommandLineArg = cmdLineArgAndValue;
      if (result.Equals(currentValue)) {
        Logger.Log("Config", Logger.LogColorByPurpose.Config, $"Config value ({Section}/{Key} => {result}) matches current setting, skipping...");
        return;
      }
      ApplyValue(rootNode, (long)result.As(typeof(long))!);
      return;
    }
    CommandLineArg = cmdLineArgAndValue;

    if (!configFile.HasSectionKey((string)Section, (string)Key)) {
      configFile.SetValue((string)Section, (string)Key, Variant.From(DefaultValue));
    }
    var value = (long)configFile.GetValue((string)Section, (string)Key).As(typeof(long))!;
    if (value.Equals(currentValue)) {
      Logger.Log("Config", Logger.LogColorByPurpose.Config, $"Config value ({Section}/{Key} => {value}) matches current setting, skipping...");
      return;
    }
    Logger.Log("Config", Logger.LogColorByPurpose.Config, $"Applying config value ({Section}/{Key} => {value}).");
    ApplyValue(rootNode, value);
  }

  #endregion
}
