using System;
using System.Collections.Generic;

namespace Mud.Server
{
    public class Client : IClient
    {
        private readonly ICommandProcessor _commandProcessor;

        public Guid Id { get; private set; }
        public DateTime LastCommandTimestamp { get; private set; }
        public string LastCommand { get; private set; }
        
        public IEntity Impersonating { get; private set; }

        public Client(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
            Id = Guid.NewGuid();
        }

        public void GoInGame(IEntity entity)
        {
            if (entity == null)
                return;
            if (entity.ImpersonatedBy != null)
            {
                // TODO
                return;
            }
            // TODO
            Impersonating = entity;
            entity.ImpersonatedBy = this;
        }

        public void GoOutOfGame()
        {
            // TODO
            Impersonating = null;
        }

        public bool ProcessCommand(string command)
        {
            LastCommand = command;
            LastCommandTimestamp = DateTime.Now;
            // TODO: check spamming
            return _commandProcessor.ProcessCommand(this, command);
        }

        public List<string> CommandList()
        {
            return _commandProcessor.CommandList(Impersonating != null ? CommandFlags.InGame : CommandFlags.OutOfGame);
        }

        public void OnDisconnected()
        {
            // TODO
        }
    }
}
