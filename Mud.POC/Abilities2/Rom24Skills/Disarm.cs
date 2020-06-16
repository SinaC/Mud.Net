using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Passives;
using Mud.Server.Random;
using Mud.Server.GameAction;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("disarm", "Abilities", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24)]
    public class Disarm : FightingSkillBase
    {
        public const string SkillName = "Disarm";

        protected int Hand2HandLearned { get; set; }
        protected IItemWeapon UserWield { get; set; }
        protected IItemWeapon VictimWield { get; set; }

        public Disarm(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (Learned == 0)
                return "You don't know how to disarm opponents.";

            INonPlayableCharacter npcUser = User as INonPlayableCharacter;

            // User has a weapon or hand to hand
            UserWield = User.GetEquipment(EquipmentSlots.MainHand) as IItemWeapon;
            Hand2HandLearned = User.GetAbilityLearned(HandToHand.PassiveName).percentage;
            if (UserWield == null
                && (Hand2HandLearned == 0
                    || (npcUser != null && !npcUser.OffensiveFlags.HasFlag(OffensiveFlags.Disarm))))
                return "You must wield a weapon to disarm.";

            // Victim wield
            VictimWield = Victim.GetEquipment(EquipmentSlots.MainHand) as IItemWeapon;
            if (VictimWield == null)
                return "Your opponent is not wielding a weapon.";

            return null;
        }

        protected override bool Invoke()
        {

            // find weapon learned
            int UserLearned = User.GetWeaponLearned(UserWield).percentage;
            int VictimLearned = Victim.GetWeaponLearned(UserWield).percentage;
            int UserOnVictimWeaponLearned = User.GetWeaponLearned(VictimWield).percentage;

            int chance = Learned;
            // modifiers
            // skill
            if (UserWield == null)
                chance = (chance * Hand2HandLearned) / 150;
            else
                chance = chance * UserLearned / 100;
            chance += (UserOnVictimWeaponLearned / 2 - VictimLearned) / 2;
            // dex vs. strength
            chance += User[CharacterAttributes.Dexterity];
            chance -= 2 * Victim[CharacterAttributes.Strength];
            // level
            chance += (User.Level - Victim.Level) * 2;
            // and now the attack
            if (RandomManager.Chance(chance))
            {
                if (VictimWield.ItemFlags.HasFlag(ItemFlags.NoRemove))
                {
                    User.Act(ActOptions.ToCharacter, "{0:S} weapon won't budge!", Victim);
                    Victim.Act(ActOptions.ToCharacter, "{0:N} tries to disarm you, but your weapon won't budge!", User);
                    Victim.Act(ActOptions.ToRoom, "{0} tries to disarm {1}, but fails.", User, Victim); // equivalent of NO_NOTVICT
                }

                Victim.Act(ActOptions.ToCharacter, "{0:N} DISARMS you and sends your weapon flying!", User);
                Victim.Act(ActOptions.ToRoom, "{0:N} disarm{0:v} {1}", User, Victim);

                VictimWield.ChangeEquippedBy(null, true);
                if (!VictimWield.ItemFlags.HasFlag(ItemFlags.NoDrop) && !VictimWield.ItemFlags.HasFlag(ItemFlags.Inventory))
                {
                    VictimWield.ChangeContainer(Victim.Room);
                    // TODO: NPC tries to get its weapon back
                    //if (Victim is INonPlayableCharacter && Victim.CanSee(VictimWield)) // && .Wait == 0 ???
                    //    Victim.GetItem(VictimWield);
                }

                // TODO  check_killer(ch, Victim);
                return true;
            }
            else
            {
                User.Act(ActOptions.ToCharacter, "You fail to disarm {0}.", Victim);
                User.Act(ActOptions.ToRoom, "{0:N} tries to disarm {1}, but fails.", User, Victim);
                // TODO  check_killer(ch, Victim);
                return false;
            }
        }
    }
}
