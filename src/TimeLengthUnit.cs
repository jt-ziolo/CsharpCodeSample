namespace MyGameName;
using System;

public readonly struct TimeLengthUnit {
  public readonly string Name { get; }
  public readonly string NamePlural { get; }
  public readonly double TotalDaysPer { get; }

  public TimeLengthUnit(string name, string namePlural, double totalDaysPer) {
    Name = name;
    NamePlural = namePlural;
    if (totalDaysPer <= 0.0) {
      throw new ArgumentException($"totalDaysPer must be > 0.0");
    }
    TotalDaysPer = totalDaysPer;
  }

  public TimeLengthUnit(string name, string namePlural, TimeSpan timeSpanPer) {
    Name = name;
    NamePlural = namePlural;
    var totalDaysPer = timeSpanPer.TotalDays;
    if (totalDaysPer <= 0.0) {
      throw new ArgumentException($"totalDaysPer must be > 0.0");
    }
    TotalDaysPer = totalDaysPer;
  }

  /// <summary>
  /// Get the amount of this unit within the provided timespan. For example,
  /// for TimeLengthUnit Week, if the timespan is exactly 7 days then the
  /// result would be 1.
  /// </summary>
  /// <param name="timeSpan">The timespan being converted.</param>
  /// <returns></returns>
  public double GetAmountInTimeSpan(TimeSpan timeSpan, bool truncate = false) {
    var amount = timeSpan.TotalDays / TotalDaysPer;
    return truncate ? double.Truncate(amount) : amount;
  }

  /// <returns>The days left until time next increments in this unit.</returns>
  public double GetDaysRemainingInCurrent(TimeSpan now, bool truncate = false) {
    var amount = TotalDaysPer - GetDaysSinceStartOfCurrent(now);
    return truncate ? double.Truncate(amount) : amount;
  }

  /// <returns>The amount of time in days since time last incremented in this unit.</returns>
  public double GetDaysSinceStartOfCurrent(TimeSpan now, bool truncate = false) {
    var amount = now.TotalDays % TotalDaysPer;
    return truncate ? double.Truncate(amount) : amount;
  }

  /// <returns>The percentage of time passed between when time last incremented in this unit and when it will increment next.</returns>
  public double GetPercentOfWayThroughCurrent(TimeSpan now) {
    return GetDaysSinceStartOfCurrent(now) / TotalDaysPer;
  }

  /// <returns>The percentage of time left between now and when time will next increment in this unit.</returns>
  public double GetPercentRemainingInCurrent(TimeSpan now) {
    return 1.0 - GetPercentOfWayThroughCurrent(now);
  }

  public (double, string) GetPluralCorrectName(TimeSpan span) {
    bool isPlural;
    var time = GetAmountInTimeSpan(span);
    isPlural = Math.Abs(time) != 1.0;
    return (time, isPlural ? this.NamePlural : this.Name);
  }

  /// <returns>The amount of time passed since it last incremented in this unit.</returns>
  public TimeSpan GetTimeSinceStartOfCurrent(TimeSpan now) {
    return TimeSpan.FromDays(GetDaysSinceStartOfCurrent(now));
  }

  /// <summary>
  /// Get the timespan equivalent to the provided amount of this unit. For
  /// example, for TimeLengthUnit Week, if amount is 2 then the result would
  /// be a timespan of exactly 14 days.
  /// </summary>
  /// <param name="amount">The amount being converted.</param>
  /// <returns></returns>
  public TimeSpan GetTimeSpanFromAmount(double amount) {
    return TimeSpan.FromDays(TotalDaysPer * amount);
  }

  /// <returns>The amount of time left to go until it next increments in this unit.</returns>
  public TimeSpan GetTimeUntilEndOfCurrent(TimeSpan now) {
    return TimeSpan.FromDays(GetDaysRemainingInCurrent(now));
  }
}
