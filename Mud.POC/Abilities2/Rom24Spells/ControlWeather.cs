using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Control Weather", AbilityEffects.None)]
    public class ControlWeather : NoTargetSpellBase
    {
        private bool _isBetterRequired;

        private ITimeManager TimeManager { get; }
        public ControlWeather(IRandomManager randomManager, IWiznet wiznet, ITimeManager timeManager) 
            : base(randomManager, wiznet)
        {
            TimeManager = timeManager;
        }

        protected override void Invoke()
        {
            int value = RandomManager.Dice(Level / 3, 4) * (_isBetterRequired ? 1 : -1);
            TimeManager.ChangePressure(value);
            Caster.Send("Ok.");
        }

        public override string Setup(AbilityActionInput actionInput)
        {
            string baseGuards = base.Setup(actionInput);
            if (baseGuards != null)
                return null;
            if (StringCompareHelpers.StringEquals(actionInput.RawParameters, "better"))
            {
                _isBetterRequired = true;
                return null;
            }
            if (StringCompareHelpers.StringEquals(actionInput.RawParameters, "worse"))
            {
                _isBetterRequired = false;
                return null;
            }
            return "Do you want it to get better or worse?";
        }
    }
}
