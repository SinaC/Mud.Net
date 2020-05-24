using Mud.Server.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [CharacterCommand("nofollow", "Group", "Movement")]
        [Syntax("[cmd]")]
        protected override CommandExecutionResults DoNofollow(string rawParameters, params CommandParameter[] parameters)
        {
            base.DoNofollow(rawParameters, parameters);
            // delete pets
            IReadOnlyCollection<INonPlayableCharacter> petsClone = new ReadOnlyCollection<INonPlayableCharacter>(Pets.ToList());
            foreach (INonPlayableCharacter pet in petsClone)
                RemovePet(pet);
            return CommandExecutionResults.Ok;
        }
    }
}
