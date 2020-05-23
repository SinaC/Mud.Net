using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.GroupsPetsFollowers
{
    public partial class PlayableCharacter : CharacterBase, IPlayableCharacter
    {
        private readonly List<INonPlayableCharacter> _pets;

        public PlayableCharacter(string name, IRoom room)
            : base(name, room)
        {
            _pets = new List<INonPlayableCharacter>();
        }

        #region IPlayableCharacter

        public long Experience { get; protected set; }

        public long ExperienceToLevel => Level * 1000 - Experience;

        public IEnumerable<INonPlayableCharacter> Pets => _pets;

        public IGroup Group { get; protected set; }

        public void ChangeGroup(IGroup group)
        {
            if (Group != null && group != null)
                return; // cannot change from one group to another
            Group = group;
        }

        public void AddPet(INonPlayableCharacter pet)
        {
            if (_pets.Contains(pet))
                return;
            if (pet.Master != null) // cannot change master
                return;
            AddFollower(pet);
            _pets.Add(pet);
            pet.ChangeMaster(this);
        }

        public void RemovePet(INonPlayableCharacter pet)
        {
            if (!_pets.Contains(pet))
                return;
            RemoveFollower(pet);
            _pets.Remove(pet);
            pet.ChangeMaster(null);
            World.RemoveCharacter(pet);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();

            // Leave group
            if (Group != null)
            {
                if (Group.Members.Count() <= 2) // group will contain only one member, disband
                    Group.Disband();
                else
                    Group.RemoveMember(this);
            }

            // Release pets
            foreach (INonPlayableCharacter pet in _pets)
            {
                if (pet.Room != null)
                    pet.Act(ActOptions.ToRoom, "{0:N} slowly fades away.", pet);
                RemoveFollower(pet);
                pet.ChangeMaster(null);
                World.RemoveCharacter(pet);
            }
            _pets.Clear();
        }

        #endregion
    }
}
