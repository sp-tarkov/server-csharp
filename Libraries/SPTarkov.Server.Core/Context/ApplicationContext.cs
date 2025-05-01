using System.Collections.Concurrent;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Context;

[Injectable(InjectionType.Singleton)]
public class ApplicationContext
{
    private const short MaxSavedValues = 10;

    private static ApplicationContext? _applicationContext;
    private readonly ISptLogger<ApplicationContext> _logger;
    private readonly ConcurrentDictionary<ContextVariableType, (Lock lockObject, LinkedList<ContextVariable> values)> _variables = new();

    /// <summary>
    ///     When ApplicationContext gets created by the DI container we store the singleton reference so we can provide it
    ///     statically for harmony patches!
    /// </summary>
    public ApplicationContext(ISptLogger<ApplicationContext> logger)
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
        if (_variables.TryGetValue(type, out var savedValues))
        {
            lock (savedValues.lockObject)
            {
                return savedValues.values.Last!.Value;
            }
        }

        return null;
    }

    public ICollection<ContextVariable> GetValues(ContextVariableType type)
    {
        var values = new List<ContextVariable>();
        if (_variables.TryGetValue(type, out var savedValues))
        {
            lock (savedValues.lockObject)
            {
                values.AddRange(savedValues.Item2);
            }
        }

        return values;
    }

    public void AddValue(ContextVariableType type, object value)
    {
        if (!_variables.TryGetValue(type, out var savedValues))
        {
            savedValues = new ValueTuple<Lock, LinkedList<ContextVariable>>();
            savedValues.lockObject = new Lock();
            savedValues.values = [];
            if (!_variables.TryAdd(type, savedValues))
            {
                _logger.Error($"Unable to add context variable type: {type}");
            }
        }

        lock (savedValues.lockObject)
        {
            if (savedValues.values.Count >= MaxSavedValues)
            {
                savedValues.values.RemoveFirst();
            }
            savedValues.values.AddLast(new ContextVariable(value, type));
        }
    }

    public void ClearValues(ContextVariableType type)
    {
        if (!_variables.Remove(type, out _))
        {
            _logger.Error($"Unable to clear context variable type: {type}");
        }
    }
}
