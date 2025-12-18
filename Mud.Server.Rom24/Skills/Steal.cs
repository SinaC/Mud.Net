using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Commands.Character.Communication;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand(SkillName, "Item")]
[Syntax("[cmd] <item|coin> <victim>")]
[Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
[Help(
@"Theft is the defining skill of the thief, and is only available to that class.
It allows items to be stolen from the inventory of monsters and characters,
and even from shops!  But beware, shop keepers gaurd their merchandise 
carefully, and attempting to steal from a character earns you a THIEF flag
if you are caught (making you free game for killing).")]
public class Steal : SkillBase
{
    private const string SkillName = "Steal";

    private IGameActionManager GameActionManager { get; }
    private int MaxLevel { get; }

    public Steal(ILogger<Steal> logger, IRandomManager randomManager, IGameActionManager gameActionManager, IOptions<WorldOptions> worldOptions)
        : base(logger, randomManager)
    {
        GameActionManager = gameActionManager;
        MaxLevel = worldOptions.Value.MaxLevel;
    }

    protected ICharacter Victim { get; set; } = default!;
    protected bool Failed { get; set; }
    protected long Gold { get; set; }
    protected long Silver { get; set; }
    protected IItem What { get; set; } = default!;

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length < 2)
            return "Steal what from whom?";

        Victim = FindHelpers.FindByName(User.Room.People, skillActionInput.Parameters[1])!;
        if (Victim == null)
            return "They aren't here.";

        if (Victim == Actor)
            return "That's pointless.";

        var isSafeResult = Victim.IsSafe(Actor);
        if (isSafeResult != null)
            return isSafeResult;

        if (Victim is INonPlayableCharacter && Victim.Fighting != null)
            return "Kill stealing is not permitted. You'd better not -- you might get hit.";

        Failed = CheckIfFailed();
        if (Failed)
            return null;

        var whatParameter = skillActionInput.Parameters[0];
        if (StringCompareHelpers.StringEquals(whatParameter.Value, "coin")
            || StringCompareHelpers.StringEquals(whatParameter.Value, "coins")
            || StringCompareHelpers.StringEquals(whatParameter.Value, "silver")
            || StringCompareHelpers.StringEquals(whatParameter.Value, "gold"))
        {
            Gold = (Victim.GoldCoins * RandomManager.Range(1, Actor.Level)) / MaxLevel;
            Silver = (Victim.SilverCoins * RandomManager.Range(1, Actor.Level)) / MaxLevel;
            if (Gold <= 0 && Silver <= 0)
                return "You couldn't get any coins.";
            return null;
        }

        What = FindHelpers.FindByName(Victim.Inventory.Concat(Victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)), whatParameter)!;
        if (What == null)
            return "You can't find it.";
        if (What.ItemFlags.HasAny("NoDrop", "Inventory")
            || What.Level > Actor.Level)
            return "You can't pry it away.";
        if (Actor.CarryNumber + What.CarryCount > Actor.MaxCarryNumber)
            return "You have your hands full.";
        if (Actor.CarryWeight + What.TotalWeight > Actor.MaxCarryWeight)
            return "You can't carry that much weight.";

        return null;
    }

    protected override bool Invoke()
    {
        if (Failed)
        {
            // Failure
            Actor.Send("Oops.");
            //Actor.RemoveBaseCharacterFlags(CharacterFlags.Sneak);
            // TODO: what about invis ?
            //Actor.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, "Sneak"), true); // recompute
            Actor.RemoveAuras(x => x.Affects.OfType<ICharacterFlagsAffect>().Any(a => a.Modifier.IsSet("Invisible") || a.Modifier.IsSet("Sneak") || a.Modifier.IsSet("Hide")), false);

            Victim.Act(ActOptions.ToCharacter, "{0:N} tried to steal from you.", Actor);
            Victim.ActToNotVictim(Victim, "{0:N} tried to steal from {1}.", Actor, Victim);

            // Stand if not awake
            if (Victim.Position <= Positions.Sleeping)
                GameActionManager.Execute(Victim, "Stand");
            // If awake, yell
            if (Victim.Position > Positions.Sleeping)
            {
                string msg = "Someone tried to rob me!";
                if (Victim.CanSee(Actor))
                    switch (RandomManager.Range(0, 3))
                    {
                        case 0:
                            msg = Victim.ActPhrase("{0:N} is a lousy thief!", Actor);
                            break;
                        case 1:
                            msg = Victim.ActPhrase("{0:N} couldn't rob {0:s} way out of a paper bag!", Actor);
                            break;
                        case 2:
                            msg = Victim.ActPhrase("{0:N} tried to rob me!", Actor);
                            break;
                        case 3:
                            msg = Victim.ActPhrase("Keep your hands out of there, {0}!", Actor);
                            break;
                    }

                //GameActionManager.Execute(Victim, "Yell", new CommandParameter(msg, msg, false, false));
                GameActionManager.Execute<Yell, ICharacter>(Victim, msg);
            }

            if (Victim is IPlayableCharacter pcVictim)
            {
                // TODO: wiznet, thief flag
            }
            else
            {
                Victim.MultiHit(Actor);
            }

            return false;
        }

        // Item
        if (What != null)
        {
            if (What.EquippedBy != null)
                What.ChangeEquippedBy(null, false);
            What.ChangeContainer(Actor);
            Actor.Act(ActOptions.ToCharacter, "You pocket {0}.", What);
            Actor.Send("Got it!");
        }
        // Money
        else
        {
            Actor.UpdateMoney(Silver, Gold);
            Victim.UpdateMoney(-Silver, -Gold);

            if(Silver <= 0)
                Actor.Send("Bingo! You got {0} gold coins.", Gold);
            else if (Gold <= 0)
                Actor.Send("Bingo! You got {0} silver coins.", Silver);
            else
                Actor.Send("Bingo! You got {0} silver and {1} gold coins.", Silver, Gold);
        }

        return true;
    }

    private bool CheckIfFailed()
    {
        int percent = RandomManager.Range(1, 100);
        if (Victim.Position <= Positions.Sleeping)
            percent -= 10;
        if (!Victim.CanSee(Actor))
            percent += 25;
        else
            percent += 50;
        if (Actor is IPlayableCharacter pcActor)
        {
            if (percent > Learned /*|| !Actor.IsInClan*/)
                return true;
            if (Victim is IPlayableCharacter pcVictim && (pcActor.Level + 7 < pcVictim.Level || pcActor.Level - 7 > pcVictim.Level))
                return true;
        }

        return false;
    }
}
