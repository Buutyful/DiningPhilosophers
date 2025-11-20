namespace DiningPhilosophersCoroutine;
public record Settings(
    int N,
    int TimeToDie,
    int TimeToEat,
    int TimeToSleep,
    int? TargetMeals = null
);
