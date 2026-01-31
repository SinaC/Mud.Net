using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Item;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Buff)]
[Syntax(
    "cast [spell]",
    "cast [spell] <object>")]
[Help(
@"This spell creates a ball of light, which you can hold as a light source.
The ball of light will last indefinitely. It may also be used on an object
to give it an enchanted glow.")]
[OneLineHelp("creates an eternal light source")]
public class ContinualLight : OptionalItemInventorySpellBase
{
    private const string SpellName = "Continual Light";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private IWiznet Wiznet { get; }
    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }
    private int LightBallBlueprintId { get; }

    public ContinualLight(ILogger<ContinualLight> logger, IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager)
    {
        Wiznet = wiznet;
        AuraManager = auraManager;
        ItemManager = itemManager;
        LightBallBlueprintId = options.Value.SpellBlueprintIds.LightBall;
    }

    protected override void Invoke()
    {
        if (Item != null)
        {
            if (Item.ItemFlags.IsSet("Glowing"))
            {
                Caster.Act(ActOptions.ToCharacter, "{0} is already glowing.", Item);
                return;
            }

            AuraManager.AddAura(Item, SpellName, Caster, Level, Pulse.Infinite, new AuraFlags("NoDispel", "Permanent"), true,
                new ItemFlagsAffect { Modifier = new ItemFlags("Glowing"), Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0} glows with a white light.", Item);
            return;
        }
        // create item
        var light = ItemManager.AddItem<IItemLight>(Guid.NewGuid(), LightBallBlueprintId, $"SpellContinualLight[{Caster.DebugName}]", Caster.Room);
        if (light == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"ContinualLight: cannot create item from blueprint {LightBallBlueprintId}.", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            return;
        }
        Caster.Act(ActOptions.ToAll, "{0} twiddle{0:v} {0:s} thumbs and {1} appears.", Caster, light);
    }
}
