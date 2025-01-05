using Types.Annotations;

namespace Types.Context;

[Injectable(InjectionType.Singleton)]
public class ApplicationContext
{
    private const short MaxSavedValues = 10;
    private readonly Dictionary<ContextVariableType, LinkedList<ContextVariable>> variables = new();
    private readonly object variablesLock = new();
    
    public ContextVariable? GetLatestValue(ContextVariableType type)
    {
        lock (variablesLock)
        {
            if (variables.TryGetValue(type, out var savedValues))
                return savedValues.Last!.Value;
        }

        return null;
    }

    public ICollection<ContextVariable> GetValues(ContextVariableType type)
    {
        lock (variablesLock)
        {
            var values = new List<ContextVariable>();
            if (variables.TryGetValue(type, out var savedValues))
            {
                values.AddRange(savedValues);
            }

            return values;
        }
    }

    public void AddValue(ContextVariableType type, object value)
    {
        lock (variablesLock)
        {
            if (!variables.TryGetValue(type, out var savedValues))
            {
                savedValues = [];
                variables.Add(type, savedValues);
            }

            if (savedValues.Count >= MaxSavedValues)
                savedValues.RemoveFirst();

            savedValues.AddLast(new ContextVariable(value, type));
        }
    }

    public void ClearValues(ContextVariableType type)
    {
        lock (variablesLock)
        {
            variables.Remove(type);
        }
    }
}