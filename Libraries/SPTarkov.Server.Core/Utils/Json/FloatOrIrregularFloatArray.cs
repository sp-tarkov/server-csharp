namespace SPTarkov.Server.Core.Utils.Json;

public class FloatOrIrregularFloatArray(float? @float, float[][]? irregularFloatArray)
{
    private FloatOrIrregularFloatArray()
        : this(null, null) { }

    public float? Float { get; private set; } = @float;

    public float[][]? IrregularFloatArray { get; private set; } = irregularFloatArray;

    public bool IsFloat
    {
        get { return Float != null; }
    }

    public bool IsIrregularFloatArray
    {
        get { return IrregularFloatArray != null; }
    }
}
