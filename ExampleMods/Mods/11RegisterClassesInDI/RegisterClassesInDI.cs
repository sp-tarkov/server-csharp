using Core.Models.External;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace ExampleMods.Mods._11RegisterClassesInDI;

[Injectable]
public class Bundle : IPostDBLoadMod // Run after db has loaded
{
    private readonly SingletonClassExample _singletonClassExample;
    private readonly TransientClassExample _transientClassExample;

    // We inject 2 classes (singleton and transient) we've made below
    public Bundle(
        SingletonClassExample singletonClassExample,
        TransientClassExample transientClassExample)
    {
        _singletonClassExample = singletonClassExample;
        _transientClassExample = transientClassExample;
    }

    public void PostDBLoad()
    {
        _singletonClassExample.IncrementCounterAndLog();
        _singletonClassExample.IncrementCounterAndLog();
        _singletonClassExample.IncrementCounterAndLog();

        _transientClassExample.IncrementCounterAndLog();
        _transientClassExample.IncrementCounterAndLog();
        _transientClassExample.IncrementCounterAndLog();
    }
}

// This class is registered as a singleton. This means ONE and only ONE instance
// of this class will ever exist.
[Injectable(InjectionType.Singleton)]
public class SingletonClassExample
{
    private readonly ISptLogger<SingletonClassExample> _logger;
    private int _counter;

    public SingletonClassExample(
        ISptLogger<SingletonClassExample> logger)
    {
        _logger = logger;
        _counter = 0;
    }

    public void IncrementCounterAndLog()
    {
        _counter++;
        _logger.Success($"{_counter}");
    }
}

// This class is being registered as default or transient. This means that
// every time a class requests an instance of this type a new one will be created
[Injectable(InjectionType.Transient)] // [Injectable] is the same as doing this
public class TransientClassExample
{
    private readonly ISptLogger<TransientClassExample> _logger;
    private int _counter;

    public TransientClassExample(
        ISptLogger<TransientClassExample> logger)
    {
        _logger = logger;
        _counter = 0;
    }

    public void IncrementCounterAndLog()
    {
        _counter++;
        _logger.Success($"{_counter}");
    }
}
