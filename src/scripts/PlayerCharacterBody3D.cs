namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EnumFastToStringGenerated;
using Godot;
using Vector3 = Godot.Vector3;
using GTweens.Builders;
using GTweens.Easings;
using GTweens.Tweens;
using GTweensGodot.Extensions;
using MyGameName.Enums;
using MyGameName.ValueObjects.Layout;
using MyGameName.ValueObjects.Physics;
using MyGameName.ValueObjects.Strings;
using MyGameName.ValueObjects.Time;
using System.Collections.Generic;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class PlayerCharacterBody3D : CharacterBody3D, ISaveNode {
  private const float TWEEN_FADE_DURATION = 0.5f;
  private const float TWEEN_FADE_WARP_DELAY = 0.05f;

  #region properties
  public Vector3 Acceleration { get; set; }

  [Export(PropertyHint.Range, "0,1")]
  public float AirControlFactor { get; private set; } = 0.4f;

  [Export(PropertyHint.Range, "0,1")]
  public float AirResistanceFactor { get; set; } = 0.3f;

  [Export(PropertyHint.Range, "0,7")]
  public float AngularSpeedFactor { get; private set; } = Mathf.Pi / 180.0f / 5.0f;

  [Node("HeadPivot/PlayerCamera")]
  public Camera3D Camera { get; set; }

  [Export(PropertyHint.Range, "0,99")]
  public float DelayBetweenMidAirJumps { get; private set; } = 0.2f;

  public Magnitude Gravity { get; set; }
  public Direction GravityDirection { get; set; }

  [Node("HeadPivot")]
  public Node3D HeadPivot { get; private set; }

  [Export(PropertyHint.Range, "1,10")]
  public float InverseFrictionPower { get; private set; } = 3.0f;

  [Export(PropertyHint.Range, "0,1")]
  public float InverseInertiaFactor { get; private set; } = 1.0f;

  [Export(PropertyHint.Range, "0,100")]
  public float JumpImpulse { get; set; } = 5.0f;

  public LocalPosition3D LastSafeGroundPosition { get; set; }

  [Export(PropertyHint.Range, "0,100")]
  public float MaxGroundedAcceleration { get; private set; } = 2.0f;

  [Export(PropertyHint.Range, "0,10000")]
  public float MaxXZSpeed { get; private set; } = 40.0f;

  [Export(PropertyHint.Range, "0,10000")]
  public float MaxYSpeed { get; private set; } = 90.0f;

  public int MidAirJumpsCount { get; set; }
  public bool MidAirJumpTimerExpired { get; set; } = true;

  [Export(PropertyHint.Range, "0,1,or_greater")]
  public int NumberOfMidAirJumps { get; private set; } = 1;

  [Export(PropertyHint.Range, "0,1000")]
  public float SpeedFactorSprint { get; private set; } = 100.0f;

  [Export(PropertyHint.Range, "0,1000")]
  public float SpeedFactorWalk { get; private set; } = 40.0f;

  public Duration TimeInMidAirAfterJump { get; set; } = Duration.From(0.0);
  public Duration TimeSpentFalling { get; set; } = Duration.From(0.0);

  [Dependency]
  public UserInterface UI => this.DependOn<UserInterface>();

  private CanvasPositionDelta MouseMotionThisFrame { get; set; } = CanvasPositionDelta.From(Vector2.Zero);
  private EulerRotationDelta2D TargetXYRotation { get; set; } = EulerRotationDelta2D.From(Vector2.Zero);
  #endregion

  #region fields
  private Magnitude _arrowsTurnInput = Magnitude.From(10.0f);
  private Duration _fellUnderWorldTimeThreshold = Duration.From(10.0f);
  private CanvasPositionDelta _mouseMotionLastFrame;
  #endregion

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public override void _UnhandledInput(InputEvent @event) {
    if (@event is InputEventMouseMotion motionEvent) {
      MouseMotionThisFrame = (CanvasPositionDelta)motionEvent.Relative;
    }
    if (@event.IsActionPressed(GodotInputAction.jump.ToStringFast())) {
      Jump();
    }
  }

  public void FixPlayerFellUnderWorld() {
    if (TimeSpentFalling < _fellUnderWorldTimeThreshold) {
      return;
    }
    TimeSpentFalling = (Duration)0.0f;
    Logger.Log(this, Logger.LogColorByPurpose.GameplayAnomaly, $"Player fell under the world, attempting to warp...");
    // Fade out and then in
    GTween tween = GTweenSequenceBuilder.New()
     .Append(UI.Overlay.TweenModulateAlpha(1.0f, TWEEN_FADE_DURATION))
     .AppendTime(TWEEN_FADE_WARP_DELAY)
     .AppendCallback(WarpPlayerToSafeGroundPosition)
     .AppendTime(TWEEN_FADE_WARP_DELAY)
     .Append(UI.Overlay.TweenModulateAlpha(0.0f, TWEEN_FADE_DURATION))
     .Build();
    tween.SetEasing(Easing.InOutCubic);
    tween.Play();
  }

  public float GetFrictionFromCollisions() {
    var count = GetSlideCollisionCount();
    var totalFriction = 0.0f;
    var countForAverage = 0;
    for (int i = 0; i < count; i++) {
      var col = GetSlideCollision(i);
      var physicsMaterial = (PhysicsMaterial?)col.GetCollider().Get(StaticBody3D.PropertyName.PhysicsMaterialOverride);
      float friction = 0.0f;
      if (physicsMaterial != null) {
        friction = physicsMaterial.Friction;
      }
      if (friction == 0.0f) {
        continue;
      }
      totalFriction += friction;
      countForAverage++;
    }
    if (countForAverage == 0) {
      // don't divide by zero
      return 0.0f;
    }
    return totalFriction / countForAverage;
  }

  public Direction GetMoveInputVector() {
    var input = Vector3.Zero;
    if (Input.IsActionPressed(GodotInputAction.move_right.ToStringFast())) {
      input += Basis.X;
    }
    if (Input.IsActionPressed(GodotInputAction.move_left.ToStringFast())) {
      input -= Basis.X;
    }
    if (Input.IsActionPressed(GodotInputAction.move_forward.ToStringFast())) {
      input -= Basis.Z;
    }
    if (Input.IsActionPressed(GodotInputAction.move_backwards.ToStringFast())) {
      input += Basis.Z;
    }
    var direction = (Direction)input.Normalized();
    return direction;
  }

  public EulerRotationDelta3D GetTurnInput() {
    // pitch, yaw, roll
    var input = Vector3.Zero;
    if (Input.IsActionPressed(GodotInputAction.look_up.ToStringFast())) {
      input = input with { Y = input.Y - 1.0f };
    }
    if (Input.IsActionPressed(GodotInputAction.look_down.ToStringFast())) {
      input = input with { Y = input.Y + 1.0f };
    }
    if (Input.IsActionPressed(GodotInputAction.look_right.ToStringFast())) {
      input = input with { X = input.X + 1.0f };
    }
    if (Input.IsActionPressed(GodotInputAction.look_left.ToStringFast())) {
      input = input with { X = input.X - 1.0f };
    }
    var magnitude = (Magnitude)MouseMotionThisFrame.Value.Length();
    if (magnitude.Value < float.Epsilon) {
      magnitude = _arrowsTurnInput;
    }
    input = input with { X = input.X + MouseMotionThisFrame.Value.X, Y = input.Y + MouseMotionThisFrame.Value.Y };
    var turnInput = (EulerRotationDelta3D)(input.Normalized() * (float)magnitude.Value);
    return turnInput;
  }

  public void OnPhysicsProcess(double delta) {
    RotateObjectLocal(Vector3.Up, -TargetXYRotation.Value.X);
    HeadPivot.RotateObjectLocal(Vector3.Right, TargetXYRotation.Value.Y);
    MoveAndSlide();
    // Friction
    var frictionCoefficient = GetFrictionFromCollisions();
    if (frictionCoefficient <= 1E-3) {
      frictionCoefficient = AirResistanceFactor;
    }
    Velocity = Velocity.Lerp(Vector3.Zero with { Y = Velocity.Y }, Mathf.Pow(frictionCoefficient, InverseFrictionPower));
    if (Mathf.Abs(Velocity.Y) <= 1.0f && IsOnFloorOnly()) {
      // Safe ground
      LastSafeGroundPosition = (LocalPosition3D)Position;
      RenderingServer.GlobalShaderParameterSet(GodotShaderGlobal.player_position.ToStringFast(), Position);
    }
    // Ortho-normalize to address deformation
    Transform = Transform.Orthonormalized();
  }

  public void OnProcess(double delta) {
    UpDirection = -1.0f * GravityDirection.Value;
    if (!MidAirJumpTimerExpired) {
      TimeInMidAirAfterJump = (Duration)(TimeInMidAirAfterJump.Value + (float)delta);
    }
    UpdateVelocity(delta);
    FixPlayerFellUnderWorld();
    UpdateAngularVelocity(delta);
  }

  public void Initialize() {
    Gravity = (Magnitude)PhysicsServer3D.AreaGetParam(GetViewport().FindWorld3D().Space, PhysicsServer3D.AreaParameter.Gravity).As<float>();
    GravityDirection = (Direction)PhysicsServer3D.AreaGetParam(GetViewport().FindWorld3D().Space, PhysicsServer3D.AreaParameter.GravityVector).As<Vector3>();
  }

  public void OnResolved() {
    SetProcess(true);
    SetPhysicsProcess(true);
  }

  void ISaveNode.AddDataToDictionary(Godot.Collections.Dictionary<string, Variant> dictionary) {
    this.SaveBasicData(dictionary);
    this.SaveNode3D(dictionary);
    dictionary.TryAdd("HeadPivotRotX", HeadPivot.Rotation.X);
    dictionary.TryAddVectorValues((Prefix)"Accel", Acceleration);
    dictionary.TryAddVectorValues((Prefix)"LastSafeGroundPos", LastSafeGroundPosition.Value);
    dictionary.TryAdd("TimeInMidAir", TimeInMidAirAfterJump.Value);
    dictionary.TryAdd("MidAirJumpsCount", MidAirJumpsCount);
    dictionary.TryAdd("MidAirJumpTimerExpired", MidAirJumpTimerExpired);
  }

  string ISaveNode.GetSaveName() => Name;

  void ISaveNode.LoadDataFromDictionary(Godot.Collections.Dictionary<string, Variant> dictionary) {
    this.LoadNode3D(dictionary);
    Acceleration = dictionary.GetSavedVector((Prefix)"Accel", Acceleration);
    LastSafeGroundPosition = (LocalPosition3D)dictionary.GetSavedValue("LastSafeGroundPos", LastSafeGroundPosition.Value).As<Vector3>();
    var rotationX = (float)dictionary.GetSavedValue("HeadPivotRotX", HeadPivot.Rotation.X);
    HeadPivot.RotateX(rotationX);
    TimeInMidAirAfterJump = (Duration)dictionary.GetSavedValue("TimeInMidAir", TimeInMidAirAfterJump.Value).As<float>();
    MidAirJumpsCount = (int)dictionary.GetSavedValue("MidAirJumpsCount", MidAirJumpsCount);
    MidAirJumpTimerExpired = (bool)dictionary.GetSavedValue("MidAirJumpTimerExpired", MidAirJumpTimerExpired);
  }

  void ISaveNode.OnPostLoad() { }

  void ISaveNode.OnPostSave() { }

  void ISaveNode.OnPreLoad() { }

  void ISaveNode.OnPreSave() { }

  private void Jump() {
    if (TimeInMidAirAfterJump.Value >= DelayBetweenMidAirJumps) {
      MidAirJumpTimerExpired = true;
    }
    var canMidAirJump = (MidAirJumpsCount < NumberOfMidAirJumps) && MidAirJumpTimerExpired;
    var isOnFloor = IsOnFloor();
    if (isOnFloor) {
      MidAirJumpTimerExpired = true;
    }
    if (!isOnFloor && !canMidAirJump) {
      return;
    }
    var jumpVector = UpDirection * JumpImpulse;
    Velocity += jumpVector;

    MidAirJumpTimerExpired = false;
    TimeInMidAirAfterJump = (Duration)0.0f;
    if (!isOnFloor) {
      MidAirJumpsCount++;
    }
    else {
      MidAirJumpsCount = 0;
    }
  }

  private void UpdateAngularVelocity(double delta) {
    if (MouseMotionThisFrame == _mouseMotionLastFrame) {
      MouseMotionThisFrame = (CanvasPositionDelta)Vector2.Zero;
    }
    _mouseMotionLastFrame = MouseMotionThisFrame;

    var input = GetTurnInput();
    TargetXYRotation = EulerRotationDelta2D.From(new Vector2(input.Value.X, input.Value.Y) * AngularSpeedFactor);
  }

  private void UpdateVelocity(double delta) {
    var isSprinting = Input.IsActionPressed("sprint");
    Acceleration = GetMoveInputVector().Value * (isSprinting ? SpeedFactorSprint : SpeedFactorWalk);
    if (!IsOnFloor()) {
      // Limit air control
      Acceleration = Acceleration with { X = Acceleration.X * AirControlFactor, Z = Acceleration.Z * AirControlFactor };
      if (Velocity.Y < 0.0f) {
        TimeSpentFalling = (Duration)(TimeSpentFalling.Value + (float)delta);
      }
    }
    else {
      TimeSpentFalling = (Duration)0.0f;
    }
    var clampMagVec = Vector3.One * MaxGroundedAcceleration;
    Acceleration.Clamp(-1.0f * clampMagVec, clampMagVec);
    // Gravity
    Acceleration += GravityDirection.Value * (float)Gravity.Value;
    Velocity = Velocity.Lerp(Velocity + (Acceleration * (float)delta), InverseInertiaFactor);
    Velocity = Velocity with {
      X = Mathf.Clamp(Velocity.X, -MaxXZSpeed, MaxXZSpeed),
      Y = Mathf.Clamp(Velocity.Y, -MaxYSpeed, MaxYSpeed),
      Z = Mathf.Clamp(Velocity.Z, -MaxXZSpeed, MaxXZSpeed)
    };
  }

  private void WarpPlayerToSafeGroundPosition() {
    // Warp the player
    Acceleration = Vector3.Zero;
    Velocity = Vector3.Zero;
    Position = (Vector3)LastSafeGroundPosition;
    Logger.Log(this, Logger.LogColorByPurpose.GameplayAnomaly, $"Warped the player to {Position}.");
  }

  #endregion
}
