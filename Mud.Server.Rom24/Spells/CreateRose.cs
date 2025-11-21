using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation)]
[Syntax("cast [spell]")]
[Help(
@"A romantic spell that creates a fragrant red rose, with utterly no game
use whatsoever.")]
public class CreateRose : ItemCreationSpellBase
{
    private const string SpellName = "Create Rose";

    private int RoseBlueprintId { get; }

    public CreateRose(ILogger<CreateRose> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, IOptions<Rom24Options> options) 
        : base(logger, randomManager, wiznet, itemManager)
    {
        RoseBlueprintId = options.Value.BlueprintIds.Rose;
    }

    public override string? Setup(ISpellActionInput spellActionInput)
    {
        var baseSetup = base.Setup(spellActionInput);
        if (baseSetup != null)
            return baseSetup;

        return "Not Yet Implemented";
    }

    protected override void Invoke()
    {
        //TODO: add rose blueprint
    }
}
