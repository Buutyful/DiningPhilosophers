using System.Diagnostics;

namespace DiningPhilosophers;

public static class TimeUtils
{
    public static long NowMs()
    {
        return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
}