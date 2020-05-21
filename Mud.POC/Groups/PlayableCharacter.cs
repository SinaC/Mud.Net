using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Groups
{
    public partial class PlayableCharacter : CharacterBase, IPlayableCharacter
    {
        private List<INonPlayableCharacter> _pets;

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
            _pets.Add(pet);
            pet.ChangeMaster(this);
        }

        public void RemovePet(INonPlayableCharacter pet)
        {
            if (!_pets.Contains(pet))
                return;
            _pets.Remove(pet);
            pet.ChangeMaster(null);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();

            // Leave group
            Group?.RemoveMember(this);

            // Release pets
            foreach (INonPlayableCharacter pet in _pets)
                pet.ChangeMaster(null);
            _pets.Clear();
        }

        #endregion
    }
}
