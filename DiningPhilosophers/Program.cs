using DiningPhilosophers;

int n = 5;
int timeToDieMs = 1200;
int timeToEatMs = 200;
int timeToSleepMs = 200;
int targetMeals = 5;

var table = new Table(n, timeToDieMs, timeToEatMs, timeToSleepMs, targetMeals);
await table.StartAsync();
