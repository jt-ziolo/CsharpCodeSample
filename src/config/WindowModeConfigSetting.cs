namespace MyGameName;

using Godot;
using MyGameName.ValueObjects.Strings;

public sealed class WindowModeConfigSetting : EnumConfigSetting<Window.ModeEnum> {
  public override Variant DefaultValue => Variant.From((long)Window.ModeEnum.ExclusiveFullscreen);

  public override StringNoWhitespace Key => (StringNoWhitespace)"Mode";

  public override StringNoWhitespace Section => (StringNoWhitespace)"Window";

  protected override void ApplyValue(Node rootNode, long variantValue) {
    rootNode.GetWindow().Mode = (Window.ModeEnum)variantValue;
  }

  protected override long GetCurrentlyAppliedValue(Node rootNode) {
    return (long)rootNode.GetWindow().Mode;
  }
}
