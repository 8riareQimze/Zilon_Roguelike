﻿
using System;

using CDT.LIV.MonoGameClient.Engine;

using Microsoft.Xna.Framework;

using Zilon.Core.Client.Sector;

namespace CDT.LIV.MonoGameClient.ViewModels.MainScene
{
    public sealed class ActorMeleeAttackEngine : IActorStateEngine
    {
        private const double ANIMATION_DURATION_SECONDS = 0.5;

        private double _animationCounterSeconds = ANIMATION_DURATION_SECONDS;

        private readonly Container _rootContainer;
        private readonly IAnimationBlockerService _animationBlockerService;
        private readonly ICommandBlocker _animationBlocker;

        private Vector2 _startPosition;
        private Vector2 _targetPosition;

        public ActorMeleeAttackEngine(Container rootContainer, Vector2 targetPosition, IAnimationBlockerService animationBlockerService)
        {
            _rootContainer = rootContainer;
            _animationBlockerService = animationBlockerService;

            _startPosition = rootContainer.Position;
            _targetPosition = Vector2.Lerp(_startPosition, targetPosition, 0.6f);

            _animationBlocker = new AnimationCommonBlocker();

            _animationBlockerService.AddBlocker(_animationBlocker);

            _rootContainer.FlipX = (_startPosition - _targetPosition).X < 0;
        }

        public bool IsComplete { get; private set; }

        public void Update(GameTime gameTime)
        {
            _animationCounterSeconds -= gameTime.ElapsedGameTime.TotalSeconds * 3;
            var t = 1 - _animationCounterSeconds / ANIMATION_DURATION_SECONDS;

            if (_animationCounterSeconds > 0)
            {
                var t2 = Math.Sin(t * Math.PI);
                _rootContainer.Position = Vector2.Lerp(_startPosition, _targetPosition, (float)t2);
            }
            else
            {
                _rootContainer.Position = _startPosition;
                _animationBlocker.Release();
                IsComplete = true;
            }
        }
    }
}