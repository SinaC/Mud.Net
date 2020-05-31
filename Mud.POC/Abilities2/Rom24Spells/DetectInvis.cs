using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DetectInvis : CharacterFlagsSpellBase, IAbilityDispellable
    {
        public override int Id => 34;
        public override string Name => "Detect Invis";
        public override AbilityEffects Effects => base.Effects | AbilityEffects.Detection;
        public override string CharacterWearOffMessage => "You no longer see invisible objects.";
        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectInvis;
        protected override string SelfAlreadyAffected => "You can already see invisible.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already see invisible things.";
        protected override string Success => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration(int level) => TimeSpan.FromMinutes(level);
        public string DispelRoomMessage => string.Empty;

        public DetectInvis(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

    }
}
