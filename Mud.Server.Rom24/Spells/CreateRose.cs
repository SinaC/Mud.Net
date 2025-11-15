using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation)]
public class CreateRose : ItemCreationSpellBase
{
    private const string SpellName = "Create Rose";

    public CreateRose(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings) 
        : base(randomManager, wiznet, itemManager, settings)
    {
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
