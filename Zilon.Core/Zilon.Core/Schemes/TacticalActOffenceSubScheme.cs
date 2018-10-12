﻿using Newtonsoft.Json;

using Zilon.Core.Components;

namespace Zilon.Core.Schemes
{
    public class TacticalActOffenceSubScheme : SubSchemeBase, ITacticalActOffenceSubScheme
    {
        [JsonProperty]
        public OffenseType Type { get; private set; }

        [JsonProperty]
        public TacticalActImpactType Impact { get; private set; }

        [JsonProperty]
        public int ApRank { get; private set; }
    }
}
