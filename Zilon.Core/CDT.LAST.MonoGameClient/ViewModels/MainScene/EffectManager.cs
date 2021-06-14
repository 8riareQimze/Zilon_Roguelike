﻿using System.Collections.Generic;

namespace CDT.LAST.MonoGameClient.ViewModels.MainScene
{
    public sealed class EffectManager
    {
        public EffectManager()
        {
            HitEffects = new List<HitEffect>();
        }

        public List<HitEffect> HitEffects { get; }
    }
}