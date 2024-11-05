namespace MyGameName;
using Godot;
using MyGameName.ValueObjects.Strings;
using System;
using System.Linq;

public abstract class ConfigSetting<[MustBeVariant] TVariantValue> : IConfigSetting {
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
  }

  protected abstract void ApplyValue(Node rootNode, TVariantValue variantValue);

  protected abstract TVariantValue GetCurrentlyAppliedValue(Node rootNode);

  /// <summary>
  /// This is needed because GD.StrToVar requires quotation marks around
  /// strings to work, but we don't know if the value is a string or not.
  /// </summary>
  protected static Variant ParseStringToClosestVariantType(string cmdLineValue, out Variant.Type variantType) {
    // Check for null
    if (cmdLineValue.ToLowerInvariant() == "null") {
      variantType = Variant.Type.Nil;
      return default;
    }
    // Check for true/false first
    if (cmdLineValue.ToLowerInvariant() == "true") {
      variantType = Variant.Type.Bool;
      return Variant.From(true);
    }
    else if (cmdLineValue.ToLowerInvariant() == "false") {
      variantType = Variant.Type.Bool;
      return Variant.From(false);
    }
    // Check for advanced Godot struct types which are represented like this:
    // "Vector3(0, 0, 0)"
    // Full list: Aabb, Projection, Quaternion, Color, Rect2, Rect2I,
    // Transform2D, Transform3D, Basis, Plane, VectorX, VectorXI.
    // Will omit Aabb, Projection, Quaternion, Transform2D, Transform3D, Basis.
    var hasPrefix = cmdLineValue.Contains('(');
    if (hasPrefix) {
      var prefix = cmdLineValue.Split('(', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First().ToLowerInvariant();
      switch (prefix) {
        case "color":
          variantType = Variant.Type.Color;
          break;
        case "rect2":
          variantType = Variant.Type.Rect2;
          break;
        case "rect2i":
          variantType = Variant.Type.Rect2I;
          break;
        case "plane":
          variantType = Variant.Type.Plane;
          break;
        case "vector2":
          variantType = Variant.Type.Vector2;
          break;
        case "vector2i":
          variantType = Variant.Type.Vector2I;
          break;
        case "vector3":
          variantType = Variant.Type.Vector3;
          break;
        case "vector3i":
          variantType = Variant.Type.Vector3I;
          break;
        case "vector4":
          variantType = Variant.Type.Vector4;
          break;
        case "vector4i":
          variantType = Variant.Type.Vector4I;
          break;
        default:
          // Not recognized but has opening parens, assume it's a string
          variantType = Variant.Type.String;
          return Variant.From(cmdLineValue);
      }
      return GD.StrToVar(cmdLineValue);
    }

    // Check if the cmd line value can be parsed into a signed numeric type:
    // int, long, float, double. There are others, but they aren't often found
    // in Godot.
    // Godot's int type is equivalent to C#'s long type, and it's float type is
    // equivalent to C#'s double type.
    if (int.TryParse(cmdLineValue, out var intResult)) {
      variantType = Variant.Type.Int;
      return Variant.From(intResult);
    }
    if (long.TryParse(cmdLineValue, out var longResult)) {
      variantType = Variant.Type.Int;
      return Variant.From(longResult);
    }
    if (float.TryParse(cmdLineValue, out var floatResult)) {
      variantType = Variant.Type.Float;
      return Variant.From(floatResult);
    }
    if (double.TryParse(cmdLineValue, out var doubleResult)) {
      variantType = Variant.Type.Float;
      return Variant.From(doubleResult);
    }
    // Assume it's a string
    variantType = Variant.Type.String;
    return Variant.From(cmdLineValue);
  }

  protected static bool TryGetCmdLineValue(StringNoWhitespace section, StringNoWhitespace key, out string cmdLineArgAndValue, out Variant value) {
    var cmdLineArg = $"--{section.Value.ToLowerInvariant()}-{key.Value.ToLowerInvariant()}";
    var tValueType = typeof(TVariantValue);
    var notSetEntry = $"{cmdLineArg} [not set, type={tValueType.Name}]";

    var argList = OS.GetCmdlineUserArgs().Where(arg => arg.StartsWith(cmdLineArg, StringComparison.OrdinalIgnoreCase) || arg.Equals(cmdLineArg, StringComparison.OrdinalIgnoreCase));
    if (argList.Any()) {
      var match = argList.First();
      cmdLineArgAndValue = match;
      if (tValueType == typeof(bool)) {
        value = true;
        return true;
      }
      var cmdLineValue = cmdLineArgAndValue[(cmdLineArgAndValue.IndexOf('=') + 1)..].Trim();
      var parsedVariant = ConfigSetting<TVariantValue>.ParseStringToClosestVariantType(cmdLineValue, out var variantType);
      if (parsedVariant.Obj == null || variantType != GD.TypeToVariantType(tValueType)) {
        Logger.LogError("Config", $"Incorrect value passed for config ({parsedVariant}, type: {parsedVariant.VariantType}). Expected type: {tValueType.Name}.");
        cmdLineArgAndValue += $" [incorrect type, expected={tValueType.Name}]";
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
      ApplyValue(rootNode, (TVariantValue)result.As(typeof(TVariantValue))!);
      return;
    }
    CommandLineArg = cmdLineArgAndValue;

    if (!configFile.HasSectionKey((string)Section, (string)Key)) {
      configFile.SetValue((string)Section, (string)Key, Variant.From(DefaultValue));
    }
    var value = (TVariantValue)configFile.GetValue((string)Section, (string)Key).As(typeof(TVariantValue))!;
    if (value.Equals(currentValue)) {
      Logger.Log("Config", Logger.LogColorByPurpose.Config, $"Config value ({Section}/{Key} => {value}) matches current setting, skipping...");
      return;
    }
    Logger.Log("Config", Logger.LogColorByPurpose.Config, $"Applying config value ({Section}/{Key} => {value}).");
    ApplyValue(rootNode, value);
  }

  #endregion
}
