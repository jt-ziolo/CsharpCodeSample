namespace MyGameName.ValueObjects.Layout;
using Godot;
using Vogen;

[ValueObject(typeof(Vector2))]
public readonly partial struct CanvasPosition {
}

[ValueObject(typeof(Vector2))]
public readonly partial struct CanvasPositionDelta {
}

[ValueObject(typeof(double))]
public readonly partial struct CanvasRotation {
}

[ValueObject(typeof(double))]
public readonly partial struct CanvasRotationDelta {
}

[ValueObject(typeof(double))]
public readonly partial struct ColorComponent {
}

[ValueObject(typeof(double))]
public readonly partial struct Offset {
}
