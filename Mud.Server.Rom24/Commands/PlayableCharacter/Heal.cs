using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Commands.PlayableCharacter;

[PlayableCharacterCommand("heal", "Shop", "Heal")]
[Syntax("[cmd] <spell>")]
[Help(
@"The healer decided to grab a quick buck, and now charges for his heals.  Some
services are still free to players of level 10 or below, however.  To see
a full listing of the healer's services, type 'heal' at his residence. To
receive healing, bring plenty of money, and type '[cmd] <spell>'.")]
public class Heal : PlayableCharacterGameAction
{
    private ILogger<Heal> Logger { get; }
    private IRandomManager RandomManager { get; }

    public Heal(ILogger<Heal> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    private static (string Keyword, string Description, string? SpellName, int Cost)[] HealSpellInfos =
    [
        ("light", "cure light wounds", "cure light", 10),
        ("serious", "cure serious wounds", "cure serious", 15),
        ("critical", "cure critical wounds", "cure critical", 25),
        ("heal", "healing spell", "heal", 50),
        ("blind", "cure blindness", "cure blindness", 20),
        ("disease", "cure disease", "cure disease", 15),
        ("poison", "cure poison", "cure poison", 25),
        ("uncurse", "remove curse", "remove curse", 50),
        ("refresh", "restore movement", "refresh", 5),
        ("mana", "restore mana", null, 10),
    ];

    protected INonPlayableCharacter Healer { get; set; } = default!;
    protected (string Keyword, string Description, string? SpellName, int Cost) SelectedHealSpellInfo { get; set; } = default!;
    protected long TotalCost { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Healer = Actor.Room?.NonPlayableCharacters.FirstOrDefault(x => x.ActFlags.IsSet("IsHealer"))!; // don't check if CanSee so blind people can use heal blind
        if (Healer == null)
            return "You can't do that here.";

        if (actionInput.Parameters.Length == 0)
        {
            Actor.Act(ActOptions.ToCharacter, "{0:N} says 'I offer the following spells:", Healer);
            var sb = new StringBuilder();
            foreach (var info in HealSpellInfos)
                sb.AppendFormatLine("{0,-10}: {1,-20} {2,2} silver", info.Keyword, info.Description, info.Cost);
            sb.Append("Type heal <type> to be healed");
            return sb.ToString();
        }

        // search spell
        var whatParameter = actionInput.Parameters[0];
        SelectedHealSpellInfo = HealSpellInfos.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Keyword, whatParameter.Value));
        if (SelectedHealSpellInfo == default)
            return Actor.ActPhrase("{0:N} says 'Type 'heal' for a list of spells.");

        // check cost
        TotalCost = Actor.SilverCoins + Actor.GoldCoins * 100;
        if (SelectedHealSpellInfo.Cost > TotalCost)
            return Actor.ActPhrase("{0:N} says 'You do not have enough gold for my services.");

        //
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // add some delay
        Actor.SetGlobalCooldown(Pulse.PulseViolence);

        // pay
        Actor.DeductCost(SelectedHealSpellInfo.Cost);
        Healer.UpdateMoney(TotalCost % 100, TotalCost / 100);

        // cast spell
        if (SelectedHealSpellInfo.SpellName == null) // special cast for mana
        {
            var amount = RandomManager.Dice(2, 8) + Healer.Level / 3;
            Actor.UpdateResource(ResourceKinds.Mana, amount);
            Actor.UpdateResource(ResourceKinds.Psy, amount);
            SpellHelper.SaySpell(Healer, "restore mana");
            Actor.Send("A warm glow passes through you.");
        }
        else
            CastSpell(Healer, SelectedHealSpellInfo.SpellName, Actor);
    }

    private bool CastSpell(INonPlayableCharacter caster, string spellName, IPlayableCharacter victim)
    {
        var successfull = caster.CastSpell(spellName, victim);
        if (!successfull)
            Logger.LogError("Heal: error on {caster} while casting spell {spellName} on {victimName}", caster.DebugName, spellName, victim.DebugName);
        return successfull;
    }
}