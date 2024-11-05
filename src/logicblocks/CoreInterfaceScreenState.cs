namespace MyGameName;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State))]
public partial class CoreInterfaceScreenState : LogicBlock<CoreInterfaceScreenState.State> {
  public override Transition GetInitialState() => To<State.SplashScreen>();

  public sealed record Data {
    public bool LoadingComplete { get; set; }
  }

  public CoreInterfaceScreenState() {
    Set(new Data() { LoadingComplete = false });
  }

  public static class Input {
    public readonly record struct AnyButtonPressed;
    public readonly record struct Restarting;
    public readonly record struct CinematicChanged(bool IsActive);
    public readonly record struct InteractiveWindowToggled(bool IsActive);
    public readonly record struct Progressing(bool ToLoadScreen);
  }

  public static class Output {
    public readonly record struct Disclaimer(bool IsVisible);
    public readonly record struct HUD(bool IsVisible);
    public readonly record struct LoadScreen(bool IsVisible);
    public readonly record struct MainMenu(bool IsVisible);
    public readonly record struct SplashScreen(bool IsVisible);
  }

  public abstract record State : StateLogic<State> {
    public record SplashScreen : State, IGet<Input.Progressing>, IGet<Input.AnyButtonPressed> {
      public SplashScreen() {
        this.OnEnter(() => Output(new Output.SplashScreen(IsVisible: true)));
        this.OnEnter(() => Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured);
        this.OnExit(() => Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Confined);
      }

      public Transition On(in Input.Progressing input) {
        return To<DisclaimerScreen>();
      }

      public Transition On(in Input.AnyButtonPressed input) => To<DisclaimerScreen>();
    }
    public record DisclaimerScreen : State, IGet<Input.Progressing> {
      public DisclaimerScreen() {
        this.OnEnter(() => Output(new Output.SplashScreen(IsVisible: false)));
        this.OnEnter(() => Output(new Output.Disclaimer(IsVisible: true)));
      }

      public Transition On(in Input.Progressing input) {
        return To<MainMenuScreen>();
      }
    }
    public record MainMenuScreen : State, IGet<Input.Progressing>, IGet<Input.CinematicChanged>, IGet<Input.InteractiveWindowToggled>, IGet<Input.Restarting> {
      public MainMenuScreen() {
        this.OnEnter(() => Output(new Output.Disclaimer(IsVisible: false)));
        this.OnEnter(() => Output(new Output.MainMenu(IsVisible: true)));
      }

      public Transition On(in Input.Progressing input) => To<LoadScreen>();
      public Transition On(in Input.CinematicChanged input) {
        if (input.IsActive) {
          Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        }
        else {
          Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Confined;
        }
        return ToSelf();
      }
      public Transition On(in Input.InteractiveWindowToggled input) {
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Confined;
        return ToSelf();
      }

      public Transition On(in Input.Restarting input) {
        // Restart upon loading mods in certain cases
        this.OnEnter(() => Output(new Output.MainMenu(IsVisible: false)));
        return To<SplashScreen>();
      }
    }
    public record LoadScreen : State, IGet<Input.Progressing>, IGet<Input.AnyButtonPressed> {
      public LoadScreen() {
        this.OnEnter(() => Output(new Output.MainMenu(IsVisible: false)));
        this.OnEnter(() => Output(new Output.HUD(IsVisible: false)));
        this.OnEnter(() => Output(new Output.LoadScreen(IsVisible: true)));
      }

      public Transition On(in Input.Progressing input) {
        Get<Data>().LoadingComplete = true;
        return ToSelf();
      }

      public Transition On(in Input.AnyButtonPressed input) {
        if (Get<Data>().LoadingComplete) {
          Get<Data>().LoadingComplete = false;
          return To<HUD>();
        }
        return ToSelf();
      }
    }
    public record HUD : State, IGet<Input.Progressing>, IGet<Input.CinematicChanged>, IGet<Input.InteractiveWindowToggled> {
      public HUD() {
        this.OnEnter(() => Output(new Output.LoadScreen(IsVisible: false)));
        this.OnEnter(() => Output(new Output.HUD(IsVisible: true)));
        this.OnEnter(() => Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured);
        this.OnExit(() => Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Confined);
      }

      public Transition On(in Input.Progressing input) {
        if (input.ToLoadScreen) {
          return To<LoadScreen>();
        }
        return To<MainMenuScreen>();
      }
      public Transition On(in Input.CinematicChanged input) {
        if (input.IsActive) {
          Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        }
        else {
          Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Confined;
        }
        return ToSelf();
      }
      public Transition On(in Input.InteractiveWindowToggled input) {
        if (input.IsActive) {
          Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Confined;
        }
        else {
          Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        }
        return ToSelf();
      }
    }
  }
}

