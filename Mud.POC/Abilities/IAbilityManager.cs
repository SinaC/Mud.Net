using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.Abilities
{
    public interface IAbilityManager
    {
        IEnumerable<IAbility> Abilities { get; }

        IAbility this[string name] { get; }

        CommandExecutionResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters);
    }
}
