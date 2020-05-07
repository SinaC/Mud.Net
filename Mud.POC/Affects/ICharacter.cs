using System.Collections.Generic;

namespace Mud.POC.Affects
{
    public interface ICharacter : IEntity
    {
        IRoom Room { get; }
        IEnumerable<IItem> Inventory { get; }
        IEnumerable<IItem> Equipments { get; }

        CharacterFlags BaseCharacterFlags { get; }
        CharacterFlags CurrentCharacterFlags { get; }

        IRVFlags BaseImmunities { get; }
        IRVFlags CurrentImmunities { get; }
        IRVFlags BaseResistances { get; }
        IRVFlags CurrentResistances { get; }
        IRVFlags BaseVulnerabilities { get; }
        IRVFlags CurrentVulnerabilities { get; }

        int BaseAttributes(CharacterAttributes attribute);
        int CurrentAttributes(CharacterAttributes attribute);

        Sex BaseSex { get; }
        Sex CurrentSex { get; }

        void ApplyAffect(CharacterFlagsAffect affect);
        void ApplyAffect(CharacterIRVAffect affect);
        void ApplyAffect(CharacterAttributeAffect affect);
        void ApplyAffect(CharacterSexAffect affect);
    }
}
