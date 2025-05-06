namespace SPTarkov.DI;

public class SingletonStateHolder<T>
{
    public T State
    {
        get;
    }

    public SingletonStateHolder(T state)
    {
        State = state;
    }
}
