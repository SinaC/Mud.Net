using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.None)]
    public class ControlWeather : NoTargetSpellBase
    {
        public const string SpellName = "Control Weather";

        private bool _isBetterRequired;

        private ITimeManager TimeManager { get; }
        public ControlWeather(IRandomManager randomManager, ITimeManager timeManager) 
            : base(randomManager)
        {
            TimeManager = timeManager;
        }

        protected override void Invoke()
        {
            int value = RandomManager.Dice(Level / 3, 4) * (_isBetterRequired ? 1 : -1);
            TimeManager.ChangePressure(value);
            Caster.Send("Ok.");
        }

        public override string Setup(ISpellActionInput spellActionInput)
        {
            string baseSetup = base.Setup(spellActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (StringCompareHelpers.StringEquals(spellActionInput.RawParameters, "better"))
            {
                _isBetterRequired = true;
                return null;
            }
            if (StringCompareHelpers.StringEquals(spellActionInput.RawParameters, "worse"))
            {
                _isBetterRequired = false;
                return null;
            }
            return "Do you want it to get better or worse?";
        }
    }
}
