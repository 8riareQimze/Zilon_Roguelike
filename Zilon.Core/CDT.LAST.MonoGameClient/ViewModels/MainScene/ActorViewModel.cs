﻿using System;
using System.Diagnostics;
using System.Linq;

using CDT.LAST.MonoGameClient.Engine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using Zilon.Core.Client;
using Zilon.Core.Client.Sector;
using Zilon.Core.Common;
using Zilon.Core.Graphs;
using Zilon.Core.PersonModules;
using Zilon.Core.Persons;
using Zilon.Core.Schemes;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.ActorInteractionEvents;
using Zilon.Core.Tactics.Spatial;

namespace CDT.LAST.MonoGameClient.ViewModels.MainScene
{
    internal class ActorViewModel : GameObjectBase, IActorViewModel
    {
        private readonly Game _game;
        private readonly IActorGraphics _graphicsRoot;
        private readonly IPersonSoundContentStorage _personSoundStorage;

        private readonly SpriteContainer _rootSprite;
        private readonly SectorViewModelContext _sectorViewModelContext;
        private readonly Sprite _shadowSprite;
        private readonly SpriteBatch _spriteBatch;

        private IActorStateEngine _actorStateEngine;

        public ActorViewModel(Game game,
            IActor actor,
            SectorViewModelContext sectorViewModelContext,
            IPersonVisualizationContentStorage personVisualizationContentStorage,
            IPersonSoundContentStorage personSoundStorage,
            SpriteBatch spriteBatch)
        {
            _game = game;
            Actor = actor;
            _sectorViewModelContext = sectorViewModelContext;
            _personSoundStorage = personSoundStorage;
            _spriteBatch = spriteBatch;

            var equipmentModule = Actor.Person.GetModuleSafe<IEquipmentModule>();

            var shadowTexture = _game.Content.Load<Texture2D>("Sprites/game-objects/simple-object-shadow");

            _rootSprite = new SpriteContainer();
            _shadowSprite = new Sprite(shadowTexture)
            {
                Position = new Vector2(0, 0),
                Origin = new Vector2(0.5f, 0.5f),
                Color = new Color(Color.White, 0.5f)
            };

            _rootSprite.AddChild(_shadowSprite);

            var isHumanGraphics = Actor.Person is HumanPerson;
            if (isHumanGraphics)
            {
                var graphicsRoot = new HumanoidGraphics(Actor.Person.GetModule<IEquipmentModule>(),
                    personVisualizationContentStorage);

                _rootSprite.AddChild(graphicsRoot);

                _graphicsRoot = graphicsRoot;
            }
            else
            {
                var graphicsRoot = new AnimalGraphics(game.Content);

                _rootSprite.AddChild(graphicsRoot);

                _graphicsRoot = graphicsRoot;
            }

            var hexSize = MapMetrics.UnitSize / 2;
            var playerActorWorldCoords = HexHelper.ConvertToWorld(((HexNode)Actor.Node).OffsetCoords);
            var newPosition = new Vector2(
                (float)(playerActorWorldCoords[0] * hexSize * Math.Sqrt(3)),
                playerActorWorldCoords[1] * hexSize * 2 / 2
            );

            _rootSprite.Position = newPosition;

            //TODO Delete subscribtions after transition
            Actor.Moved += Actor_Moved;
            Actor.UsedAct += Actor_UsedAct;
            Actor.DamageTaken += Actor_DamageTaken;

            _actorStateEngine = new ActorIdleEngine(_graphicsRoot.RootSprite);
        }

        public override bool HiddenByFow => true;

        public override Vector2 HitEffectPosition => _graphicsRoot.HitEffectPosition;
        public override IGraphNode Node => Actor.Node;

        public override void Draw(GameTime gameTime, Matrix transform)
        {
            _spriteBatch.Begin(transformMatrix: transform);

            _rootSprite.Draw(_spriteBatch);

            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if (_actorStateEngine != null)
            {
                _actorStateEngine.Update(gameTime);
                if (_actorStateEngine.IsComplete)
                {
                    _actorStateEngine = new ActorIdleEngine(_graphicsRoot.RootSprite);

                    var hexSize = MapMetrics.UnitSize / 2;
                    var playerActorWorldCoords = HexHelper.ConvertToWorld(((HexNode)Actor.Node).OffsetCoords);
                    var newPosition = new Vector2(
                        (float)(playerActorWorldCoords[0] * hexSize * Math.Sqrt(3)),
                        playerActorWorldCoords[1] * hexSize * 2 / 2
                    );

                    _rootSprite.Position = newPosition;
                }
            }
        }

        private void Actor_DamageTaken(object? sender, DamageTakenEventArgs e)
        {
            if (sender is Actor actor)
            {
                if (actor.Person.CheckIsDead())
                {
                    var deathSoundEffect = _personSoundStorage.GetDeathEffect(actor.Person);
                    deathSoundEffect.CreateInstance().Play();
                }
            }
        }

        private void Actor_Moved(object? sender, EventArgs e)
        {
            var hexSize = MapMetrics.UnitSize / 2;
            var playerActorWorldCoords = HexHelper.ConvertToWorld(((HexNode)Actor.Node).OffsetCoords);
            var newPosition = new Vector2(
                (float)(playerActorWorldCoords[0] * hexSize * Math.Sqrt(3)),
                playerActorWorldCoords[1] * hexSize * 2 / 2
            );

            if (CanDraw)
            {
                var serviceScope = ((LivGame)_game).ServiceProvider;

                var animationBlockerService = serviceScope.GetRequiredService<IAnimationBlockerService>();

                var moveEngine = new ActorMoveEngine(
                    _rootSprite,
                    _graphicsRoot.RootSprite,
                    _shadowSprite,
                    newPosition,
                    animationBlockerService);
                _actorStateEngine = moveEngine;
            }
            else
            {
                _rootSprite.Position = newPosition;
            }
        }

        private void Actor_UsedAct(object? sender, UsedActEventArgs e)
        {
            var stats = e.TacticalAct.Stats;
            if (stats is null)
            {
                throw new InvalidOperationException("The act has no stats to select visualization.");
            }

            if (CanDraw)
            {
                if (stats.Effect == TacticalActEffectType.Damage && stats.IsMelee)
                {
                    var serviceScope = ((LivGame)_game).ServiceProvider;

                    var animationBlockerService = serviceScope.GetRequiredService<IAnimationBlockerService>();

                    var hexSize = MapMetrics.UnitSize / 2;
                    var playerActorWorldCoords = HexHelper.ConvertToWorld(((HexNode)e.TargetNode).OffsetCoords);
                    var newPosition = new Vector2(
                        (float)(playerActorWorldCoords[0] * hexSize * Math.Sqrt(3)),
                        playerActorWorldCoords[1] * hexSize * 2 / 2
                    );

                    var targetSpritePosition = newPosition;

                    var attackSoundEffectInstance = GetSoundEffect(e.TacticalAct.Stats);
                    _actorStateEngine =
                        new ActorMeleeAttackEngine(
                            _rootSprite,
                            targetSpritePosition,
                            animationBlockerService,
                            attackSoundEffectInstance);

                    var targetGameObject =
                        _sectorViewModelContext.GameObjects.SingleOrDefault(x => x.Node == e.TargetNode);
                    if (targetGameObject is null)
                    {
                        // This means the attacker is miss.
                        // This situation can be then the target actor moved before the attack reaches the target.
                    }
                    else
                    {
                        _sectorViewModelContext.EffectManager.HitEffects.Add(new HitEffect((LivGame)_game,
                            targetSpritePosition + targetGameObject.HitEffectPosition,
                            targetSpritePosition - _rootSprite.Position));
                    }
                }
            }
        }

        private SoundEffectInstance? GetSoundEffect(ITacticalActStatsSubScheme actStatScheme)
        {
            //TODO Move act description creating in event owner. Place this structure into event args.
            var usedActDescription =
                new ActDescription(actStatScheme.Tags?.Where(x => x != null)?.Select(x => x!)?.ToArray() ??
                                   Array.Empty<string>());

            var attackSoundEffect = _personSoundStorage.GetActStartSound(usedActDescription);

            var attackSoundEffectInstance = attackSoundEffect?.CreateInstance();
            return attackSoundEffectInstance;
        }

        public IActor Actor { get; set; }
        public object Item => Actor;
    }
}