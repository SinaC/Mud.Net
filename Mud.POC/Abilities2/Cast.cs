using System;
using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class Cast : IGameAction<ActionInput>
    {
        protected AbilityInfo AbilityInfo { get; set; }
        protected ICharacter Caster { get; set; }
        protected string RawParameters { get; set; }
        protected CommandParameter[] Parameters { get; set; }

        public string Guards(ActionInput actionInput)
        {
            // TODO: check if enough parameters, get spell name and parameters
            throw new NotImplementedException();
        }

        public void Execute(ActionInput actionInput)
        {
            var abilityActionInput = new AbilityActionInput(AbilityInfo, Caster, RawParameters, Parameters);
            // TODO: we have to register every GameAction in DependencyContainer
            IAbilityAction abilityInstance = (IAbilityAction)DependencyContainer.Current.GetInstance(AbilityInfo.AbilityExecutionType);
            abilityInstance.Execute(abilityActionInput);
        }
    }
}
