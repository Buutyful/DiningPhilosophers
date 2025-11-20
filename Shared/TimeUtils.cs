using System.Diagnostics;

namespace Shared;

public static class TimeUtils
{
    public static long NowMs()
    {
        return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
}