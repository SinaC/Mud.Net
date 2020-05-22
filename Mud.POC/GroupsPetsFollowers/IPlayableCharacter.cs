using Mud.Server.Input;
using System.Collections.Generic;

namespace Mud.POC.GroupsPetsFollowers
{
    public interface IPlayableCharacter : ICharacter
    {
        long Experience { get; }
        long ExperienceToLevel { get; }

        IEnumerable<INonPlayableCharacter> Pets { get; }

        IGroup Group { get; }

        void ChangeGroup(IGroup group);

        void AddPet(INonPlayableCharacter pet);
        void RemovePet(INonPlayableCharacter pet);

        // TEST PURPOSE
        CommandExecutionResults DoOrder(string rawParameters, params CommandParameter[] parameters); // order to one pet or all to do something
        CommandExecutionResults DoGroup(string rawParameters, params CommandParameter[] parameters); // display group info, add member
        CommandExecutionResults DoUngroup(string rawParameters, params CommandParameter[] parameters); // remove member
        CommandExecutionResults DoLeave(string rawParameters, params CommandParameter[] parameters); // leave a group
    }
}
