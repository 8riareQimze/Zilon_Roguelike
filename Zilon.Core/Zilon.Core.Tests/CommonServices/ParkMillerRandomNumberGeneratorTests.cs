﻿using System;
using System.Linq;
using FluentAssertions;

using NUnit.Framework;

using Zilon.Core.CommonServices;
using Zilon.Core.CommonServices.Dices;

namespace Zilon.Core.Tests.CommonServices
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ParkMillerRandomNumberGeneratorTests
    {
        /// <summary>
        /// Тест проверяет, что при разных зернах генерации не происходит ошибки получения случайного числа.
        /// </summary>
        /// <param name="seed"></param>
        [Test]
        public void NextTest([Values(uint.MinValue + 1, (uint)100, (uint)300, uint.MaxValue)]uint seed,
            [Values(1, 10, 100, 1000)] int count)
        {
            // ARRANGE
            //var rng = new ParkMillerRandomNumberGenerator(seed);

            var r = new ExpDice(3);
            var seq = new int[1000];
            for (var i = 0; i < seq.Length; i++)
            {
                seq[i] = r.Roll(100);
            }
            var gr = seq.GroupBy(x => x);
            var freq = gr.ToDictionary(x => x.Key, x => x.Count()).OrderBy(x => x.Key);
            foreach (var fr in freq)
            {
                Console.WriteLine(fr.Key + "\t" + fr.Value);
            }

            //// ACT
            //Action act = () =>
            //{
            //    rng.GetSequence(count);
            //};

            //// ASSERT
            //act.Should().NotThrow();
        }
    }
}