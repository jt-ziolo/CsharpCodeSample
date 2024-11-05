namespace MyGameName;
using Godot;

public static class ApplicationUtility {
  #region properties
  public static Platform AppPlatform { get; private set; }
  public static PlatformType AppPlatformType { get; private set; }
  public static bool IsDebugBuild { get; private set; }
  public static bool IsEditorBuild { get; private set; }
  public static bool IsHeadlessOrServer => OS.HasFeature("dedicated_server") || DisplayServer.GetName() == "headless";
  public static bool IsX32ArchBuild { get; private set; }
  private static bool IsApplicationInfoStored { get; set; }
  #endregion

  public static void StoreApplicationInfo() {
    if (IsApplicationInfoStored) {
      return;
    }
    IsApplicationInfoStored = true;
#if DEBUG
    IsDebugBuild = true;
#else
    IsDebugBuild = false;
#endif
#if TOOLS
    IsEditorBuild = true;
    AppPlatformType = PlatformType.PC;
#if GODOT_64
    IsX32ArchBuild = false;
#else
    IsX32ArchBuild = true;
#endif
#if GODOT_WINDOWS
    AppPlatform = Platform.Windows;
#elif GODOT_LINUXBSD
    AppPlatform = Platform.LinuxBSD;
#elif GODOT_MACOS
    AppPlatform = Platform.MacOS;
#elif GODOT_ANDROID
    AppPlatform = Platform.Android;
#elif GODOT_IOS
    AppPlatform = Platform.IOS;
#else
    AppPlatform = Platform.Web;
#endif
#else
    IsEditorBuild = false;
#if GODOT_64
    IsX32ArchBuild = false;
#else
    IsX32ArchBuild = true;
#endif
#if GODOT_PC
    AppPlatformType = PlatformType.PC;
#elif GODOT_MOBILE
    AppPlatformType = PlatformType.Mobile;
#else
    AppPlatformType = PlatformType.Web;
#endif
#if GODOT_WINDOWS
    AppPlatform = Platform.Windows;
#elif GODOT_LINUXBSD
    AppPlatform = Platform.LinuxBSD;
#elif GODOT_MACOS
    AppPlatform = Platform.MacOS;
#elif GODOT_ANDROID
    AppPlatform = Platform.Android;
#elif GODOT_IOS
    AppPlatform = Platform.IOS;
#else
    AppPlatform = Platform.Web;
#endif
#endif
  }

  public enum Platform {
    Windows,
    LinuxBSD,
    MacOS,
    Android,
    IOS,
    Web
  }

  public enum PlatformType {
    PC,
    Mobile,
    Web
  }
}
