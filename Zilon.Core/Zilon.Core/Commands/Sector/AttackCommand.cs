﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Zilon.Core.Client;
using Zilon.Core.Components;
using Zilon.Core.Persons;
using Zilon.Core.Props;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Core.Commands
{
    /// <inheritdoc />
    /// <summary>
    /// Команда на перемещение взвода в указанный узел карты.
    /// </summary>
    public class AttackCommand : ActorCommandBase
    {
        private readonly ITacticalActUsageService _tacticalActUsageService;

        [ExcludeFromCodeCoverage]
        public AttackCommand(IGameLoop gameLoop,
            ISectorManager sectorManager,
            ISectorUiState playerState,
            ITacticalActUsageService tacticalActUsageService) :
            base(gameLoop, sectorManager, playerState)
        {
            _tacticalActUsageService = tacticalActUsageService;
        }

        public override bool CanExecute()
        {
            var map = SectorManager.CurrentSector.Map;

            var currentNode = PlayerState.ActiveActor.Actor.Node;

            var selectedActorViewModel = GetCanExecuteActorViewModel();
            if (selectedActorViewModel == null)
            {
                return false;
            }

            var targetNode = selectedActorViewModel.Actor.Node;

            var act = PlayerState.TacticalAct;
            if ((act.Stats.Targets & TacticalActTargets.Self) > 0 &&
                PlayerState.ActiveActor.Actor == selectedActorViewModel.Actor)
            {
                return true;
            }
            else
            {
                // Проверка, что цель достаточно близка по дистации и видна.
                if (act.Stats.Range == null)
                {
                    return false;
                }

                var isInDistance = act.CheckDistance(currentNode, targetNode, SectorManager.CurrentSector.Map);
                if (!isInDistance)
                {
                    return false;
                }

                var targetIsOnLine = map.TargetIsOnLine(currentNode, targetNode);
                if (!targetIsOnLine)
                {
                    return false;
                }

                // Проверка наличия ресурсов для выполнения действия

                if (act.Constrains?.PropResourceType != null && act.Constrains?.PropResourceCount != null)
                {
                    var hasPropResource = CheckPropResource(PlayerState.ActiveActor.Actor.Person.Inventory,
                        act.Constrains.PropResourceType,
                        act.Constrains.PropResourceCount.Value);

                    if (!hasPropResource)
                    {
                        return false;
                    }
                }

                // Проверка КД

                if (act.CurrentCooldown > 0)
                {
                    return false;
                }
            }


            return true;
        }

        private bool CheckPropResource(IPropStore inventory,
            string usedPropResourceType,
            int usedPropResourceCount)
        {
            var props = inventory.CalcActualItems();
            var propResources = new List<Resource>();
            foreach (var prop in props)
            {
                var propResource = prop as Resource;
                if (propResource == null)
                {
                    continue;
                }

                if (propResource.Scheme.Bullet?.Caliber == usedPropResourceType)
                {
                    propResources.Add(propResource);
                }
            }

            var preferredPropResource = propResources.FirstOrDefault();

            return preferredPropResource != null && preferredPropResource.Count >= usedPropResourceCount;
        }

        private IActorViewModel GetCanExecuteActorViewModel()
        {
            var hover = PlayerState.HoverViewModel as IActorViewModel;
            var selected = PlayerState.SelectedViewModel as IActorViewModel;
            return hover ?? selected;
        }

        protected override void ExecuteTacticCommand()
        {
            var targetActorViewModel = (IActorViewModel)PlayerState.SelectedViewModel;
            var targetActor = targetActorViewModel.Actor;

            var tacticalAct = PlayerState.TacticalAct;

            var intention = new Intention<AttackTask>(a => new AttackTask(a, targetActor, tacticalAct, _tacticalActUsageService));
            PlayerState.TaskSource.Intent(intention);
        }
    }
}