﻿using System;
using System.Threading.Tasks;

namespace Zilon.Core.Tactics.Behaviour
{
    public enum ActorTaskSourceControl
    {
        Undefined = 0,
        Human = 1,
        Bot = 2
    }

    public interface IActorTaskControlSwitcher
    {
        void Switch(ActorTaskSourceControl taskSourceControl);

        ActorTaskSourceControl CurrentControl { get; }
    }

    public class SwitchHumanActorTaskSource<TContext> : IActorTaskControlSwitcher, IHumanActorTaskSource<TContext>
        where TContext : ISectorTaskSourceContext
    {
        private readonly IActorTaskSource<TContext> _botActorTaskContext;
        private readonly IHumanActorTaskSource<TContext> _humanActorTaskSource;

        public ActorTaskSourceControl CurrentControl { get; private set; }

        public SwitchHumanActorTaskSource(IHumanActorTaskSource<TContext> humanActorTaskSource,
            IActorTaskSource<TContext> botActorTaskContext)
        {
            _humanActorTaskSource =
                humanActorTaskSource ?? throw new ArgumentNullException(nameof(humanActorTaskSource));
            _botActorTaskContext = botActorTaskContext ?? throw new ArgumentNullException(nameof(botActorTaskContext));

            CurrentControl = ActorTaskSourceControl.Human;
        }

        public void Switch(ActorTaskSourceControl taskSourceControl)
        {
            switch (taskSourceControl)
            {
                case ActorTaskSourceControl.Undefined:
                default:
                    throw new ArgumentException($"Invalid value {taskSourceControl}", nameof(taskSourceControl));

                case ActorTaskSourceControl.Human:
                case ActorTaskSourceControl.Bot:
                    // If bot switched need to drop intention of player.
                    // Because intention stops game loop for user command.
                    CurrentControl = taskSourceControl;
                    _humanActorTaskSource.DropIntentionWaiting();
                    break;
            }
        }

        public void CancelTask(IActorTask cancelledActorTask)
        {
            switch (CurrentControl)
            {
                case ActorTaskSourceControl.Human:
                    _humanActorTaskSource.CancelTask(cancelledActorTask);
                    break;

                case ActorTaskSourceControl.Bot:
                    _botActorTaskContext.CancelTask(cancelledActorTask);
                    break;

                case ActorTaskSourceControl.Undefined:
                    //TODO Add common TaskSourceException
                    throw new InvalidOperationException("Task source is undefined.");
                default:
                    throw new InvalidOperationException($"Task source {CurrentControl} is unknown.");
            }
        }

        public bool CanIntent()
        {
            switch (CurrentControl)
            {
                case ActorTaskSourceControl.Human:
                    return _humanActorTaskSource.CanIntent();

                case ActorTaskSourceControl.Bot:
                    throw new InvalidOperationException("Intension available only under human control.");

                case ActorTaskSourceControl.Undefined:
                    //TODO Add common TaskSourceException
                    throw new InvalidOperationException("Task source is undefined.");
                default:
                    throw new InvalidOperationException($"Task source {CurrentControl} is unknown.");
            }
        }

        public void DropIntentionWaiting()
        {
            switch (CurrentControl)
            {
                case ActorTaskSourceControl.Human:
                    _humanActorTaskSource.DropIntentionWaiting();
                    break;

                case ActorTaskSourceControl.Bot:
                    throw new InvalidOperationException("Intension available only under human control.");

                default:
                    //TODO Add common TaskSourceException
                    throw new InvalidOperationException("Task source is undefined.");
            }
        }

        public async Task<IActorTask> GetActorTaskAsync(IActor actor, TContext context)
        {
            switch (CurrentControl)
            {
                case ActorTaskSourceControl.Human:
                    return await _humanActorTaskSource.GetActorTaskAsync(actor, context).ConfigureAwait(false);

                case ActorTaskSourceControl.Bot:
                    await Task.Delay(100);
                    return await _botActorTaskContext.GetActorTaskAsync(actor, context).ConfigureAwait(false);

                case ActorTaskSourceControl.Undefined:
                    //TODO Add common TaskSourceException
                    throw new InvalidOperationException("Task source is undefined.");
                default:
                    throw new InvalidOperationException($"Task source {CurrentControl} is unknown.");
            }
        }

        public void Intent(IIntention intention, IActor activeActor)
        {
            if (CurrentControl == ActorTaskSourceControl.Human)
            {
                _humanActorTaskSource.Intent(intention, activeActor);
            }
            else
            {
                throw new InvalidOperationException("Intension available only under human control.");
            }
        }

        public async Task IntentAsync(IIntention intention, IActor activeActor)
        {
            if (CurrentControl == ActorTaskSourceControl.Human)
            {
                await _humanActorTaskSource.IntentAsync(intention, activeActor);
            }
            else
            {
                throw new InvalidOperationException("Intension available only under human control.");
            }
        }

        public void ProcessTaskComplete(IActorTask actorTask)
        {
            switch (CurrentControl)
            {
                case ActorTaskSourceControl.Human:
                    _humanActorTaskSource.ProcessTaskComplete(actorTask);
                    break;

                case ActorTaskSourceControl.Bot:
                    _botActorTaskContext.ProcessTaskComplete(actorTask);
                    break;

                case ActorTaskSourceControl.Undefined:
                    //TODO Add common TaskSourceException
                    throw new InvalidOperationException("Task source is undefined.");
                default:
                    throw new InvalidOperationException($"Task source {CurrentControl} is unknown.");
            }
        }

        public void ProcessTaskExecuted(IActorTask actorTask)
        {
            switch (CurrentControl)
            {
                case ActorTaskSourceControl.Human:
                    _humanActorTaskSource.ProcessTaskExecuted(actorTask);
                    break;

                case ActorTaskSourceControl.Bot:
                    _botActorTaskContext.ProcessTaskExecuted(actorTask);
                    break;

                case ActorTaskSourceControl.Undefined:
                    //TODO Add common TaskSourceException
                    throw new InvalidOperationException("Task source is undefined.");
                default:
                    throw new InvalidOperationException($"Task source {CurrentControl} is unknown.");
            }
        }
    }
}