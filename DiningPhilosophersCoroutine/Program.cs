using DiningPhilosophersCoroutine;

var config = new Settings(
           N: 5,
           TimeToDie: 1000,
           TimeToEat: 150,
           TimeToSleep: 150,
           TargetMeals: 10
       );

Console.WriteLine($"=== Async Coroutine Dining Philosophers ({config.N} nodes) ===");
Console.WriteLine($"[Main Thread ID: {Environment.CurrentManagedThreadId}] All tasks will originate here.");

var table = new Table(config);
await table.StartSimulation();

Console.WriteLine("Simulation ended.");
