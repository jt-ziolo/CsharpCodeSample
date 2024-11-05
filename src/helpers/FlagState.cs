namespace MyGameName;
using CLSS;
using Godot;
using System.Collections.Generic;

public static class FlagState {
  private static Dictionary<string, HashSet<ulong>> FlagDictionary { get; } = new();

  #region methods

  public static void ForceUnsetAllFlags(this Node node, string referenceString) {
    var set = FlagDictionary.GetOrAdd(referenceString, new HashSet<ulong>());
    set.Clear();
  }

  public static bool GetAnyFlagsSet(string referenceString) {
    Prune();
    if (!FlagDictionary.TryGetValue(referenceString, out var set)) {
      return false;
    }
    if (set.Count == 0) {
      return false;
    }
    return true;
  }

  public static bool SetFlagFromThisNode(this Node node, string referenceString) {
    var key = node.GetInstanceId();
    var set = FlagDictionary.GetOrAdd(referenceString, _ => new HashSet<ulong>());
    return set.Add(key);
  }

  public static bool UnsetFlagFromThisNode(this Node node, string referenceString) {
    var key = node.GetInstanceId();
    var set = FlagDictionary.GetOrAdd(referenceString, new HashSet<ulong>());
    return set.Remove(key);
  }

  private static void Prune() {
    var sets = FlagDictionary.Values;
    foreach (var set in sets) {
      set.RemoveWhere(id => !GodotObject.IsInstanceIdValid(id));
    }
  }

  #endregion
}
