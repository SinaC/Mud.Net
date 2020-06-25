using System.Linq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings;

namespace Mud.Server.Rom24.Skills
{
    [CharacterCommand(SkillName, "Item")]
    [Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    public class Steal : SkillBase
    {
        private const string SkillName = "Steal";

        private IGameActionManager GameActionManager { get; }
        private ISettings Settings { get; }

        public ICharacter Victim { get; protected set; }
        public bool Failed { get; protected set; }
        public long Gold { get; protected set; }
        public long Silver { get; protected set; }
        public IItem What { get; protected set; }

        public Steal(IRandomManager randomManager, IGameActionManager gameActionManager, ISettings settings)
            : base(randomManager)
        {
            GameActionManager = gameActionManager;
            Settings = settings;
        }

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            if (skillActionInput.Parameters.Length < 2)
                return "Steal what from whom?";

            Victim = FindHelpers.FindByName(User.Room.People, skillActionInput.Parameters[0]);
            if (Victim == null)
                return "They aren't here.";

            if (Victim == Actor)
                return "That's pointless.";

            string isSafeResult = Victim.IsSafe(Actor);
            if (isSafeResult != null)
                return isSafeResult;

            if (Victim is INonPlayableCharacter && Victim.Fighting != null)
                return "Kill stealing is not permitted. You'd better not -- you might get hit.";

            Failed = CheckIfFailed();
            if (Failed)
                return null;

            ICommandParameter whatParameter = skillActionInput.Parameters[1];
            if (StringCompareHelpers.StringEquals(whatParameter.Value, "coin")
                || StringCompareHelpers.StringEquals(whatParameter.Value, "coins")
                || StringCompareHelpers.StringEquals(whatParameter.Value, "silver")
                || StringCompareHelpers.StringEquals(whatParameter.Value, "gold"))
            {
                Gold = (Victim.GoldCoins * RandomManager.Range(1, Actor.Level)) / Settings.MaxLevel;
                Silver = (Victim.SilverCoins * RandomManager.Range(1, Actor.Level)) / Settings.MaxLevel;
                if (Gold <= 0 && Silver <= 0)
                    return "You couldn't get any coins.";
                return null;
            }

            What = FindHelpers.FindByName(Victim.Inventory ?? Victim.Equipments.Where(x => x.Item != null).Select(x => x.Item), whatParameter);
            if (What == null)
                return "You can't find it.";
            if (What.ItemFlags.HasFlag(ItemFlags.NoDrop)
                || What.ItemFlags.HasFlag(ItemFlags.Inventory)
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
                Actor.RemoveAuras(x => x.AbilityName == Sneak.SkillName, true); // recompute

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

                    GameActionManager.Execute(Victim, "Yell", new CommandParameter(msg, msg, false));
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
            IPlayableCharacter pcActor = Actor as IPlayableCharacter;
            IPlayableCharacter pcVictim = Victim as IPlayableCharacter;
            if (pcActor != null)
            {
                if (
                    (pcVictim != null && (Actor.Level + 7 < Victim.Level || Actor.Level - 7 > Victim.Level))
                    || percent > Learned
                // || pcActor.Clan == null
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
