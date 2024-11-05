namespace MyGameName.ValueObjects;
using System.Linq;
using Vogen;

[ValueObject(typeof(string))]
public readonly partial struct ModName {
  private const int MAX_LENGTH = 24;

  private static readonly char[] _allowed_chars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '_', '-'];

  private static string NormalizeInput(string input) {
    return input.SanitizeAll();
  }

  private static Validation Validate(string value) {
    if (value.Length > MAX_LENGTH) {
      return Validation.Invalid($"Exceeds the maximum length of {MAX_LENGTH} characters.");
    }
    for (int i = 0; i < value.Length; i++) {
      var next_char = value[i];
      if (!_allowed_chars.Contains(next_char)) {
        return Validation.Invalid($"Character {next_char} is not allowed (must be alphanumeric without whitespace: a-z, A-Z, 0-9, _).");
      }
    }
    return Validation.Ok;
  }
}
