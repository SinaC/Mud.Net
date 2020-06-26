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

        public Summon(IRandomManager randomManager, ICharacterManager characterManager)
            : base(randomManager, characterManager)
        {
        }

        protected override void Invoke()
        {
            Victim.Act(ActOptions.ToRoom, "{0:N} disappears suddenly.", Victim);
            Victim.ChangeRoom(Caster.Room);
            Caster.Act(ActOptions.ToRoom, "{0:N} arrives suddenly", Victim);
            Victim.Act(ActOptions.ToCharacter, "{0:N} has summoned you!", Caster);
            AutoLook(Victim);
        }

        protected override bool IsVictimValid() // TODO: try to refactor to use base.IsVictimValid
        {
            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            IPlayableCharacter pcVictim = Victim as IPlayableCharacter;
            if (Victim == Caster
                || Victim.Room == null
                || Caster.Room.RoomFlags.HasAny("Safe", "NoRecall")
                || Victim.Room.RoomFlags.HasAny("Safe", "Private", "Solitary", "NoRecall", "ImpOnly")
                || (npcVictim != null && npcVictim.ActFlags.HasFlag(ActFlags.Aggressive))
                || Victim.Level >= Level + 3
                || pcVictim?.IsImmortal == true
                || Victim.Fighting != null
                || (npcVictim != null && npcVictim.Immunities.HasFlag(IRVFlags.Summon))
                || (npcVictim != null && (npcVictim.Blueprint is CharacterShopBlueprint))
                //TODO: plr_nosummon || playableCharacterVictim
                || (npcVictim != null && Victim.SavesSpell(Level, SchoolTypes.Other)))
                return false;
            return true;
        }
    }
}
