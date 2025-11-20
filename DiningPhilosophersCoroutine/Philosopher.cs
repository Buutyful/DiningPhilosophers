using Shared;

namespace DiningPhilosophersCoroutine;

public class Philosopher
{
    public int Id { get; }
    public int MealsEaten { get; private set; } = 0;
    public long LastMealTime { get; private set; }

    private readonly SemaphoreSlim _leftFork;
    private readonly SemaphoreSlim _rightFork;
    private readonly Settings _config;
    private readonly Action<string> _logger;
    private readonly bool _pickLeftFirst;

    public Philosopher(
        int id,
        SemaphoreSlim left,
        SemaphoreSlim right,
        Settings config,
        Action<string> logger,
        long startTime)
    {
        Id = id;
        _leftFork = left;
        _rightFork = right;
        _config = config;
        _logger = logger;
        LastMealTime = startTime;
        _pickLeftFirst = id % 2 == 0;
    }
    public async Task RunLifeCycle(CancellationToken token)
    {
        try
        {
            if (Id % 2 != 0) await Task.Delay(10, token);

            while (!token.IsCancellationRequested)
            {
                var first = _pickLeftFirst ? _leftFork : _rightFork;
                var second = _pickLeftFirst ? _rightFork : _leftFork;

                await first.WaitAsync(token);
                _logger($"{Id} has taken a fork");

                await second.WaitAsync(token);
                _logger($"{Id} has taken a fork");

                LastMealTime = TimeUtils.NowMs();
                _logger($"{Id} is eating");

                await Task.Delay(_config.TimeToEat, token);

                MealsEaten++;

                second.Release();
                first.Release();

                _logger($"{Id} is sleeping");

                await Task.Delay(_config.TimeToSleep, token);
                _logger($"{Id} is thinking");
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }
}