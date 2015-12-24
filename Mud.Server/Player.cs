using System;
using Mud.Server.Commands;

namespace Mud.Server
{
    public class Player : IPlayer
    {
        protected readonly ICommandProcessor CommandProcessor;

        public Player(ICommandProcessor processor)
        {
            CommandProcessor = processor;
            Id = Guid.NewGuid();
        }

        #region IPlayer

        #region IActor

        public bool ProcessCommand(string commandLine)
        {
            return CommandProcessor.ProcessCommand(this, commandLine);
        }

        public void Send(string message)
        {
            // TODO: get socket client and send message
        }

        #endregion

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime LastCommandTimestamp { get; private set; }
        public string LastCommand { get; private set; }

        public IEntity Impersonating { get; private set; }

        public void StartImpersonating(IEntity target)
        {
            if (target == null)
                return;
            if (target.ImpersonatedBy != null)
            {
                // TODO
                return;
            }
            // TODO
            Impersonating = target;
            target.StartBeingImpersonated(this);
        }

        public void StopImpersonating()
        {
            // TODO
            if (Impersonating != null)
                Impersonating.StopBeingImpersonated();
            Impersonating = null;
        }

        public void OnDisconnected()
        {
            // TODO
        }

        #endregion
    }
}
