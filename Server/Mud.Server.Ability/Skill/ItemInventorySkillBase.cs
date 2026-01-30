using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Random;

namespace Mud.Server.Ability.Skill;

public abstract class ItemInventorySkillBase : SkillBase
{
    protected ItemInventorySkillBase(ILogger<ItemInventorySkillBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected IItem Item { get; set; } = default!;

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length < 1) // should already been tested in Guards
            return "What should the skill be used upon?";

        Item = FindHelpers.FindByName(User.Inventory.Where(User.CanSee), skillActionInput.Parameters[0])!; // TODO: equipments ?
        if (Item == null)
            return "You are not carrying that.";
        return null;
    }
}
