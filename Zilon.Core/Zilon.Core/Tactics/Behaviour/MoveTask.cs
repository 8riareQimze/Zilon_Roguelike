﻿using System.Collections.Generic;
using System.Linq;
using Zilon.Core.Common;
using Zilon.Core.Tactics.Spatial;
using Zilon.Core.Tactics.Spatial.PathFinding;

namespace Zilon.Core.Tactics.Behaviour
{
    public class MoveTask: IActorTask
    {
        private readonly HexNode _targetNode;
        private readonly IHexMap _map;
        private readonly List<HexNode> _path;
        
        public IActor Actor { get; }
        public void Execute()
        {
            if (!_path.Any())
            {
                IsComplete = true;
                return;
            }

            var nextNode = _path[0];
            Actor.MoveToNode(nextNode);
            _path.RemoveAt(0);

            if (!_path.Any())
            {
                IsComplete = true;
                return;
            }
        }

        public bool IsComplete { get; set; }

        public MoveTask(IActor actor, HexNode targetNode, IHexMap map)
        {
            Actor = actor;
            _targetNode = targetNode;
            _map = map;

            _path = new List<HexNode>();

            CreatePath();
        }

        private void CreatePath()
        {
            var startNode = Actor.Node;
            var finishNode = _targetNode;
            
            _path.Clear();

            var astar = new AStar((IMap<IMapNode, IEdge>)_map, startNode, finishNode);
            var resultState = astar.Run();
            if (resultState == State.GoalFound)
            {
                var foundPath = astar.GetPath();
                foreach (var pathNode in foundPath)
                {
                    _path.Add((HexNode)pathNode);
                }
            }
        }
    }
}