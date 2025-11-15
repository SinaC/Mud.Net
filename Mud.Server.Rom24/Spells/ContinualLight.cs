using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Buff)]
public class ContinualLight : OptionalItemInventorySpellBase
{
    private const string SpellName = "Continual Light";

    private IWiznet Wiznet { get; }
    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }
    private ISettings Settings { get; }

    public ContinualLight(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager, IItemManager itemManager, ISettings settings)
        : base(randomManager)
    {
        Wiznet = wiznet;
        AuraManager = auraManager;
        ItemManager = itemManager;
        Settings = settings;
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
                new ItemFlagsAffect { Modifier = new ItemFlags("Glowing"), Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0} glows with a white light.", Item);
            return;
        }
        // create item
        var light = ItemManager.AddItem(Guid.NewGuid(), Settings.LightBallBlueprintId, Caster.Room);
        if (light == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Wiznet($"ContinualLight: cannot create item from blueprint {Settings.LightBallBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        Caster.Act(ActOptions.ToAll, "{0} twiddle{0:v} {0:s} thumbs and {1} appears.", Caster, light);
    }
}
