using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Ability.Spell
{
    public abstract class TransportationSpellBase : SpellBase
    {
        private ICharacterManager CharacterManager { get; }

        protected ICharacter Victim { get; set; }

        protected TransportationSpellBase(IRandomManager randomManager, ICharacterManager characterManager)
            : base(randomManager)
        {
            CharacterManager = characterManager;
        }

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter ?? Caster;
                return null;
            }

            if (spellActionInput.Parameters.Length < 1)
                Victim = Caster;
            else
            {
                Victim = FindHelpers.FindChararacterInWorld(CharacterManager, Caster, spellActionInput.Parameters[0]);
                if (Victim == null)
                    return "They aren't here.";
            }
            if (!IsVictimValid())
                return "You failed.";
            return null;
        }

        protected virtual bool IsVictimValid()
        {
            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            if (Victim == Caster
                || Victim.Room == null
                || !Caster.CanSee(Victim.Room)
                || Caster.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || Caster.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Private)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Solitary)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.ImpOnly)
                || Victim.Level >= Level + 3
                // TODO: clan check
                // TODO: hero level check 
                || (npcVictim != null && npcVictim.Immunities.HasFlag(IRVFlags.Summon))
                || (npcVictim != null && Victim.SavesSpell(Level, SchoolTypes.Other)))
                return false;
            return true;
        }

        protected virtual void AutoLook(ICharacter victim)
        {
            StringBuilder sb = new StringBuilder();
            victim.Room.Append(sb, victim);
            victim.Send(sb);
        }
    }
}
