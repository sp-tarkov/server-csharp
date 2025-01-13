﻿using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class TraderAssortService
{
    private Dictionary<string, TraderAssort> _pristineTraderAssorts = new();
    
    public TraderAssort GetPristineTraderAssort(string traderId)
    {
        return _pristineTraderAssorts[traderId];
    }

    /// <summary>
    /// Store trader assorts inside a class property
    /// </summary>
    /// <param name="traderId">Traderid to store assorts against</param>
    /// <param name="assort">Assorts to store</param>
    public void SetPristineTraderAssort(string traderId, TraderAssort assort)
    {
        _pristineTraderAssorts[traderId] = assort;
    }
}
