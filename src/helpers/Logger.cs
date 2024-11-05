namespace MyGameName;

using System.Collections.Generic;
using Godot;

public static class Logger {
  public static Queue<string> PreviousWarnings { get; private set; } = new();
  public static Queue<string> PreviousErrors { get; private set; } = new();
  public static int WarningsAndErrorsPreserved { get; set; }

  #region methods

  public static void Log(params string[] what) {
    var output = $"[color={LogColor.gray}][b]Debug:[/b] {string.Join(", ", what)}[/color]";
    GD.PrintRich(output);
  }

  public static void Log(string title, LogColor color, params string[] what) {
    var output = string.Join(", ", what);
    output = $"[color={color}][b]{title}:[/b] {output}[/color]";
    GD.PrintRich(output);
  }

  public static void Log(string title, LogColorByPurpose color, params string[] what) => Log(title, (LogColor)color, what);

  public static void Log(Node sourceNode, LogColor color, params string[] what) {
    var title = GetTitle(sourceNode);
    Log(title, color, what);
  }

  public static void Log(Node sourceNode, LogColorByPurpose color, params string[] what) => Log(sourceNode, (LogColor)color, what);

  public static void LogError(string title, params string[] what) {
    var output = string.Join(", ", what);
    output = $"{title}: {output}";
    GD.PushError(output);
    if (WarningsAndErrorsPreserved > 0) {
      PreviousErrors ??= new();
      if (PreviousErrors.Count >= WarningsAndErrorsPreserved) {
        PreviousErrors.Dequeue();
      }
      PreviousErrors.Enqueue(output);
    }
  }

  public static void LogError(Node sourceNode, params string[] what) => LogError(GetTitle(sourceNode), what);

  public static void LogWarning(string title, params string[] what) {
    var output = string.Join(", ", what);
    output = $"{title}: {output}";
    GD.PushWarning(output);
    if (WarningsAndErrorsPreserved > 0) {
      PreviousWarnings ??= new();
      if (PreviousWarnings.Count >= WarningsAndErrorsPreserved) {
        PreviousWarnings.Dequeue();
      }
      PreviousWarnings.Enqueue(output);
    }
  }

  public static void LogWarning(Node sourceNode, params string[] what) => LogWarning(GetTitle(sourceNode), what);

  private static string GetTitle(Node sourceNode) => sourceNode.GetPath();
  #endregion

  public enum LogColor {
    black, red, green, yellow, blue, magenta, pink, purple, cyan, white, orange, gray
  }

  public enum LogColorByPurpose {
    None = LogColor.black, UnrecoverableIssue = LogColor.red, Success = LogColor.green, EngineAnomaly = LogColor.yellow, Networking = LogColor.blue, Application = LogColor.magenta, UIAnomaly = LogColor.pink, GameplayAnomaly = LogColor.purple, Config = LogColor.cyan, Default = LogColor.white, FileOperation = LogColor.orange, Debug = LogColor.gray
  }
}
