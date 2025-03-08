namespace SPTarkov.Server.Core.Utils.Cloners;

public interface ICloner
{
    public T? Clone<T>(T? obj);
}
