namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using Scriban.Runtime;

[Meta(typeof(IAutoNode))]
public partial class TemplateLabelLookupService : Node {
  public ScriptObject ScriptObject { get; } = new();

  public override void _Notification(int what) => this.Notify(what);
}
