namespace Types.Context;

public interface IApplicationContext
{
    ContextVariable? GetLatestValue(ContextVariableType type);
    ICollection<ContextVariable> GetValues(ContextVariableType type);
    void AddValue(ContextVariableType type, object value);
    void ClearValues(ContextVariableType type);
}