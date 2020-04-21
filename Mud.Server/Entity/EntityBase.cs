using System;
using System.Collections.Generic;
using System.Text;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Input;

namespace Mud.Server.Entity
{
    public abstract class EntityBase : ActorBase, IEntity
    {
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
            // Extract command and parameters
            bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(commandLine, out var command, out var rawParameters, out var parameters, out _);
            if (!extractedSuccessfully)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                Send("Invalid command or parameters");
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DebugName, commandLine);
            return ExecuteCommand(command, rawParameters, parameters);
        }

        public override void Send(string message, bool addTrailingNewLine)
        {
            Log.Default.WriteLine(LogLevels.Debug, "SEND[{0}]: {1}", DebugName, message);

            if (IncarnatedBy != null)
            {
                if (Settings.PrefixForwardedMessages)
                    message = "<INC|" + DisplayName + ">" + message;
                IncarnatedBy.Send(message, addTrailingNewLine);
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
        public abstract string DebugName { get; }

        public bool Incarnatable { get; private set; }
        public IAdmin IncarnatedBy { get; protected set; }

        public virtual bool ChangeIncarnation(IAdmin admin) // if non-null, start incarnation, else, stop incarnation
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.ChangeIncarnation: {0} is not valid anymore", DisplayName);
                IncarnatedBy = null;
                return false;
            }
            if (admin != null)
            {
                if (!Incarnatable)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "IEntity.ChangeIncarnation: {0} cannot be incarnated", DebugName);
                    return false;
                }
                if (IncarnatedBy != null)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "IEntity.ChangeIncarnation: {0} is already incarnated by {1}", DebugName, IncarnatedBy.DisplayName);
                    return false;
                }
            }
            IncarnatedBy = admin;
            return true;
        }

        public virtual string RelativeDisplayName(INonPlayableCharacter beholder, bool capitalizeFirstLetter = false)
        {
            return DisplayName; // no behavior by default
        }

        public virtual string RelativeDisplayName(IPlayableCharacter beholder, bool capitalizeFirstLetter = false)
        {
            return DisplayName; // no behavior by default
        }

        public virtual string RelativeDescription(INonPlayableCharacter beholder)
        {
            return Description; // no behavior by default
        }

        public virtual string RelativeDescription(IPlayableCharacter beholder)
        {
            return Description; // no behavior by default
        }

        // Overriden in inherited class
        public virtual void OnRemoved() // called before removing an item from the game
        {
            IsValid = false;
            // TODO: warn IncarnatedBy about removing
            IncarnatedBy?.StopIncarnating();
            IncarnatedBy = null;
        }

        #endregion
    }
}
