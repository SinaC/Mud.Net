﻿using System;
using Mud.Logger;

namespace Mud.Server
{
    public abstract class EntityBase : ActorBase, IEntity
    {
        protected EntityBase(Guid guid, string name)
        {
            if (guid == Guid.Empty)
                guid = Guid.NewGuid();
            Id = guid;
            Name = name;
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

        public virtual void OnRemoved()
        {
            // TODO: warn IncarnatedBy about removing
        }

        #endregion
    }
}
