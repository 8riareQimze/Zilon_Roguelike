﻿namespace Zilon.Core.Schemes
{
    public interface IMonsterScheme: IScheme
    {
        IMonsterDefenceSubScheme Defence { get; }
        string[] DropTableSids { get; }
        int Hp { get; }
        ITacticalActStatsSubScheme PrimaryAct { get; }
    }
}