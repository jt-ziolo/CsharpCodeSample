namespace MyGameName;
using Godot;
using MyGameName.ValueObjects.Strings;

public interface IConfigSetting {
  public StringNoWhitespace Section { get; }
  public StringNoWhitespace Key { get; }
  public string CommandLineArg { get; }

  public void ApplyConfigSetting(ApplicationUtility.Platform platform, bool isHeadless, ConfigFile configFile, Node rootNode);
  public Variant DefaultValue { get; }
}
