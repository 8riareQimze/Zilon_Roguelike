﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Zilon.Core.MapGenerators.CellularAutomatonStyle;
using Zilon.Core.MapGenerators.StaticObjectFactories;
using Zilon.Core.Schemes;
using Zilon.Core.StaticObjectModules;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Core.MapGenerators
{
    public sealed class StaticObstaclesGenerator : IStaticObstaclesGenerator
    {
        private readonly IChestGenerator _chestGenerator;
        private readonly IInteriorObjectRandomSource _interiorObjectRandomSource;
        private readonly IStaticObjectfactoryCollector _staticObjectfactoryCollector;

        public StaticObstaclesGenerator(IChestGenerator chestGenerator,
            IInteriorObjectRandomSource interiorObjectRandomSource,
            IStaticObjectfactoryCollector staticObjectfactoryCollector)
        {
            _chestGenerator = chestGenerator ?? throw new ArgumentNullException(nameof(chestGenerator));
            _interiorObjectRandomSource = interiorObjectRandomSource ?? throw new ArgumentNullException(nameof(interiorObjectRandomSource));
            _staticObjectfactoryCollector = staticObjectfactoryCollector ?? throw new ArgumentNullException(nameof(staticObjectfactoryCollector));
        }

        public Task CreateAsync(ISector sector, ISectorSubScheme sectorSubScheme)
        {
            if (sector is null)
            {
                throw new ArgumentNullException(nameof(sector));
            }

            // Генерация препятсвий, как статических объектов.
            foreach (var region in sector.Map.Regions)
            {
                var regionNodes = region.Nodes.Cast<HexNode>().ToArray();
                var regionCoords = regionNodes.Select(x => x.OffsetCoords).ToArray();
                var interiorMetas = _interiorObjectRandomSource.RollInteriorObjects(regionCoords);

                foreach (var interior in interiorMetas)
                {
                    var node = regionNodes.Single(x => x.OffsetCoords == interior.Coords);
                    var staticObject = CreateStaticObject(sector, node);

                    sector.StaticObjectManager.Add(staticObject);
                }
            }

            _chestGenerator.CreateChests(sector, sectorSubScheme, sector.Map.Regions);

            return Task.CompletedTask;
        }

        private IStaticObject CreateStaticObject(ISector sector, HexNode node)
        {
            var staticObjectPurpose = RollPurpose();

            var factory = SelectStaticObjectFactory(staticObjectPurpose);

            var staticObject = factory.Create(sector, node, default);

            return staticObject;
        }

        private IStaticObjectFactory SelectStaticObjectFactory(PropContainerPurpose staticObjectPurpose)
        {
            var factories = _staticObjectfactoryCollector.GetFactories();

            foreach (var factory in factories)
            {
                if (factory.Purpose != staticObjectPurpose)
                {
                    continue;
                }

                return factory;
            }

            throw new InvalidOperationException($"Не обнаружена фабрика для статических объектов типа {staticObjectPurpose}");
        }

        private PropContainerPurpose RollPurpose()
        {
            var availableStatiObjectPurpose = new[] {
                PropContainerPurpose.CherryBrush,
                PropContainerPurpose.OreDeposits,
                PropContainerPurpose.Pit,
                PropContainerPurpose.Puddle,
                PropContainerPurpose.ThrashHeap
            };

            return PropContainerPurpose.OreDeposits;
        }
    }
}
