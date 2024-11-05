namespace MyGameName.Exceptions;

using Godot;
using System;

public sealed class ConfigLoadException : Exception {
  public ConfigLoadException(string path, Error error) : base(message: $"Failed to load ConfigFile at path ({path}), reason: {error}.") {
  }
}
