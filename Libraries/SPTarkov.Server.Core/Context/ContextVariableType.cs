namespace SPTarkov.Server.Core.Context;

public enum ContextVariableType
{
    // Logged-in users session id
    SESSION_ID = 0,

    // Currently active raid information
    RAID_CONFIGURATION = 1,

    // SessionID + Timestamp when client first connected, has _ between values
    CLIENT_START_TIMESTAMP = 2,

    // When player is loading into map and loot is requested
    REGISTER_PLAYER_REQUEST = 3,
    RAID_ADJUSTMENTS = 4,

    // Data returned from client request object from endLocalRaid()
    TRANSIT_INFO = 5,
    APP_BUILDER = 6,
    LOADED_MOD_ASSEMBLIES = 7,
    WEB_APPLICATION = 8
}
