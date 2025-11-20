using DiningPhilosophersCoroutine;
using Shared;

public class Table
{
    private readonly Settings _config;
    private readonly SemaphoreSlim[] _forks;
    private readonly Philosopher[] _philosophers;
    private readonly long _startTime;

    public Table(Settings config)
    {
        _config = config;
        _forks = new SemaphoreSlim[config.N];
        _philosophers = new Philosopher[config.N];
        _startTime = TimeUtils.NowMs();

        for (int i = 0; i < config.N; i++) _forks[i] = new SemaphoreSlim(1, 1);

        for (int i = 0; i < config.N; i++)
        {
            var left = _forks[i];
            var right = _forks[(i + 1) % config.N];
            _philosophers[i] = new Philosopher(i + 1, left, right, config, Log, _startTime);
        }
    }

    public async Task StartSimulation()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        //HERE NOT USING TASK.RUN TO USE DIFFERENT THREADS, JUST KEEPING TASKS AND USE COROUTINE
        var philosopherTasks = _philosophers
            .Select(p => p.RunLifeCycle(token))
            .ToList();

        var monitorTask = MonitorRoutine(cts);

        await monitorTask;

        await Task.WhenAll(philosopherTasks);
    }

    private async Task MonitorRoutine(CancellationTokenSource cts)
    {
        while (!cts.Token.IsCancellationRequested)
        {
            long now = TimeUtils.NowMs();
            bool allSatisfied = true;

            foreach (var p in _philosophers)
            {
                if (now - p.LastMealTime > _config.TimeToDie)
                {
                    Log($"{p.Id} DIED! (Starvation)");
                    cts.Cancel();
                    return;
                }
                if (_config.TargetMeals.HasValue && p.MealsEaten < _config.TargetMeals)
                {
                    allSatisfied = false;
                }
            }

            if (_config.TargetMeals.HasValue && allSatisfied)
            {
                Log("Everyone ate enough! Simulation Success.");
                cts.Cancel();
                return;
            }
            //THIS IS NEEDED TO YIELD BACK CONTROL, OTHERWISE THE THREAD WOULD BE BUSY RUNNING THE MONITOR AND FREEZ THE OTHER TASKS
            await Task.Delay(20, cts.Token);
        }
    }
    private void Log(string msg)
    {
        var time = TimeUtils.NowMs() - _startTime;
        Console.WriteLine($"{time}ms: {msg}");
    }
}