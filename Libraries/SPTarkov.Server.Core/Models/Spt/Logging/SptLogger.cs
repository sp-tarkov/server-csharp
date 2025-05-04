namespace SPTarkov.Server.Core.Models.Spt.Logging;

public record SptLogger
{
    public object Error { get; set; } // error: (msg: string | Record<string, unknown>) => void;

    public object Warn { get; set; } // warn: (msg: string | Record<string, unknown>) => void;

    public object Succ { get; set; } // succ?: (msg: string | Record<string, unknown>) => void;

    public object Info { get; set; } // info: (msg: string | Record<string, unknown>) => void;

    public object Debug { get; set; } // debug: (msg: string | Record<string, unknown>) => void;
}

// TODO: needs to be reimplemented however we want to do it in this project
