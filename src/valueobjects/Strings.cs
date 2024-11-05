namespace MyGameName.ValueObjects.Strings;
using Vogen;

[ValueObject(typeof(string))]
public readonly partial struct Prefix {
}

[ValueObject(typeof(string))]
public readonly partial struct StringNoWhitespace {
  public static Validation Validate(string value) {
    if (value.Contains(' ') || value.Contains('\n') || value.Contains('\t')) {
      return Validation.Invalid($"Must not contain whitespace characters ({value}).");
    }
    return Validation.Ok;
  }
}

[ValueObject(typeof(string))]
public readonly partial struct Suffix {
}
