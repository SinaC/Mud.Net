using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Buff)]
[Syntax(
    "cast [spell]",
    "cast [spell] <object>")]
[Help(
@"This spell creates a ball of light, which you can hold as a light source.
The ball of light will last indefinitely. It may also be used on an object
to give it an enchanted glow.")]
public class ContinualLight : OptionalItemInventorySpellBase
{
    private const string SpellName = "Continual Light";

    private IServiceProvider ServiceProvider { get; }
    private IWiznet Wiznet { get; }
    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }
    private int LightBallBlueprintId { get; }

    public ContinualLight(ILogger<ContinualLight> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        Wiznet = wiznet;
        AuraManager = auraManager;
        ItemManager = itemManager;
        LightBallBlueprintId = options.Value.BlueprintIds.LightBall;
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

            AuraManager.AddAura(Item, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Glowing"), Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0} glows with a white light.", Item);
            return;
        }
        // create item
        var light = ItemManager.AddItem(Guid.NewGuid(), LightBallBlueprintId, Caster.Room);
        if (light == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"ContinualLight: cannot create item from blueprint {LightBallBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        Caster.Act(ActOptions.ToAll, "{0} twiddle{0:v} {0:s} thumbs and {1} appears.", Caster, light);
    }
}
