namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using UnitsNet;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class OriginShifting : Node3D {
  /// <summary>
  /// Float precision losses start to become significant around 4096. 2048 may
  /// be a safer choice. See:
  /// https://docs.godotengine.org/en/4.0/tutorials/physics/large_world_coordinates.html#why-use-large-world-coordinates
  /// Note: Issues with warping may result if this value is too low
  /// </summary>
  [Export(PropertyHint.Range, "8.0,4096.0")]
  public double LengthThreshold {
    get => _lengthThreshold.Value; set {
      _lengthThreshold = Length.FromMeters(value);
      _lengthThresholdSquared = Area.FromSquareMeters(Mathf.Pow(_lengthThreshold.As(UnitsNet.Units.LengthUnit.Meter), 2.0f));
    }
  }

  [Dependency]
  private GameWorld WorldProvider => this.DependOn<GameWorld>();

  private Length _lengthThreshold = Length.FromMeters(2048.0f);
  private Area _lengthThresholdSquared = Area.FromSquareMeters(4_194_304.0f);

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public override void _PhysicsProcess(double delta) {
    // Check if this node's position is too far from the origin
    var distanceToOriginSquared = GlobalPosition.LengthSquared();
    if (_lengthThresholdSquared.Value > distanceToOriginSquared) {
      return;
    }
    ShiftSceneRoot();
  }

  public void Initialize() {
    // Force the squared calculation
    LengthThreshold = _lengthThreshold.Value;
    SetPhysicsProcess(true);
  }

  public void OnResolved() {
    ShiftSceneRoot();
  }

  private void ShiftSceneRoot() {
    var currentPosition = GlobalPosition;
    WorldProvider.Translate(-1.0f * currentPosition);
    // This node's global position should now be 0,0,0
  }

  #endregion
}
