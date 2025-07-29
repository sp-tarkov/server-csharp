namespace SPTarkov.DI;

public class SingletonStateHolder<T>(T state)
{
    public T State { get; } = state;
}
