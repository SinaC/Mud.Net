using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.Abilities
{
    public interface IAbilityManager
    {
        IAbility this[string name] { get; }
    }
}
