using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mud.Logger;
using Mud.Server.Input;

namespace Mud.Server
{
    public abstract class EntityBase : ActorBase, IEntity
    {
        private readonly List<IObject> _objectsInContainer;

        protected EntityBase(Guid guid, string name)
        {
            _objectsInContainer = new List<IObject>();

            if (guid == Guid.Empty)
                guid = Guid.NewGuid();
            Id = guid;
            Name = name;

            // TODO: remove
            Description = "This is the description of the" + Environment.NewLine
                          + "%Y%"+ GetType().Name+ "%x%" + " %B%" + Name + "%x%" + Environment.NewLine
                          + "over multiple lines";
        }

        #region IEntity

        #region IActor

        public override bool ProcessCommand(string commandLine)
        {
            string command;
            string rawParameters;
            CommandParameter[] parameters;
            bool forceOutOfGame;

            // Extract command and parameters
            bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(commandLine, out command, out rawParameters, out parameters, out forceOutOfGame);
            if (!extractedSuccessfully)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                Send("Invalid command or parameters");
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", Name, commandLine);
            return ExecuteCommand(command, rawParameters, parameters);
        }

        public override void Send(string format, params object[] parameters)
        {
            if (IncarnatedBy != null)
                IncarnatedBy.Send("<INC|" + Name + ">" + format, parameters);
        }

        #endregion

        #region IContainer

        public IReadOnlyCollection<IObject> ObjectsInContainer
        {
            get { return new ReadOnlyCollection<IObject>(_objectsInContainer); }
        }

        public bool Put(IObject obj)
        {
            // TODO: check if already in a container
            _objectsInContainer.Add(obj);
            return true;
        }

        public bool Get(IObject obj)
        {
            bool removed = _objectsInContainer.Remove(obj);
            return removed;
        }

        #endregion

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Keyword { get; private set; }
        public string Description { get; private set; }

        public bool Incarnatable { get; private set; }
        public IAdmin IncarnatedBy { get; private set; }

        public bool ChangeIncarnation(IAdmin admin) // if non-null, start incarnation, else, stop incarnation
        {
            // TODO: check if not already incarnated, if incarnatable, ...
            IncarnatedBy = admin;
            return true;
        }

        // TODO: override in inherited class
        public virtual void OnRemoved()
        {
            // TODO: warn IncarnatedBy about removing
        }

        #endregion
    }
}
