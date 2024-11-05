namespace MyGameName.ValueObjects.Time;
using Vogen;

[ValueObject(typeof(double))]
public readonly partial struct Delay {
}

[ValueObject(typeof(double))]
public readonly partial struct Duration {
  public static bool operator >(Duration left, Duration right) {
    return left.CompareTo(right) > 0;
  }

  public static bool operator >=(Duration left, Duration right) {
    return left.CompareTo(right) >= 0;
  }

  public static bool operator <(Duration left, Duration right) {
    return left.CompareTo(right) < 0;
  }

  public static bool operator <=(Duration left, Duration right) {
    return left.CompareTo(right) <= 0;
  }
}
