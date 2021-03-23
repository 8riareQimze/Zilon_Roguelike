﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Zilon.Core.Commands;
using Zilon.Core.PersonModules;
using Zilon.Core.Players;

namespace Zilon.TextClient
{
    internal class CommandLoop
    {
        private readonly IPlayer _player;
        private readonly ICommandManager _commandManager;

        public CommandLoop(IPlayer player, ICommandManager commandManager)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var playerPersonSurvivalModule = _player.MainPerson.GetModule<ISurvivalModule>();
                while (!playerPersonSurvivalModule.IsDead)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        ExecuteCommands();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }, cancellationToken);
        }

        private bool _hasPendingCommand;

        private object _lockObj = new object();

        private void ExecuteCommands()
        {
            lock (_lockObj)
            {
                _hasPendingCommand = true;

                var errorOccured = false;

                var command = _commandManager.Pop();

                try
                {
                    if (command != null)
                    {
                        command.Execute();

                        if (command is IRepeatableCommand repeatableCommand)
                        {
                            if (repeatableCommand.CanRepeat())
                            {
                                _commandManager.Push(repeatableCommand);
                                Console.WriteLine("Auto execute last command");
                            }
                            else
                            {
                                _hasPendingCommand = true;
                            }
                        }
                        else
                        {
                            _hasPendingCommand = false;
                        }
                    }
                    else
                    {
                        _hasPendingCommand = false;
                    }
                }
                catch (Exception exception)
                {
                    errorOccured = true;
                    throw new InvalidOperationException($"Не удалось выполнить команду {command}.", exception);
                }
                finally
                {
                    if (errorOccured)
                    {
                        _hasPendingCommand = false;
                    }
                }
            }
        }

        public bool HasPendingCommands()
        {
            lock (_lockObj)
            {
                return _hasPendingCommand;
            }
        }
    }
}
