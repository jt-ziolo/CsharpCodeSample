namespace MyGameName;
using Arch.Core;
using Arch.Core.Extensions;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class EcsNode : Node3D {
  [Dependency]
  public GameWorld WorldProvider => this.DependOn<GameWorld>();

  // This is the Entity associated with this node.
  protected Entity _entity;

  #region methods

  public override void _EnterTree() {
    if (!_entity.IsAlive()) {
      var world = WorldProvider.EcsWorld;
      _entity = world.Create(this);
    }

    _entity.Add(this);
    base._EnterTree();
  }

  public override void _ExitTree() {
    _entity.Remove<Node>();
    base._ExitTree();
  }

  public override void _Notification(int what) => this.Notify(what);

  protected override void Dispose(bool disposing) {
    if (disposing) {
      var world = World.Create();
      world.Destroy(_entity);
    }
    base.Dispose(disposing);
  }

  #endregion
}
