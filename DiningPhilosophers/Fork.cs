namespace DiningPhilosophers;

public sealed class Fork(int id)
{
    public int Id { get; } = id;
    public SemaphoreSlim Available { get; } = new SemaphoreSlim(1, 1);
}