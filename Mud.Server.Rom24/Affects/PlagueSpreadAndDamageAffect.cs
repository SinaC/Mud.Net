using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Affects.Character;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[Affect("Plague", typeof(PlagueSpreadAndDamageAffectData))]
public class PlagueSpreadAndDamageAffect : ICharacterPeriodicAffect, ICustomAffect
{
    private IServiceProvider ServiceProvider { get; }
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }

    public PlagueSpreadAndDamageAffect(IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
    {
        ServiceProvider = serviceProvider;
        RandomManager = randomManager;
        AuraManager = auraManager;
    }

    public void Initialize(AffectDataBase data)
    {
        //TODO: PlagueSpreadAndDamageAffectData
    }

    public void Append(StringBuilder sb)
    {
        sb.Append("%c%applies %r%disease%x% damage periodically");
    }

    public AffectDataBase MapAffectData()
    {
        return new PlagueSpreadAndDamageAffectData();
    }

    public void Apply(IAura aura, ICharacter character)
    {
        if (aura.Level == 1)
            return;

        character.Act(ActOptions.ToRoom, "{0:N} writhes in agony as plague sores erupt from {0:s} skin.", character);
        character.Send("You writhe in agony from the plague.");

        // spread
        if (character.Room != null)
        {
            foreach (ICharacter victim in character.Room.People.Where(x => !x.CharacterFlags.IsSet("Plague")))
            {
                if (!victim.SavesSpell(aura.Level - 2, SchoolTypes.Disease)
                    && RandomManager.Chance(6))
                {
                    victim.Send("You feel hot and feverish.");
                    victim.Act(ActOptions.ToRoom, "{0:N} shivers and looks very ill.", victim);
                    int duration = RandomManager.Range(1, 2 * aura.Level);
                    AuraManager.AddAura(victim, aura.AbilityName, character, aura.Level - 1, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                        new CharacterAttributeAffect {Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add},
                        new CharacterFlagsAffect {Modifier = new CharacterFlags(ServiceProvider, "Plague"), Operator = AffectOperators.Or},
                        new PlagueSpreadAndDamageAffect(ServiceProvider, RandomManager, AuraManager));
                }
            }
        }

        // damage
        int damage = Math.Min(character.Level, aura.Level / 5 + 1);
        character.UpdateMovePoints(-damage);
        character.UpdateResource(ResourceKinds.Mana, -damage);
        character.UpdateResource(ResourceKinds.Psy, -damage);
        character.AbilityDamage(character, damage, SchoolTypes.Disease, "sickness", false);
    }
}
