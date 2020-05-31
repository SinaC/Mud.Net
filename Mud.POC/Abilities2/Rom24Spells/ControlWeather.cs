using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ControlWeather : SpellBase
    {
        public override int Id => 18;
        public override string Name => "Control Weather";
        public override AbilityEffects Effects => throw new NotImplementedException();

        private bool BetterWanted { get; set; }

        private ITimeManager TimeManager { get; }
        public ControlWeather(IRandomManager randomManager, IWiznet wiznet, ITimeManager timeManager) 
            : base(randomManager, wiznet)
        {
            TimeManager = timeManager;
        }

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            int value = RandomManager.Dice(level / 3, 4) * (BetterWanted ? 1 : -1);
            TimeManager.ChangePressure(value);
            caster.Send("Ok");
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            if (StringCompareHelpers.StringEquals(rawParameters, "better"))
            {
                BetterWanted = true;
                return AbilityTargetResults.Ok;
            }
            if (StringCompareHelpers.StringEquals(rawParameters, "worse"))
            {
                BetterWanted = false;
                return AbilityTargetResults.Ok;
            }
            caster.Send("Do you want it to get better or worse?");
            return AbilityTargetResults.InvalidTarget;
        }
    }
}
