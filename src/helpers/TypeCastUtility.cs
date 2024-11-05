namespace MyGameName;
using Godot;
using System;

public static class TypeCastUtility {
  public static object? As(this Variant variant, Type type) => Convert.ChangeType(variant.Obj, type);
}
