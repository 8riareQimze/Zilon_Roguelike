﻿namespace Zilon.Core.World
{
    public static class GlobeMetrics
    {
        /// <summary>
        /// Count of globe update to execute actor tasks to next iteration.
        /// Iteration of the globe is:
        /// - Survival updates.
        /// - Events updates.
        /// - Desease updates.
        /// </summary>
        public static int OneIterationLength => 10;

        /// <summary>
        /// Transition limit from pool.
        /// The limit is required to prevent hanging.
        /// </summary>
        public static int TransitionPerGlobeIteration => 10;
    }
}