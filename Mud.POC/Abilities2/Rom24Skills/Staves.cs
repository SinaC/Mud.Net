using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("brandish", "Abilities", "Skills")]
    [Skill(SkillName, AbilityEffects.None)]
    public class Staves : ItemCastSpellSkillBase<IItemStaff>
    {
        public const string SkillName = "Staves";

        public Staves(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
            : base(randomManager, abilityManager, itemManager)
        {
        }

        protected override bool Invoke()
        {
            // cast spell on every available targets
            return false;
        }

        protected override string SetTargets(SkillActionInput skillActionInput)
        {
            // TODO: every available targets
            return "Not Yet Implemented";
        }
        /*
        IItemStaff staff = source.GetEquipment<IItemStaff>(EquipmentSlots.OffHand);
            if (staff == null)
            {
                source.Send("You can brandish only with a staff.");
                return UseResults.InvalidTarget;
            }

            bool? success = null;
            if (staff.CurrentChargeCount > 0)
            {
                source.Act(ActOptions.ToAll, "{0:N} brandish{0:v} {1}.", source, staff);
                int chance = 20 + (4 * learned) / 5;
                if (source.Level < staff.Level
                    || !RandomManager.Chance(chance))
                {
                    source.Act(ActOptions.ToCharacter, "You fail to invoke {0}.", staff);
                    source.Act(ActOptions.ToRoom, "...and nothing happens.");
                    success = false;
                }
                else if (staff.Spell != null)
                {
                    INonPlayableCharacter npcSource = source as INonPlayableCharacter;
                    IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(source.Room.People.ToList()); // clone because victim can die and be removed from room
                    foreach (ICharacter victim in clone)
                    {
                        INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
                        bool cast = true;
                        switch (staff.Spell.Target)
                        {
                            case AbilityTargets.None:
                                if (victim == source)
                                    cast = false;
                                break;
                            case AbilityTargets.CharacterOffensive:
                                if (npcSource != null ? npcVictim != null : npcVictim == null)
                                    cast = false;
                                break;
                            case AbilityTargets.CharacterDefensive:
                                if (npcSource != null ? npcVictim == null : npcVictim != null)
                                    cast = false;
                                break;
                            case AbilityTargets.CharacterSelf:
                                if (victim == source)
                                    cast = false;
                                break;
                            default:
                                Wiznet.Wiznet($"SkillStaves: spell {staff.Spell} has invalid target in staff {staff.DebugName}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                                return UseResults.Error;
                        }
                        if (cast)
                            CastFromItem(staff.Spell, staff.SpellLevel, source, victim, rawParameters, parameters);
                    }
                    success = true;
                }
                else
                    Wiznet.Wiznet($"SkillStaves: no spell found in staff {staff.DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);

                staff.Use();
            }

            if (staff.CurrentChargeCount == 0)
            {
                source.Act(ActOptions.ToAll, "{0:P} {1} blazes bright and is gone.", source, staff);
                World.RemoveItem(staff);
            }
            return success.HasValue
                ? (success.Value
                    ? UseResults.Ok
                    : UseResults.Failed)
                : UseResults.InvalidParameter;
        */
    }
}
