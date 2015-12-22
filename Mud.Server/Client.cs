using System;

namespace Mud.Server
{
    public class Client : IClient
    {
        private readonly ICommandProcessor _commandProcessor;

        public Guid Id { get; private set; }
        public DateTime LastCommandTimestamp { get; private set; }
        public string LastCommand { get; private set; }

        public Client(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
            Id = Guid.NewGuid();
        }

        public void ProcessCommand(string command)
        {
            LastCommand = command;
            LastCommandTimestamp = DateTime.Now;
            // TODO: check spamming
            _commandProcessor.ProcessCommand(this, command);
        }

        public void OnDisconnected()
        {
            // TODO
        }
    }
}
