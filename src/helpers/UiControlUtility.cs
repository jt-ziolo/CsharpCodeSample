namespace MyGameName;
using Godot;

public static class UiControlUtility {
  public static void RotateClockwiseThisFrame(this Control control, UnitsNet.RotationalSpeed speed, double delta) {
    control.Rotation += (float)(speed.As(UnitsNet.Units.RotationalSpeedUnit.RadianPerSecond) * delta);
  }

  public static void RotateCounterClockwiseThisFrame(this Control control, UnitsNet.RotationalSpeed speed, double delta) {
    control.RotateClockwiseThisFrame(-speed, delta);
  }
}
