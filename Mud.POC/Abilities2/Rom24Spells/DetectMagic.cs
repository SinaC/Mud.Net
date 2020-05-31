using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DetectMagic : CharacterFlagsSpellBase, IAbilityDispellable
    {
        public override int Id => 35;
        public override string Name => "Detect Magic";
        public override AbilityEffects Effects => base.Effects | AbilityEffects.Detection;
        public override string CharacterWearOffMessage => "The detect magic wears off.";
        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectInvis;
        protected override string SelfAlreadyAffected => "You can already sense magical auras.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect magic.";
        protected override string Success => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration(int level) => TimeSpan.FromMinutes(level);
        public string DispelRoomMessage => string.Empty;

        public DetectMagic(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

    }
}
