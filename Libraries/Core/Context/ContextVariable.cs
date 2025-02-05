namespace Core.Context;

public class ContextVariable(object value, ContextVariableType contextVariableInternalType)
{
    private readonly DateTime _timestamp = DateTime.UtcNow;

    public T GetValue<T>()
    {
        return (T)value;
    }

    public DateTime GetTimestamp()
    {
        return _timestamp;
    }

    public ContextVariableType GetContextType()
    {
        return contextVariableInternalType;
    }
}
