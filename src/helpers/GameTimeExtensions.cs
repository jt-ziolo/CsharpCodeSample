namespace MyGameName;
using System;

public static class GameTimeExtensions {
  #region methods

  /// <returns>The hours and minutes as a string array in military time, e.g. [14,3] for 2:03PM</returns>
  public static string[] Get_HH_MM_FormattedTime(this TimeSpan timeSpan) {
    return [timeSpan.ToString(@"%h"), timeSpan.ToString(@"%m")];
  }

  /// <returns>The hours, minutes, and AM/PM time designation as a string array formatted in 12hr time, e.g. [12,5,AM] for 00:05, [12,23,PM] for 12:23, and [4,50,PM] for 16:50</returns>
  public static string[] Get_HH_MM_FormattedTimeWith_AM_PM(this TimeSpan timeSpan) {
    // 23:00 = 11PM
    // 02:00 = 2AM
    // 01:00 = 1AM
    // 00:00 = 12AM
    // -01:00 = 11PM
    // -02:00 = 10PM
    var hours = timeSpan.Hours % 24;
    if (hours == 0) {
      return ["12", timeSpan.ToString(@"%m"), "AM"];
    }
    if (hours > 0 && hours < 12) {
      return [hours.ToString(), timeSpan.ToString(@"%m"), "AM"];
    }
    if (hours == 12) {
      return ["12", timeSpan.ToString(@"%m"), "PM"];
    }
    return [(hours - 12).ToString(), timeSpan.ToString(@"%m"), "PM"];
  }

  /// <returns>The total amount of time represented by the timeSpan in the provided unit.</returns>
  public static double GetAmountInUnit(this TimeSpan timeSpan, TimeLengthUnit unit, bool truncate = false) {
    return unit.GetAmountInTimeSpan(timeSpan, truncate);
  }

  public static string GetConvertedToEnum<TEnum>(this TimeSpan timeSpan, TimeLengthUnit unit) where TEnum : System.Enum {
    var index = timeSpan.GetAmountInUnit(unit) % System.Enum.GetValues(typeof(TEnum)).Length;
    return System.Enum.GetName(typeof(TEnum), index);
  }

  public static string GetDayOfWeek<TEnum>(this TimeSpan timeSpan) where TEnum : System.Enum {
    var dayOfWeekIndex = timeSpan.Days % System.Enum.GetValues(typeof(TEnum)).Length;
    return System.Enum.GetName(typeof(TEnum), dayOfWeekIndex);
  }

  /// <returns>The days left until time next increments in this unit.</returns>
  public static double GetDaysRemainingInCurrent(this TimeSpan now, TimeLengthUnit unit, bool truncate = false) {
    return unit.GetDaysRemainingInCurrent(now);
  }

  /// <returns>The amount of time in days since time last incremented in this unit.</returns>
  public static double GetDaysSinceStartOfCurrent(this TimeSpan now, TimeLengthUnit unit, bool truncate = false) {
    return unit.GetDaysSinceStartOfCurrent(now);
  }

  public static string GetFormattedPaddingAndColon_HH_MM(this string[] strings) {
    return $"{strings[0].PadLeft(2, '0')}:{strings[1].PadLeft(2, '0')}";
  }

  public static string GetFormattedPaddingAndColon_HH_MM_XM(this string[] strings, bool addSpace = false) {
    return $"{strings[0].PadLeft(2, '0')}:{strings[1].PadLeft(2, '0')}{(addSpace ? " " : "")}{strings[2]}";
  }

  /// <returns>The percentage of time passed between when time last incremented in this unit and when it will increment next.</returns>
  public static double GetPercentOfWayThroughCurrent(this TimeSpan now, TimeLengthUnit unit) {
    return unit.GetPercentOfWayThroughCurrent(now);
  }

  /// <returns>The percentage of time left between now and when time will next increment in this unit.</returns>
  public static double GetPercentRemainingInCurrent(this TimeSpan now, TimeLengthUnit unit) {
    return unit.GetPercentRemainingInCurrent(now);
  }

  /// <returns>The amount of time passed since it last incremented in this unit.</returns>
  public static TimeSpan GetTimeSinceStartOfCurrent(this TimeSpan now, TimeLengthUnit unit) {
    return unit.GetTimeSinceStartOfCurrent(now);
  }

  /// <returns>The amount of time left to go until it next increments in this unit.</returns>
  public static TimeSpan GetTimeUntilEndOfCurrent(this TimeSpan now, TimeLengthUnit unit) {
    return unit.GetTimeUntilEndOfCurrent(now);
  }

  #endregion
}
