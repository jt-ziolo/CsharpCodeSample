using StringSanitizer.StringSanitizer;

public static class StringUtility {
  public static string SanitizeAll(this string input) {
    var result = input.SanitizeEmojis(" ")
      .SanitizeCreditCard()
      .SanitizeExcessiveSpaces()
      .SanitizeLeadingEmptyCharacter()
      .SanitizeLinebreaks()
      .SanitizeUrls()
      .SanitizeEndingEmptyCharacter();
    return result;
  }
}
