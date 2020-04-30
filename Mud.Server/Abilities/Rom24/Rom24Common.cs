using Mud.Domain;
using Mud.Server.Common;
using System.Linq;

namespace Mud.Server.Abilities.Rom24
{
    public class Rom24Common
    {
        private IRandomManager RandomManager { get; }

        public Rom24Common(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public bool SavesSpell(int level, ICharacter victim, SchoolTypes damageType)
        {
            int save = 50 + (victim.Level - level) * 5 - victim.CurrentAttributes(CharacterAttributes.SavingThrow) * 2;
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Berserk))
                save += victim.Level / 2;
            ResistanceLevels resist = victim.CheckResistance(damageType);
            switch (resist)
            {
                case ResistanceLevels.Immune:
                    return true;
                case ResistanceLevels.Resistant:
                    save += 2;
                    break;
                case ResistanceLevels.Vulnerable:
                    save -= 2;
                    break;
            }
            if (victim.Class?.CurrentResourceKinds(victim.Form).Contains(ResourceKinds.Mana) == true)
                save = (save * 9) / 10;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        public bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft)
        {
            if (pulseLeft == -1) // very hard to dispel permanent effects
                spellLevel += 5;

            int save = 50 + (spellLevel - dispelLevel) * 5;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        public bool IsSafeSpell(ICharacter caster, ICharacter victim, bool area)
        {
            if (!victim.IsValid || victim.Room == null || !caster.IsValid || caster.Room == null)
                return true;
            if (area && caster == victim)
                return true;
            if (victim.Fighting == caster || victim == caster)
                return false;
            if (!area && (caster is IPlayableCharacter pcCaster && pcCaster.ImpersonatedBy is IAdmin))
                return false;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                // safe room ?
                if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                    return true;
                // TODO: No fight in a shop -> send_to_char("The shopkeeper wouldn't like that.\n\r",ch);
                // TODO: Can't killer trainer, practicer, healer, changer, questor  -> send_to_char("I don't think Mota would approve.\n\r",ch);

                // Npc doing the killing
                if (caster is INonPlayableCharacter)
                {
                    // no pets
                    if (npcVictim.ActFlags.HasFlag(ActFlags.Pet))
                        return true;
                    // no charmed creatures unless owner
                    if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm) && (area || caster != victim.ControlledBy))
                        return true;
                    // TODO: legal kill? -- cannot hit mob fighting non-group member
                    //if (victim->fighting != NULL && !is_same_group(ch,victim->fighting)) -> true
                }
                // Player doing the killing
                else
                {
                    // TODO: area effect spells do not hit other mobs
                    //if (area && !is_same_group(victim,ch->fighting)) -> true
                }
            }
            // Killing players
            else
            {
                if (area && (victim is IPlayableCharacter pcVictim && pcVictim.ImpersonatedBy is IAdmin))
                    return true;
                // Npc doing the killing
                if (caster is INonPlayableCharacter npcCaster)
                {
                    // charmed mobs and pets cannot attack players while owned
                    if (caster.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm) && caster.ControlledBy != null && caster.ControlledBy.Fighting != victim)
                        return true;
                    // safe room
                    if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                        return true;
                    // TODO:  legal kill? -- mobs only hit players grouped with opponent
                    //if (ch->fighting != NULL && !is_same_group(ch->fighting, victim))
                }
                // Player doing the killing
                else
                {
                    // TODO: PK
                    //if (!is_clan(ch))
                    //    return true;

                    //if (IS_SET(victim->act, PLR_KILLER) || IS_SET(victim->act, PLR_THIEF))
                    //    return FALSE;

                    //if (!is_clan(victim))
                    //    return true;

                    //if (ch->level > victim->level + 8)
                    //    return true;
                }
            }
            return false;
        }

        public bool IsSafe(ICharacter character, ICharacter victim)
        {
            if (!victim.IsValid || victim.Room == null || !character.IsValid || character.Room == null)
                return true;
            if (victim.Fighting == character || victim == character)
                return false;
            if (character is IPlayableCharacter pcCaster && pcCaster.ImpersonatedBy is IAdmin)
                return false;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                {
                    character.Send("Not in the room.");
                    return true;
                }
                // TODO: No fight in a shop -> send_to_char("The shopkeeper wouldn't like that.\n\r",ch);
                // TODO: Can't killer trainer, practicer, healer, changer, questor -> send_to_char("I don't think Mota would approve.\n\r",ch);

                // Player doing the killing
                if (character is IPlayableCharacter)
                {
                    // no pets
                    if (npcVictim.ActFlags.HasFlag(ActFlags.Pet))
                    {
                        character.Act(ActOptions.ToCharacter, "But {0} looks so cute and cuddly...", victim);
                        return true;
                    }
                    // no charmed creatures unless owner
                    if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm) && character != victim.ControlledBy)
                    {
                        character.Send("You don't own that monster.");
                        return true;
                    }
                }
            }
            // Killing player
            else
            {
                // Npc doing the killing
                if (character is INonPlayableCharacter npcCaster)
                {
                    // safe room
                    if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                    {
                        character.Send("Not in the room.");
                        return true;
                    }
                    // charmed mobs and pets cannot attack players while owned
                    if (character.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm) && character.ControlledBy != null && character.ControlledBy.Fighting != victim)
                    {
                        character.Send("Players are your friends!");
                        return true;
                    }
                }
                // Player doing the killing
                else
                {
                    //if (!is_clan(ch))
                    //{
                    //    send_to_char("Join a clan if you want to kill players.\n\r", ch);
                    //    return true;
                    //}

                    //if (IS_SET(victim->act, PLR_KILLER) || IS_SET(victim->act, PLR_THIEF))
                    //    return FALSE;

                    //if (!is_clan(victim))
                    //{
                    //    send_to_char("They aren't in a clan, leave them alone.\n\r", ch);
                    //    return true;
                    //}

                    //if (ch->level > victim->level + 8)
                    //{
                    //    send_to_char("Pick on someone your own size.\n\r", ch);
                    //    return true;
                    //}
                }
            }
            return false;
        }

    }
}
