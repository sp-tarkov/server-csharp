namespace Core.DI;

public interface OnLoad
{
    Task OnLoad();
    string GetRoute();
}
