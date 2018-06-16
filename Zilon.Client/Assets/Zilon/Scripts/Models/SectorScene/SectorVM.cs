﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Zilon.Scripts.Models.CombatScene;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Zilon.Core.Commands;
using Zilon.Core.Common;
using Zilon.Core.Persons;
using Zilon.Core.Services.CombatEvents;
using Zilon.Core.Services.CombatMap;
using Zilon.Core.Services.MapGenerators;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;
using Zilon.Core.Tactics.Spatial;

class SectorVM : MonoBehaviour
{

    private HumanActorTaskSource _playerActorTaskSource;
    private Sector _sector;
    
    private float turnCounter;

    public SchemeLocator SchemeLocator;
    public Text Text;
    
    public MapNodeVM MapNodePrefab;
    public ActorVM ActorPrefab;

    [Inject]
    private ICommandManager _commandManager;
//    [Inject]
//    private ICombatService _combatService;
    [Inject]
    private ICombatManager _combatManager;
    [Inject]
    private IEventManager _eventManager;
    [Inject]
    private IMapGenerator _mapGenerator;

//    [Inject(Id = "squad-command-factory")]
//    private ICommandFactory _commandFactory;

    private void FixedUpdate()
    {
//        ExecuteCommands();
//        UpdateEvents();
//        UpdateTurnCounter();
    }

//    private void UpdateTurnCounter()
//    {
//        turnCounter += Time.deltaTime;
//        if (turnCounter < 10)
//        {
//            return;
//        }
//
//        turnCounter = 0;
//
//        var endTurnCommand = _commandFactory.CreateCommand<EndTurnCommand>();
//        _commandManager.Push(endTurnCommand);
//    }

    private void UpdateEvents()
    {
       // _eventManager.Update();
    }

    private void ExecuteCommands()
    {
//        var command = _commandManager?.Pop();
//        if (command == null)
//            return;
//
//        Debug.Log($"Executing {command}");
//
//        command.Execute();
    }

    private void Awake()
    {
//        var initData = CombatHelper.GetData(_mapGenerator);
//        var combat = _combatService.CreateCombat(initData);
//        _combatManager.CurrentCombat = combat;
        
        var mapGenerator = new GridMapGenerator();
        var map = new Map();
        mapGenerator.CreateMap(map);
        var sector = new Sector(map);

        var nodeVMs = new List<MapNodeVM>();
        foreach (var node in map.Nodes)
        {
            
            var mapNodeVM = Instantiate(MapNodePrefab, transform);

            var nodeWorldPositionParts = HexHelper.ConvertToWorld(node.OffsetX, node.OffsetY);
            var worldPosition = new Vector3(nodeWorldPositionParts[0], nodeWorldPositionParts[1]);
            mapNodeVM.transform.position = worldPosition;
            mapNodeVM.Node = node;
            
            mapNodeVM.OnSelect+= MapNodeVmOnOnSelect;

            nodeVMs.Add(mapNodeVM);
        }

        var playerPerson = new Person();
        
        var playerActor = sector.AddActor(playerPerson, map.Nodes.First());

        var playerActorVM = Instantiate(ActorPrefab, transform);

        var actorNodeVm = nodeVMs.Single(x => x.Node == playerActor.Node);
        var actorPosition = actorNodeVm.transform.position;
        playerActorVM.transform.position = actorPosition;
        playerActorVM.Actor = playerActor;

        _playerActorTaskSource = new HumanActorTaskSource(playerActor);
        sector.BehaviourSources = new IActorTaskSource[] { _playerActorTaskSource };

        _sector = sector;

        //Map.InitCombat();
    }

    private void MapNodeVmOnOnSelect(object sender, EventArgs e)
    {
        // указываем намерение двигиться на выбранную точку (узел).
        
        var nodeVM = sender as MapNodeVM;

        if (nodeVM != null)
        {
            var targetNode = nodeVM.Node;
            _playerActorTaskSource.AssignMoveToPointCommand(targetNode);
            _sector.Update();
        }
    }
}
