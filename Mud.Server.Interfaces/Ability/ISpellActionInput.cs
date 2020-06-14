using Mud.Server.Input;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Ability
{
    public interface ISpellActionInput
    {
        ICharacter Caster { get; }
        string RawParameters { get; }
        CommandParameter[] Parameters { get; }
        IAbilityInfo AbilityInfo { get; }
        int Level { get; }
        CastFromItemOptions CastFromItemOptions { get; }
        bool IsCastFromItem { get; }
    }

    public class CastFromItemOptions
    {
        public IItem Item { get; set; }
        public IEntity PredefinedTarget { get; set; }
    }
}
