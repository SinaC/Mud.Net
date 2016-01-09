using System;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Abilities
{
    public abstract class SingleTargetInRoomAbilityBase : AbilityBase
    {
        public override bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                source.Send("Use {0} on whom ?"+Environment.NewLine, Name);
                return false;
            }
            ICharacter victim = FindHelpers.FindByName(source.Room.People, parameters[0]);
            if (victim == null)
            {
                source.Send(StringHelpers.CharacterNotFound);
                return false;
            }
            return Process(source, victim);
        }

        protected abstract bool Process(ICharacter source, ICharacter victim);
    }
}
