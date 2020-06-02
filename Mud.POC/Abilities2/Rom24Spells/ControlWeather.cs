using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ControlWeather : NoTargetSpellBase
    {
        public override int Id => 18;
        public override string Name => "Control Weather";
        public override AbilityEffects Effects => throw new NotImplementedException();

        private ITimeManager TimeManager { get; }
        public ControlWeather(IRandomManager randomManager, IWiznet wiznet, ITimeManager timeManager) 
            : base(randomManager, wiznet)
        {
            TimeManager = timeManager;
        }

        public override void Action(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            bool betterWanted;
            if (StringCompareHelpers.StringEquals(rawParameters, "better"))
                betterWanted = true;
            else if (StringCompareHelpers.StringEquals(rawParameters, "worse"))
                betterWanted = false;
            else
            {
                caster.Send("Do you want it to get better or worse?");
                return;
            }

            int value = RandomManager.Dice(level / 3, 4) * (betterWanted ? 1 : -1);
            TimeManager.ChangePressure(value);
            caster.Send("Ok");
        }
    }
}
