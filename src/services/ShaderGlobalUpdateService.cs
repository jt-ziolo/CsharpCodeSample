namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EnumFastToStringGenerated;
using Godot;
using MyGameName.Enums;

[Meta(typeof(IAutoNode))]
public partial class ShaderGlobalUpdateService : Node {
  public float TimeScale { get; set; } = 1.0f;

  public override void _Notification(int what) => this.Notify(what);

  public override void _Process(double delta) {
    RenderingServer.GlobalShaderParameterSet(GodotShaderGlobal.timescale.ToStringFast(), TimeScale);
  }
}
