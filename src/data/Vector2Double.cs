namespace MyGameName;

using Godot;

public readonly struct Vector2Double(double x, double y) {
  public readonly double X { get; init; } = x;
  public readonly double Y { get; init; } = y;

  public static Vector2Double operator +(Vector2Double vecA, Vector2Double vecB) => new(vecA.X + vecB.X, vecA.Y + vecB.Y);
  public static Vector2Double operator /(Vector2Double vec, double factor) => new(vec.X / factor, vec.Y / factor);
  public static explicit operator Vector2(Vector2Double v) => new((float)v.X, (float)v.Y);
  public static Vector2Double operator *(Vector2Double vec, double factor) => new(vec.X * factor, vec.Y * factor);
  public static Vector2Double operator -(Vector2Double vecA, Vector2Double vecB) => new(vecA.X - vecB.X, vecA.Y - vecB.Y);

  public override string ToString() {
    return $"({x}, {y})";
  }

  public Vector2Double Normalized() => this / Length();

  public double Length() {
    return Mathf.Sqrt(LengthSquared());
  }

  public double LengthSquared() {
    return X * X + Y * Y;
  }
}
