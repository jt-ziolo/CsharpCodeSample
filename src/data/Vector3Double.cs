namespace MyGameName;

using Godot;

public readonly struct Vector3Double(double x, double y, double z) {
  public readonly double X { get; init; } = x;
  public readonly double Y { get; init; } = y;
  public readonly double Z { get; init; } = z;

  public static Vector3Double operator +(Vector3Double vecA, Vector3Double vecB) => new(vecA.X + vecB.X, vecA.Y + vecB.Y, vecA.Z + vecB.Z);
  public static explicit operator Vector3Double(Vector3 v) => new(v.X, v.Y, v.Z);
  public static Vector3Double operator *(Vector3Double vec, double factor) => new(vec.X * factor, vec.Y * factor, vec.Z * factor);
  public static Vector3Double operator /(Vector3Double vec, double factor) => new(vec.X / factor, vec.Y / factor, vec.Z / factor);
  public override string ToString() {
    return $"({x}, {y}, {z})";
  }

  public double Length() {
    return Mathf.Sqrt(LengthSquared());
  }

  public double LengthSquared() {
    return X * X + Y * Y + Z * Z;
  }

  internal Vector3Double Normalized() => this / Length();
}
