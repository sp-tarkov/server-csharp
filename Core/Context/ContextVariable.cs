namespace Core.Context;

public class ContextVariable(object value, ContextVariableType contextVariableType)
{
    public object Value { get; } = value; 
    public DateTime Timestamp { get; } = DateTime.Now;
    public ContextVariableType Type { get; } = contextVariableType;
}