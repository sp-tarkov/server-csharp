using System.Collections.Concurrent;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Context;

[Injectable(InjectionType.Singleton)]
public class ApplicationContext
{
    protected const short MaxSavedValues = 10;
    protected readonly ConcurrentDictionary<ContextVariableType, LinkedList<ContextVariable>> variables = new();

    private static ApplicationContext? _applicationContext;
    private readonly ISptLogger<ApplicationContext> _logger;

    /// <summary>
    /// When ApplicationContext gets created by the DI container we store the singleton reference so we can provide it
    /// statically for harmony patches!
    /// </summary>
    public ApplicationContext
    (
        ISptLogger<ApplicationContext> logger
    )
    {
        _logger = logger;
        _applicationContext = this;
    }

    public static ApplicationContext? GetInstance()
    {
        return _applicationContext;
    }

    public ContextVariable? GetLatestValue(ContextVariableType type)
    {
        if (variables.TryGetValue(type, out var savedValues))
        {
            return savedValues.Last!.Value;
        }

        return null;
    }

    public ICollection<ContextVariable> GetValues(ContextVariableType type)
    {
        var values = new List<ContextVariable>();
        if (variables.TryGetValue(type, out var savedValues))
        {
            values.AddRange(savedValues);
        }

        return values;
    }

    public void AddValue(ContextVariableType type, object value)
    {
        if (!variables.TryGetValue(type, out var savedValues))
        {
            savedValues = [];
            if (!variables.TryAdd(type, savedValues))
            {
                _logger.Error($"Unable to add context variable type: {type}");
            }
        }

        if (savedValues.Count >= MaxSavedValues)
        {
            savedValues.RemoveFirst();
        }

        savedValues.AddLast(new ContextVariable(value, type));
    }

    public void ClearValues(ContextVariableType type)
    {
        if (!variables.Remove(type, out _))
        {
            _logger.Error($"Unable to clear context variable type: {type}");
        }
    }
}
