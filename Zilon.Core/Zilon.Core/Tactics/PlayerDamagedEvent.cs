﻿using Zilon.Core.Persons;
using Zilon.Core.Scoring;

namespace Zilon.Core.Tactics
{
    public sealed class PlayerDamagedEvent : IPlayerEvent
    {
        public PlayerDamagedEvent(ITacticalAct tacticalAct, IActor damager)
        {
            TacticalAct = tacticalAct ?? throw new ArgumentNullException(nameof(tacticalAct));
            Damager = damager ?? throw new ArgumentNullException(nameof(damager));
        }

        public IActor Damager { get; }

        public ITacticalAct TacticalAct { get; }

        public string Key => $"{Damager.Person}-{TacticalAct}";

        public int Weight => TacticalAct.Efficient.Dice * TacticalAct.Efficient.Count;
    }
}