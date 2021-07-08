﻿using System;
using System.Linq;

using CDT.LAST.MonoGameClient.Screens;
using CDT.LAST.MonoGameClient.ViewModels.MainScene.GameObjectVisualization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Zilon.Core.Client;
using Zilon.Core.Commands;
using Zilon.Core.Persons;
using Zilon.Core.Players;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.ActorInteractionEvents;
using Zilon.Core.World;

namespace CDT.LAST.MonoGameClient.ViewModels.MainScene
{
    public sealed class SectorViewModel
    {
        private readonly Camera _camera;
        private readonly CommandInput _commandInput;
        private readonly GameObjectsViewModel _gameObjectsViewModel;
        private readonly IActorInteractionBus _intarectionBus;

        private readonly MapViewModel _mapViewModel;
        private readonly IPersonSoundContentStorage _personSoundContentStorage;
        private readonly IPlayer _player;
        private readonly SpriteBatch _spriteBatch;
        private readonly ISectorUiState _uiState;
        private readonly SectorViewModelContext _viewModelContext;

        public SectorViewModel(Game game, Camera camera, SpriteBatch spriteBatch)
        {
            _camera = camera;
            _spriteBatch = spriteBatch;

            var serviceScope = ((LivGame)game).ServiceProvider;

            _player = serviceScope.GetRequiredService<IPlayer>();
            _uiState = serviceScope.GetRequiredService<ISectorUiState>();

            _intarectionBus = serviceScope.GetRequiredService<IActorInteractionBus>();

            _intarectionBus.NewEvent += IntarectionBus_NewEvent;

            var personVisualizationContentStorage =
                serviceScope.GetRequiredService<IPersonVisualizationContentStorage>();
            _personSoundContentStorage = serviceScope.GetRequiredService<IPersonSoundContentStorage>();
            var gameObjectVisualizationContentStorage =
                serviceScope.GetRequiredService<IGameObjectVisualizationContentStorage>();

            var sector = GetPlayerSectorNode(_player).Sector;

            if (sector is null)
            {
                throw new InvalidOperationException();
            }

            Sector = sector;

            var playerActor = (from actor in Sector.ActorManager.Items
                               where actor.Person == _player.MainPerson
                               select actor).SingleOrDefault();

            _mapViewModel = new MapViewModel(game, _player, _uiState, Sector, spriteBatch);

            _viewModelContext = new SectorViewModelContext(sector);

            var gameObjectParams = new GameObjectParams
            {
                Game = game,
                Camera = camera,
                UiState = _uiState,
                Player = _player,
                SpriteBatch = _spriteBatch,
                SectorViewModelContext = _viewModelContext,
                PersonSoundStorage = _personSoundContentStorage,
                PersonVisualizationContentStorage = personVisualizationContentStorage,
                GameObjectVisualizationContentStorage = gameObjectVisualizationContentStorage
            };
            _gameObjectsViewModel = new GameObjectsViewModel(gameObjectParams);

            var commandFactory = new ServiceProviderCommandFactory(((LivGame)game).ServiceProvider);

            var commandPool = serviceScope.GetRequiredService<ICommandPool>();
            var commandInput =
                new CommandInput(_uiState, commandPool, _camera, Sector, _viewModelContext, commandFactory);
            _commandInput = commandInput;
        }

        public ISector Sector { get; }

        public void Draw(GameTime gameTime)
        {
            _mapViewModel.Draw(_camera.Transform);

            if (_player.MainPerson is null)
            {
                throw new InvalidOperationException();
            }

            DrawCorpses();

            _gameObjectsViewModel.Draw(gameTime);

            _spriteBatch.Begin(transformMatrix: _camera.Transform);

            foreach (var visualEffect in _viewModelContext.EffectManager.VisualEffects.ToArray())
            {
                visualEffect.Draw(_spriteBatch);
            }

            _spriteBatch.End();
        }

        public void UnsubscribeEventHandlers()
        {
            _intarectionBus.NewEvent -= IntarectionBus_NewEvent;
            _gameObjectsViewModel.UnsubscribeEventHandlers();
        }

        public void Update(GameTime gameTime)
        {
            var mainPerson = _player.MainPerson;
            if (mainPerson is null)
            {
                throw new InvalidOperationException();
            }

            _mapViewModel.Update(gameTime);

            _gameObjectsViewModel.Update(gameTime);

            _viewModelContext.CorpseManager.Update(gameTime);

            foreach (var hitEffect in _viewModelContext.EffectManager.VisualEffects.ToArray())
            {
                hitEffect.Update(gameTime);

                if (hitEffect.IsComplete)
                {
                    _viewModelContext.EffectManager.VisualEffects.Remove(hitEffect);
                }
            }

            _commandInput.Update(_viewModelContext);
        }

        private void DrawCorpses()
        {
            _spriteBatch.Begin(transformMatrix: _camera.Transform);

            _viewModelContext.CorpseManager.Draw(_spriteBatch);

            _spriteBatch.End();
        }

        private static ISectorNode GetPlayerSectorNode(IPlayer player)
        {
            if (player.Globe is null)
            {
                throw new InvalidOperationException();
            }

            return (from sectorNode in player.Globe.SectorNodes
                    let sector = sectorNode.Sector
                    where sector != null
                    from actor in sector.ActorManager.Items
                    where actor.Person == player.MainPerson
                    select sectorNode).Single();
        }

        private void IntarectionBus_NewEvent(object? sender, ActorInteractionEventArgs e)
        {
            if (e.ActorInteractionEvent is DamageActorInteractionEvent damageActorInteractionEvent)
            {
                var actDescription = damageActorInteractionEvent.UsedActDescription;
                var targetActor = damageActorInteractionEvent.TargetActor;
                var targetPerson = targetActor.Person;

                var attackerViewModel = _viewModelContext.GameObjects.OfType<ActorViewModel>()
                    .Single(x => x.Actor == damageActorInteractionEvent.Actor);
                if (attackerViewModel.CanDraw)
                {
                    /*var soundEffect = _personSoundContentStorage.GetActHitSound(actDescription, targetPerson);
                    soundEffect.CreateInstance().Play();*/

                    attackerViewModel.RunCombatActUsageAnimation(actDescription, targetActor.Node);
                }

                var targetViewModel = _viewModelContext.GameObjects.OfType<ActorViewModel>()
                    .Single(x => x.Actor == targetActor);
                if (targetViewModel.CanDraw)
                {
                    var targetPersonIsStillAlive = !targetActor.Person.CheckIsDead();
                    if (targetPersonIsStillAlive)
                    {
                        targetViewModel.RunHitAnimation();
                    }
                    else
                    {
                        // Do not animate hit for dead persons.
                        // Because if the person is dead the actor of the person removes.
                        // It lead to some unexpected cases:
                        // - You can't see animation of the actor that removed.
                        // - It create a animation blocker that can't update beacause removed actor will not call update.

                        targetViewModel.RunDeathAnimation();
                    }
                }
            }
        }
    }
}