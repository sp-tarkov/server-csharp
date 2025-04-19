namespace SPTarkov.Server.Core.Models.Enums
{
    public enum DamageType
    {
        Undefined = 1,
        Fall = 2,
        Explosion = 4,
        Barbed = 8,
        Flame = 16,
        GrenadeFragment = 32,
        Impact = 64,
        Existence = 128,
        Medicine = 256,
        Bullet = 512,
        Melee = 1024,
        Landmine = 2048,
        Sniper = 4096,
        Blunt = 8192,
        LightBleeding = 16384,
        HeavyBleeding = 32768,
        Dehydration = 65536,
        Exhaustion = 131072,
        RadExposure = 262144,
        Stimulator = 524288,
        Poison = 1048576,
        LethalToxin = 2097152,
        Btr = 4194304,
        Artillery = 8388608,
        Environment = 16777216
    }
}
