using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Blueprints.Character;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Transportation)]
    public class Summon : TransportationSpellBase
    {
        public const string SpellName = "Summon";

        public Summon(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            Victim.Act(ActOptions.ToRoom, "{0:N} disappears suddenly.", Victim);
            Victim.ChangeRoom(Caster.Room);
            Caster.Act(ActOptions.ToRoom, "{0:N} arrives suddenly", Victim);
            Victim.Act(ActOptions.ToCharacter, "{0:N} has summoned you!", Caster);
            Victim.AutoLook();
        }

        protected override bool IsVictimValid() // TODO: try to refactor to use base.IsVictimValid
        {
            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            IPlayableCharacter pcVictim = Victim as IPlayableCharacter;
            if (Victim == Caster
                || Victim.Room == null
                || Caster.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || Caster.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Private)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Solitary)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.ImpOnly)
                || npcVictim?.ActFlags.HasFlag(ActFlags.Aggressive) == true
                || Victim.Level >= Level + 3
                || pcVictim?.IsImmortal == true
                || Victim.Fighting != null
                || npcVictim?.Immunities.HasFlag(IRVFlags.Summon) == true
                || (npcVictim?.Blueprint is CharacterShopBlueprint) == true
                //TODO: plr_nosummon || playableCharacterVictim
                || (npcVictim != null && Victim.SavesSpell(Level, SchoolTypes.Other)))
                return false;
            return true;
        }
    }
}
