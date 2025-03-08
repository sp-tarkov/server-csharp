using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Context;

[Injectable(InjectionType.Singleton)]
public class ApplicationContext
{
    protected const short MaxSavedValues = 10;
    protected readonly Dictionary<ContextVariableType, LinkedList<ContextVariable>> variables = new();
    private readonly Lock _lockObject = new();

    public ContextVariable? GetLatestValue(ContextVariableType type)
    {
        lock (_lockObject)
        {
            if (variables.TryGetValue(type, out var savedValues))
            {
                return savedValues.Last!.Value;
            }
        }

        return null;
    }

    public ICollection<ContextVariable> GetValues(ContextVariableType type)
    {
        lock (_lockObject)
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
        lock (_lockObject)
        {
            if (!variables.TryGetValue(type, out var savedValues))
            {
                savedValues = [];
                variables.Add(type, savedValues);
            }

            if (savedValues.Count >= MaxSavedValues)
            {
                savedValues.RemoveFirst();
            }

            savedValues.AddLast(new ContextVariable(value, type));
        }
    }

    public void ClearValues(ContextVariableType type)
    {
        lock (_lockObject)
        {
            variables.Remove(type);
        }
    }
}
