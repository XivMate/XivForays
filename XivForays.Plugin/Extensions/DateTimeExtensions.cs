using System;

namespace XivMate.DataGathering.Forays.Dalamud.Extensions;

/// <summary>
/// Extension methods for DateTime
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds)
    /// </summary>
    /// <param name="dateTime">DateTime to convert</param>
    /// <returns>Unix timestamp in seconds</returns>
    public static long ToUnixTime(this DateTime dateTime)
    {
        DateTimeOffset dto = new DateTimeOffset(dateTime.ToUniversalTime());
        return dto.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp with millisecond precision
    /// </summary>
    /// <param name="dateTime">DateTime to convert</param>
    /// <returns>Unix timestamp in milliseconds</returns>
    public static long ToUnixTimeMilliSeconds(this DateTime dateTime)
    {
        DateTimeOffset dto = new DateTimeOffset(dateTime.ToUniversalTime());
        return dto.ToUnixTimeMilliseconds();
    }
}
