namespace DiningPhilosophers;

public sealed class Philosopher(int id, Fork left, Fork right)
{
    public int Id { get; } = id;
    public Fork Left { get; } = left;
    public Fork Right { get; } = right;
}