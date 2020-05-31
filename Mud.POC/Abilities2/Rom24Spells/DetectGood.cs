using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DetectGood : CharacterFlagsSpellBase, IAbilityDispellable
    {
        public override int Id => 32;
        public override string Name => "Detect Good";
        public override AbilityEffects Effects => base.Effects | AbilityEffects.Detection;
        public override string CharacterWearOffMessage => "The gold in your vision disappears.";
        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectGood;
        protected override string SelfAlreadyAffected => "You can already sense good.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect good.";
        protected override string Success => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration(int level) => TimeSpan.FromMinutes(level);
        public string DispelRoomMessage => string.Empty;

        public DetectGood(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

    }
}
