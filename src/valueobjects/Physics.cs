namespace MyGameName.ValueObjects.Physics;
using Godot;
using Vogen;

[ValueObject(typeof(double))]
public readonly partial struct Coefficient {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct Direction {
}

[ValueObject(typeof(Vector2))]
public readonly partial struct EulerRotation2D {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct EulerRotation3D {
}

[ValueObject(typeof(Vector2))]
public readonly partial struct EulerRotationDelta2D {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct EulerRotationDelta3D {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct GlobalPosition3D {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct GlobalPositionDelta3D {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct LocalPosition3D {
}

[ValueObject(typeof(Vector3))]
public readonly partial struct LocalPositionDelta3D {
}

[ValueObject(typeof(double))]
public readonly partial struct Magnitude {
}
