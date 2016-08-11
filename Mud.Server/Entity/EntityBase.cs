using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Entity
{
    public abstract class EntityBase : ActorBase, IEntity
    {
        protected EntityBase(Guid guid, string name)
        {
            IsValid = true;
            if (guid == Guid.Empty)
                guid = Guid.NewGuid();
            Id = guid;
            Name = name;
            Keywords = name.Split(new [] { ' '}, StringSplitOptions.RemoveEmptyEntries);
            // TODO: remove
            Description = "This is the description of the" + Environment.NewLine
                          + "%Y%"+ GetType().Name+ "%x%" + " %B%" + Name + "%x%" + Environment.NewLine
                          + "over multiple lines";
        }

        protected EntityBase(Guid guid, string name, string description)
        {
            IsValid = true;
            if (guid == Guid.Empty)
                guid = Guid.NewGuid();
            Id = guid;
            Name = name;
            Keywords = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Description = description;
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
                Send("Invalid command or parameters" + Environment.NewLine);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
            return ExecuteCommand(command, rawParameters, parameters);
        }

        public override void Send(string message)
        {
            Log.Default.WriteLine(LogLevels.Debug, "SEND[{0}]: {1}", DisplayName, message);

            if (IncarnatedBy != null)
            {
                if (ServerOptions.PrefixForwardedMessages)
                    message = "<INC|" + DisplayName + ">" + message;
                IncarnatedBy.Send(message);
            }
        }

        public override void Page(StringBuilder text)
        {
            IncarnatedBy?.Page(text);
        }

        #endregion

        public Guid Id { get; }
        public bool IsValid { get; protected set; }
        public string Name { get; protected set; }
        public abstract string DisplayName { get; }
        public IEnumerable<string> Keywords { get; }
        public string Description { get; protected set; }

        public bool Incarnatable { get; private set; }
        public IAdmin IncarnatedBy { get; private set; }

        public virtual bool ChangeIncarnation(IAdmin admin) // if non-null, start incarnation, else, stop incarnation
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.ChangeIncarnation: {0} is not valid anymore", DisplayName);
                return false;
            }
            // TODO: check if not already incarnated, if incarnatable, ...
            IncarnatedBy = admin;
            return true;
        }

        // Overriden in inherited class
        public virtual void OnRemoved() // called before removing an item from the game
        {
            IsValid = false;
            // TODO: warn IncarnatedBy about removing
        }

        #endregion
    }
}
