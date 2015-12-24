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
        
        public bool Impersonating { get; set; }

        public Client(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
            Id = Guid.NewGuid();
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
            return _commandProcessor.CommandList(Impersonating ? CommandFlags.InGame : CommandFlags.OutOfGame);
        }

        public void OnDisconnected()
        {
            // TODO
        }
    }
}
