namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record DifficultyCategories
{
    public Dictionary<string, object>? Aiming
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Boss
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Change
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Core
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Cover
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Grenade
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Hearing
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Lay
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Look
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Mind
    {
        get;
        set;
    } // TODO: string | number | boolean | string[]

    public Dictionary<string, object>? Move
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Patrol
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Scattering
    {
        get;
        set;
    } // TODO: string | number | boolean

    public Dictionary<string, object>? Shoot
    {
        get;
        set;
    } // TODO: string | number | boolean
}
