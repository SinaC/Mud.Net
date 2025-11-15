using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Ability.Skill;

public abstract class ItemInventorySkillBase : SkillBase
{
    protected IItem Item { get; set; } = default!;

    protected ItemInventorySkillBase(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length < 1)
            return "What should the skill be cast upon?";
        Item = FindHelpers.FindByName(User.Inventory.Where(User.CanSee), skillActionInput.Parameters[0])!; // TODO: equipments ?
        if (Item == null)
            return "You are not carrying that.";
        return null;
    }
}
