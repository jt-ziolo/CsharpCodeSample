namespace MyGameName;

using System;
using Godot;
using MyGameName.ValueObjects;

internal record struct ModData(
  string[] Authors,
  ModName Name,
  string[] LoadBefore,
  string[] LoadAfter,
  string[] IncompatibleWith,
  Version Version,
  Texture2D Icon,
  ConfigFile Config,
  PackedScene ModScene,
  Node ModSceneRootNode
) {
  public override string ToString() {
    return $"{Name} {Version} by {string.Join(", ", Authors)}";
  }
}

