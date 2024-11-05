namespace MyGameName;
using Godot;
using MyGameName.ValueObjects.Strings;
using System.Collections.Generic;

public static class SaveUtility {
  private const int SAVE_DICTIONARY_VERSION = 1;

  #region methods

  public static Variant GetSavedValue(this Godot.Collections.Dictionary<string, Variant> dictionary, string key, Variant fallback) {
    if (!dictionary.TryGetValue(key, out var value)) {
      return fallback;
    }
    return value;
  }

  public static Vector3 GetSavedVector(this Godot.Collections.Dictionary<string, Variant> dictionary, Prefix keyPrefix, Vector3 fallback) {
    Vector3 vector;
    if (!dictionary.ContainsKey($"{keyPrefix}X")) {
      return fallback;
    }
    vector = new Vector3((float)dictionary[$"{keyPrefix}X"], (float)dictionary[$"{keyPrefix}Y"], (float)dictionary[$"{keyPrefix}Z"]);
    return vector;
  }

  public static Vector2 GetSavedVector(this Godot.Collections.Dictionary<string, Variant> dictionary, Prefix keyPrefix, Vector2 fallback) {
    Vector2 vector;
    if (!dictionary.ContainsKey($"{keyPrefix}X")) {
      return fallback;
    }
    vector = new Vector2((float)dictionary[$"{keyPrefix}X"], (float)dictionary[$"{keyPrefix}Y"]);
    return vector;
  }

  public static void LoadNode3D(this Node3D node, Godot.Collections.Dictionary<string, Variant> dictionary) {
    node.Position = dictionary.GetSavedVector((Prefix)"Pos", node.Position);
    node.Rotation = dictionary.GetSavedVector((Prefix)"Rot", node.Rotation);
    node.Transform = node.Transform.Orthonormalized();
    node.Scale = dictionary.GetSavedVector((Prefix)"Scale", node.Scale);
    node.Visible = (bool)GetSavedValue(dictionary, "Visible", node.Visible);
  }

  public static void SaveBasicData(this Node node, Godot.Collections.Dictionary<string, Variant> dictionary) {
    dictionary.TryAdd("_GameVersion", ProjectSettings.GetSetting("application/config/version"));
    dictionary.TryAdd("_SaveVersion", SAVE_DICTIONARY_VERSION);
    dictionary.TryAdd("_NodePath", node.GetPath());
    dictionary.TryAdd("_Name", node.Name);
  }

  public static void SaveNode3D(this Node3D node, Godot.Collections.Dictionary<string, Variant> dictionary) {
    dictionary.TryAdd("_IsNode3D", node.GetType().IsAssignableTo(typeof(Node3D)));
    dictionary.TryAddVectorValues((Prefix)"Pos", node.Position);
    dictionary.TryAddVectorValues((Prefix)"Rot", node.Rotation);
    dictionary.TryAddVectorValues((Prefix)"Scale", node.Scale);
    dictionary.TryAdd("Visible", node.Visible);
  }

  public static void TryAddVectorValues(this Godot.Collections.Dictionary<string, Variant> dictionary, Prefix keyPrefix, Vector3 vector) {
    dictionary.TryAdd($"{keyPrefix}X", vector.X);
    dictionary.TryAdd($"{keyPrefix}Y", vector.Y);
    dictionary.TryAdd($"{keyPrefix}Z", vector.Z);
  }

  public static void TryAddVectorValues(this Godot.Collections.Dictionary<string, Variant> dictionary, Prefix keyPrefix, Vector2 vector) {
    dictionary.TryAdd($"{keyPrefix}X", vector.X);
    dictionary.TryAdd($"{keyPrefix}Y", vector.Y);
  }

  #endregion
}
