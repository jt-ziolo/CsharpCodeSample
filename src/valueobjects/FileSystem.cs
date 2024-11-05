namespace MyGameName.ValueObjects.FileSystem;
using System.IO;
using Vogen;

[ValueObject(typeof(string))]
public readonly partial struct FileExtension {
  private static Validation Validate(string value) {
    if (value.Contains('/') || value.Contains('\\')) {
      return Validation.Invalid($"Must not contain directory separator characters ({value}).");
    }
    if (!value.StartsWith('.')) {
      return Validation.Invalid($"Must start with a dot (.) character ({value}).");
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct FileName {
  private static Validation Validate(string value) {
    if (value.Contains('/') || value.Contains('\\')) {
      return Validation.Invalid($"Must not contain directory separator characters ({value}).");
    }
    if (Path.HasExtension(value)) {
      return Validation.Invalid($"Must not have extension ({value}).");
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct FileNameWithExtension {
  private static Validation Validate(string value) {
    if (value.Contains('/') || value.Contains('\\')) {
      return Validation.Invalid($"Must not contain directory separator characters ({value}).");
    }
    if (!Path.HasExtension(value)) {
      return Validation.Invalid($"Must have extension ({value}).");
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct GlobalizedPath {
  internal static Validation Validate(string value) {
    if (value.StartsWith(GodotPath.UserPathPrefix.Value) || value.StartsWith(GodotPath.ResPathPrefix.Value)) {
      return Validation.Invalid($"Must not start with a Godot path prefix ({value}).");
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct GlobalizedPathToFile {
  private static Validation Validate(string value) {
    var globalizedPathValidation = GlobalizedPath.Validate(value);
    if (globalizedPathValidation != Validation.Ok) {
      return globalizedPathValidation;
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct GlobalizedPathToFileWithExtension {
  private static Validation Validate(string value) {
    var globalizedPathValidation = GlobalizedPath.Validate(value);
    if (globalizedPathValidation != Validation.Ok) {
      return globalizedPathValidation;
    }
    if (!Path.HasExtension(value)) {
      return Validation.Invalid($"Must have extension ({value}).");
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct GodotPath {
  public static readonly GodotPath ResPathPrefix = From("res://");
  public static readonly GodotPath UserPathPrefix = From("user://");

  private static Validation Validate(string value) {
    if (value.Contains('\\')) {
      return Validation.Invalid($"Must not contain a backslash character ({value}).");
    }
    if (!value.StartsWith("user://") && !value.StartsWith("res://")) {
      return Validation.Invalid($"Does not start with a valid Godot path prefix ({value}).");
    }
    return Validation.Ok;
  }
}
