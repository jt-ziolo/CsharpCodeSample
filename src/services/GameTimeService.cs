namespace MyGameName;
using Godot;
using System;

public partial class GameTimeService : Node {
  private const string GAME_TIME_PAUSE_FLAG = "GameTimePaused";

  #region properties
  public string CurrentDayOfWeek => Now.GetDayOfWeek<DayOfWeek>();
  public double DayPctProgress => Now.GetPercentOfWayThroughCurrent(DayUnit);
  public TimeLengthUnit DayUnit { get; private set; } = new TimeLengthUnit("day", "days", 1);

  // Every second is one minute
  [Export]
  public double GameSecondsPerRealSecond { get; set; } = 60.0;

  public bool IsGameTimePaused => FlagState.GetAnyFlagsSet(GAME_TIME_PAUSE_FLAG);
  public bool IsPaused => FlagState.GetAnyFlagsSet(PauseService.PAUSE_FLAG) || IsGameTimePaused;
  public double MoonCyclePctProgress => Now.GetPercentOfWayThroughCurrent(MoonCycleUnit);
  public TimeLengthUnit MoonCycleUnit { get; private set; } = new TimeLengthUnit("moon", "moons", 29.5);
  public TimeSpan Now { get; private set; } = new TimeSpan(0);
  public string Season => Now.GetConvertedToEnum<Seasons>(SeasonUnit);
  public double SeasonPctProgress => Now.GetPercentOfWayThroughCurrent(SeasonUnit);
  public TimeLengthUnit SeasonUnit { get; private set; } = new TimeLengthUnit("season", "seasons", 365.0 / 4.0);
  public double YearPctProgress => Now.GetPercentOfWayThroughCurrent(YearUnit);
  public TimeLengthUnit YearUnit { get; private set; } = new TimeLengthUnit("year", "years", 365.0);
  #endregion

  #region methods

  public override void _Process(double delta) {
    TryAddToGameTime(delta);
  }

  public override void _Ready() {
    var formattedTimeArr = Now.Get_HH_MM_FormattedTimeWith_AM_PM();
    var timeStringNow = formattedTimeArr.GetFormattedPaddingAndColon_HH_MM_XM();
  }

  /// <summary>
  /// Increments InGameTime by delta seconds, if not paused.
  /// </summary>
  /// <param name="delta">Time passed in seconds.</param>
  /// <returns>True if time was successfully added, false otherwise.</returns>
  public bool TryAddToGameTime(double delta) {
    if (IsPaused) {
      return false;
    }
    Now += TimeSpan.FromSeconds(delta * GameSecondsPerRealSecond);
    return true;
  }

  /// <summary>
  /// Sets InGameTime to the time, if not paused.
  /// </summary>
  /// <param name="time"></param>
  /// <returns>True if time was successfully set, false otherwise.</returns>
  public bool TrySetGameTime(TimeSpan time) {
    if (IsPaused) {
      return false;
    }
    Now = time;
    return true;
  }

  #endregion

  public enum DayOfWeek {
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
  }

  public enum Seasons {
    Spring,
    Summer,
    Fall,
    Winter,
  }
}
