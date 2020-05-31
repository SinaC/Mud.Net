using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DetectHidden : CharacterFlagsSpellBase, IAbilityDispellable
    {
        public override int Id => 33;
        public override string Name => "Detect Hidden";
        public override AbilityEffects Effects => base.Effects | AbilityEffects.Detection;
        public override string CharacterWearOffMessage => "You feel less aware of your surroundings.";
        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectHidden;
        protected override string SelfAlreadyAffected => "You are already as alert as you can be.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already sense hidden lifeforms.";
        protected override string Success => "Your awareness improves.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration(int level) => TimeSpan.FromMinutes(level);
        public string DispelRoomMessage => string.Empty;

        public DetectHidden(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

    }
}
