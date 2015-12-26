using System;
using Mud.Server.Old.Commands;

namespace Mud.Server.Old
{
    public abstract class EntityBase : IEntity
    {
        protected readonly ICommandProcessor CommandProcessor;

        protected EntityBase(ICommandProcessor processor)
        {
            CommandProcessor = processor;
        }

        #region IEntity

        #region IActor

        public bool ProcessCommand(string commandLine)
        {
            return CommandProcessor.ProcessCommand(this, commandLine);
        }

        public void Send(string message)
        {
            if (ControlledBy != null)
                ControlledBy.Send(message);
            if (ImpersonatedBy != null)
                ImpersonatedBy.Send(message);
        }
        
        #endregion

        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public bool Impersonable { get; protected set; }
        public IEntity Controlling { get; protected set; }
        public IPlayer ImpersonatedBy { get; protected set; }
        public IEntity ControlledBy { get; protected set; }

        public void StartControlling(IEntity slave)
        {
            if (slave == null)
                return;
            if (slave == this)
                return;
            if (slave.ControlledBy != null)
            {
                // TODO
                return;
            }
            Controlling = slave;
            slave.StartBeingControlled(this);
        }

        public void StopControlling()
        {
            if (Controlling != null)
                Controlling.StopBeingControlled();
            Controlling = null;
        }

        public void StartBeingImpersonated(IPlayer by)
        {
            // TODO: check if already impersonated
            ImpersonatedBy = by;
        }

        public void StopBeingImpersonated()
        {
            // TODO
            ImpersonatedBy = null;
        }

        public void StartBeingControlled(IEntity master)
        {
            // TODO: check if already controlled
            ControlledBy = master;
        }

        public void StopBeingControlled()
        {
            // TODO
            ControlledBy = null;
        }

        #endregion
    }
}
