using Shared;

namespace DiningPhilosophers;

//DONT WORRY ABOUT THE TABLE "GOD CLASS",
//THE WHOLE POINT OF THIS EXCERCISE IS TO HAVE SHARED RESOURCES THAT DIFFERENT THREADS HAVE ACCESS TO,
//WHILE AVOIDING DEADLOCKS AND RACE CONDITIONS
public sealed class Table
{
    private readonly Fork[] _forks;
    private readonly Philosopher[] _philosophers;
    private readonly CancellationTokenSource _cts = new();

    private readonly long[] _lastMealTimestampsMs;
    private readonly int[] _mealCounts;

    private readonly Lock _consoleLock = new();
    private long _simulationStartTimeMs;

    private readonly int _timeToDieMs;
    private readonly int _timeToEatMs;
    private readonly int _timeToSleepMs;
    private readonly int _targetMeals;

    public Table(int nPhilosophers, int timeToDieMs, int timeToEatMs, int timeToSleepMs, int targetMeals)
    {
        if (nPhilosophers < 1) throw new ArgumentException("one philosopher need.", nameof(nPhilosophers));
        _timeToDieMs = timeToDieMs;
        _timeToEatMs = timeToEatMs;
        _timeToSleepMs = timeToSleepMs;
        _targetMeals = targetMeals;

        _forks = new Fork[nPhilosophers];
        for (int i = 0; i < _forks.Length; i++)
        {
            _forks[i] = new Fork(i);
        }

        _lastMealTimestampsMs = new long[nPhilosophers];
        _mealCounts = new int[nPhilosophers];
        _philosophers = new Philosopher[nPhilosophers];

        for (int i = 0; i < nPhilosophers; i++)
        {
            Fork left = _forks[i];
            Fork right = _forks[(i + 1) % nPhilosophers];
            _philosophers[i] = new Philosopher(i, left, right);
        }
    }
    public async Task StartAsync()
    {
        _simulationStartTimeMs = TimeUtils.NowMs();
        long baselineTimestamp = TimeUtils.NowMs();
        for (int i = 0; i < _lastMealTimestampsMs.Length; i++)
        {
            Interlocked.Exchange(ref _lastMealTimestampsMs[i], baselineTimestamp);
        }

        var tasks = new List<Task>();
        foreach (var p in _philosophers)
        {
            tasks.Add(Task.Run(() => PhilosopherLoopAsync(p, _cts.Token)));
        }

        var monitor = Task.Run(() => MonitorLoopAsync(_cts.Token));
        tasks.Add(monitor);

        SafeWrite("Simulation starting...");
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            _cts.Cancel();
        }
        finally
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        SafeWrite("Simulation ended.");
    }

    private async Task PhilosopherLoopAsync(Philosopher p, CancellationToken ct)
    {
        int id = p.Id;
        while (!ct.IsCancellationRequested)
        {
            Fork firstFork = p.Left.Id < p.Right.Id ? p.Left : p.Right;
            Fork secondFork = firstFork == p.Left ? p.Right : p.Left;

            //alive check
            long lastEatenTimestamp = Volatile.Read(ref _lastMealTimestampsMs[id]);
            long timeSinceLastMeal = TimeUtils.NowMs() - lastEatenTimestamp;
            long timeRemaining = _timeToDieMs - timeSinceLastMeal;

            if (timeRemaining <= 0) return;

            //get forks
            bool gotFirst = await firstFork.Available.WaitAsync((int)timeRemaining, ct);
            if (!gotFirst) return;

            lastEatenTimestamp = Volatile.Read(ref _lastMealTimestampsMs[id]);
            timeSinceLastMeal = TimeUtils.NowMs() - lastEatenTimestamp;
            timeRemaining = _timeToDieMs - timeSinceLastMeal;

            if (timeRemaining <= 0)
            {
                firstFork.Available.Release();
                return;
            }

            bool gotSecond = await secondFork.Available.WaitAsync((int)timeRemaining, ct);
            if (!gotSecond)
            {
                firstFork.Available.Release();
                return;
            }

            //eat
            Interlocked.Exchange(ref _lastMealTimestampsMs[id], TimeUtils.NowMs());
            Interlocked.Increment(ref _mealCounts[id]);

            ReportEat(id, TimestampMs());
            await Task.Delay(_timeToEatMs, ct);

            firstFork.Available.Release();
            secondFork.Available.Release();

            //sleep
            ReportSleep(id, TimestampMs());
            await Task.Delay(_timeToSleepMs, ct);

            //think
            ReportThink(id, TimestampMs());

        }
    }


    private async Task MonitorLoopAsync(CancellationToken ct)
    {
        const int pollIntervalMs = 5;

        while (!ct.IsCancellationRequested)
        {
            for (int i = 0; i < _lastMealTimestampsMs.Length; i++)
            {
                long lastMealTimestamp = Volatile.Read(ref _lastMealTimestampsMs[i]);
                long sinceLastMeal = TimeUtils.NowMs() - lastMealTimestamp;
                if (sinceLastMeal > _timeToDieMs)
                {
                    ReportDeath(i, TimestampMs());
                    _cts.Cancel();
                    return;
                }
            }

            bool allFinished = true;
            for (int i = 0; i < _mealCounts.Length; i++)
            {
                if (Volatile.Read(ref _mealCounts[i]) < _targetMeals)
                {
                    allFinished = false;
                    break;
                }
            }

            if (allFinished)
            {
                SafeWrite($"{TimestampMs()}ms: All philosophers completed {_targetMeals} meals.");
                _cts.Cancel();
                return;
            }

            await Task.Delay(pollIntervalMs, ct);
        }
    }

    private void ReportEat(int id, long ts) => SafeWrite($"{ts}ms: {id} is eating (meal #{Volatile.Read(ref _mealCounts[id])})");
    private void ReportSleep(int id, long ts) => SafeWrite($"{ts}ms: {id} is sleeping");
    private void ReportThink(int id, long ts) => SafeWrite($"{ts}ms: {id} is thinking");
    private void ReportDeath(int id, long ts) => SafeWrite($"{ts}ms: {id} died");

    private long TimestampMs() => TimeUtils.NowMs() - _simulationStartTimeMs;

    //COULD IMPLEMENT A LOGGER THAT TAKES MESSAGES IN A CHANNEL, THAT WOULD BE BEST
    //FOR THE SAKE OF THE EXCERCISE KEEPING THINGS SHORT
    private void SafeWrite(string line)
    {
        using var scope = _consoleLock.EnterScope();
        {
            Console.WriteLine(line);
        }
    }
}