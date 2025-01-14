namespace Core.Context;

public class ContextVariable
{
    private readonly object _value;
    private readonly ContextVariableType _internalType;
    private readonly DateTime _timestamp;

    public ContextVariable(object value, ContextVariableType contextVariableInternalType)
    {
        _value = value;
        _timestamp = DateTime.Now;
        _internalType = contextVariableInternalType;
    }

    public T GetValue<T>() {
        return (T)_value;
    }

    public DateTime GetTimestamp()
    {
        return _timestamp;
    }

    public ContextVariableType GetContextType()
    {
        return _internalType;
    }
}
