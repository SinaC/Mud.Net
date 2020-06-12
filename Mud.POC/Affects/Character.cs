using Mud.Common;
using Mud.Server.Random;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Affects
{

    // TODO
    public class Character : EntityBase, ICharacter
    {
        private readonly List<IItem> _inventory;
        private readonly List<IItem> _equipments;

        private readonly int[] _baseAttributes;
        private readonly int[] _currentAttributes;

        public Character(string name)
            : base(name)
        {
            _inventory = new List<IItem>();
            _equipments = new List<IItem>();
            _baseAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _currentAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
        }

        public IRoom Room { get; private set; }

        public IEnumerable<IItem> Inventory => _inventory;

        public IEnumerable<IItem> Equipments => _equipments;

        public CharacterFlags BaseCharacterFlags { get; private set; }
        public CharacterFlags CurrentCharacterFlags { get; private set; }

        public IRVFlags BaseImmunities { get; private set; }
        public IRVFlags CurrentImmunities { get; private set; }
        public IRVFlags BaseResistances { get; private set; }
        public IRVFlags CurrentResistances { get; private set; }
        public IRVFlags BaseVulnerabilities { get; private set; }
        public IRVFlags CurrentVulnerabilities { get; private set; }

        public int BaseAttributes(CharacterAttributes attribute) => _baseAttributes[(int)attribute];
        public int CurrentAttributes(CharacterAttributes attribute) => _currentAttributes[(int)attribute];

        public Sex BaseSex { get; private set; }
        public Sex CurrentSex { get; private set; }

        public void ApplyAffect(CharacterFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    CurrentCharacterFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    CurrentCharacterFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    CurrentCharacterFlags &= ~affect.Modifier;
                    break;
                default:
                    break;
            }
            return;
        }

        public void ApplyAffect(CharacterIRVAffect affect)
        {
            switch (affect.Location)
            {
                case IRVAffectLocations.Immunities:
                    switch (affect.Operator)
                    {
                        case AffectOperators.Add:
                        case AffectOperators.Or:
                            CurrentImmunities |= affect.Modifier;
                            break;
                        case AffectOperators.Assign:
                            CurrentImmunities = affect.Modifier;
                            break;
                        case AffectOperators.Nor:
                            CurrentImmunities &= ~affect.Modifier;
                            break;
                        default:
                            break;
                    }
                    break;
                case IRVAffectLocations.Resistances:
                    switch (affect.Operator)
                    {
                        case AffectOperators.Add:
                        case AffectOperators.Or:
                            CurrentResistances |= affect.Modifier;
                            break;
                        case AffectOperators.Assign:
                            CurrentResistances = affect.Modifier;
                            break;
                        case AffectOperators.Nor:
                            CurrentResistances &= ~affect.Modifier;
                            break;
                        default:
                            break;
                    }
                    break;
                case IRVAffectLocations.Vulnerabilities:
                    switch (affect.Operator)
                    {
                        case AffectOperators.Add:
                        case AffectOperators.Or:
                            CurrentResistances |= affect.Modifier;
                            break;
                        case AffectOperators.Assign:
                            CurrentResistances = affect.Modifier;
                            break;
                        case AffectOperators.Nor:
                            CurrentResistances &= ~affect.Modifier;
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        public void ApplyAffect(CharacterAttributeAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                    _currentAttributes[(int)affect.Location] += affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    _currentAttributes[(int)affect.Location] = affect.Modifier;
                    break;
                case AffectOperators.Or:
                case AffectOperators.Nor:
                default:
                    // Error
                    break;
            }
        }

        public void ApplyAffect(CharacterSexAffect affect)
        {
            CurrentSex = affect.Value;
        }

        public override void Recompute()
        {
            // 0) Reset
            ResetAttributes();

            // 1) Apply room auras
            if (Room != null)
                ApplyAuras(Room);

            // 2) Apply equipment auras
            foreach(IItem equipment in Equipments)
                ApplyAuras(equipment);

            // 3) Apply own auras
            ApplyAuras(this);
        }

        protected override void ResetAttributes()
        {
            CurrentCharacterFlags = BaseCharacterFlags;
            CurrentImmunities = BaseImmunities;
            CurrentResistances = BaseResistances;
            CurrentVulnerabilities = BaseVulnerabilities;

            CurrentSex = BaseSex;
        }

        private void ApplyAuras(IEntity entity)
        {
            if (!entity.IsValid)
                return;
            foreach (IAura aura in entity.Auras.Where(x => x.IsValid))
            {
                foreach (ICharacterAffect affect in aura.Affects.OfType<ICharacterAffect>())
                {
                    affect.Apply(this);
                }
            }
        }
    }
}
