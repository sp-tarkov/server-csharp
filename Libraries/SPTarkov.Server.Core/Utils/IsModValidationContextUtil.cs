namespace SPTarkov.Server.Core.Utils;

/// <summary>
/// Initialize manually and call <see cref="SetModValidationContext"/> before registering as a service.
/// Can be used to i.e. control logging depending on the context.
/// </summary>
public class IsModValidationContextUtil
{
    private bool _isModValidationContext;

    public void SetModValidationContext(bool isModValidationContext = true)
    {
        _isModValidationContext = isModValidationContext;
    }

    public bool IsModValidationContext()
    {
        return _isModValidationContext;
    }
}
