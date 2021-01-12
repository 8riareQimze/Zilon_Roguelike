﻿using System;
using System.Linq;

using Zilon.Core.Client;
using Zilon.Core.Common;
using Zilon.Core.PersonModules;
using Zilon.Core.Players;
using Zilon.Core.Props;
using Zilon.Core.StaticObjectModules;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;

namespace Zilon.Core.Commands.Sector
{
    public sealed class MineDepositCommand : ActorCommandBase
    {
        private readonly IMineDepositMethodRandomSource _mineDepositMethodRandomSource;
        private readonly IPlayer _player;

        public MineDepositCommand(
            IPlayer player,
            ISectorUiState playerState,
            IMineDepositMethodRandomSource mineDepositMethodRandomSource) : base(playerState)
        {
            _player = player;
            _mineDepositMethodRandomSource = mineDepositMethodRandomSource;
        }

        public override bool CanExecute()
        {
            var selectedViewModel = PlayerState.SelectedViewModel ?? PlayerState.HoverViewModel;
            var staticObject = (selectedViewModel as IContainerViewModel)?.StaticObject;
            if (staticObject is null)
            {
                return false;
            }

            var map = _player.SectorNode.Sector.Map;

            var currentNode = PlayerState.ActiveActor.Actor.Node;

            var distance = map.DistanceBetween(currentNode, staticObject.Node);
            if (distance > 1)
            {
                return false;
            }

            var targetDeposit = staticObject.GetModuleSafe<IPropDepositModule>();

            if (targetDeposit is null)
            {
                return false;
            }

            var equipmentCarrier = PlayerState.ActiveActor.Actor.Person.GetModuleSafe<IEquipmentModule>();
            if (equipmentCarrier is null)
            {
                return false;
            }

            var requiredTags = targetDeposit.GetToolTags();
            if (requiredTags.Any())
            {
                var equipedTool = GetEquipedTool(equipmentCarrier, requiredTags);
                if (equipedTool is null)
                {
                    return false;
                }

                return true;
            }

            // Если для добычи не указаны теги, то предполагается,
            // что добывать можно "руками".
            // То есть никакого инструмента не требуется.
            return true;
        }

        protected override void ExecuteTacticCommand()
        {
            var targetStaticObject = (PlayerState.SelectedViewModel as IContainerViewModel).StaticObject;
            var targetDeposit = targetStaticObject.GetModule<IPropDepositModule>();

            var equipmentCarrier = PlayerState.ActiveActor.Actor.Person.GetModule<IEquipmentModule>();
            var requiredTags = targetDeposit.GetToolTags();

            if (requiredTags.Any())
            {
                var equipedTool = GetEquipedTool(equipmentCarrier, requiredTags);
                if (equipedTool is null)
                {
                    throw new InvalidOperationException("Try to mine without required tools.");
                }

                var intetion = new Intention<MineTask>(actor =>
                    CreateTaskByInstrument(actor, targetStaticObject, equipedTool));
                PlayerState.TaskSource.Intent(intetion, PlayerState.ActiveActor.Actor);
            }
            else
            {
                // Добыча руками, если никаких тегов инструмента не задано.
                var intetion = new Intention<MineTask>(actor => CreateTaskByHands(actor, targetStaticObject));
                PlayerState.TaskSource.Intent(intetion, PlayerState.ActiveActor.Actor);
            }
        }

        private MineTask CreateTaskByHands(IActor actor, IStaticObject staticObject)
        {
            var handMineDepositMethod = new HandMineDepositMethod(_mineDepositMethodRandomSource);

            var taskContext = new ActorTaskContext(_player.SectorNode.Sector);
            return new MineTask(actor, taskContext, staticObject, handMineDepositMethod);
        }

        private MineTask CreateTaskByInstrument(IActor actor, IStaticObject staticObject, Equipment equipedTool)
        {
            var toolMineDepositMethod = new ToolMineDepositMethod(equipedTool, _mineDepositMethodRandomSource);

            var taskContext = new ActorTaskContext(_player.SectorNode.Sector);
            return new MineTask(actor, taskContext, staticObject, toolMineDepositMethod);
        }

        private static Equipment GetEquipedTool(IEquipmentModule equipmentModule, string[] requiredToolTags)
        {
            if (!requiredToolTags.Any())
            {
                // В этом методе предполагается, что наличие тегов проверено до вызова.
                throw new ArgumentException("Требуется не пустой набор тегов.", nameof(requiredToolTags));
            }

            foreach (var equipment in equipmentModule)
            {
                if (equipment is null)
                {
                    // Если для добычи указаны какие-либо теги, а ничего не экипировано,
                    // то такая экипировака не подходит.
                    continue;
                }

                var hasAllTags = EquipmentHelper.HasAllTags(equipment.Scheme.Tags, requiredToolTags);
                if (hasAllTags)
                {
                    // This equipment has all required tags.
                    return equipment;
                }
            }

            return null;
        }
    }
}