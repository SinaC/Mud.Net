using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mud.Logger;

namespace Mud.Repository.Filesystem
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // External to Internal
            ExternalToInternal();

            // Internal to External
            InternalToExternal();
        }

        private void ExternalToInternal()
        {
            CreateMap<Domain.PlayerData, DataContracts.PlayerData>()
                .Include<Domain.AdminData, DataContracts.AdminData>()
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapFromDictionary(x.Aliases)));
            CreateMap<Domain.AdminData, DataContracts.AdminData>()
                .ForMember(x => x.Level, expression => expression.MapFrom(x => MapAdminLevel(x.Level)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Domain.CharacterData, DataContracts.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)));

            CreateMap<Domain.ItemData, DataContracts.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Domain.ItemContainerData, DataContracts.ItemContainerData>()
                .Include<Domain.ItemCorpseData, DataContracts.ItemCorpseData>();
            CreateMap<Domain.ItemContainerData, DataContracts.ItemContainerData>();
            CreateMap<Domain.ItemCorpseData, DataContracts.ItemCorpseData>();

            CreateMap<Domain.EquipedItemData, DataContracts.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<Domain.CurrentQuestData, DataContracts.CurrentQuestData>();
            CreateMap<Domain.CurrentQuestObjectiveData, DataContracts.CurrentQuestObjectiveData>();

            CreateMap<Domain.AuraData, DataContracts.AuraData>()
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapAuraModifier(x.Modifier)))
                .ForMember(x => x.AmountOperator, expression => expression.MapFrom(x => MapAmountOperator(x.AmountOperator)));
        }

        private void InternalToExternal()
        {
            CreateMap<DataContracts.PlayerData, Domain.PlayerData>()
                .Include<DataContracts.AdminData, Domain.AdminData>()
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapToDictionary(x.Aliases)));
            CreateMap<DataContracts.AdminData, Domain.AdminData>()
                .ForMember(x => x.Level, expression => expression.MapFrom(x => MapAdminLevel(x.Level)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<DataContracts.CharacterData, Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)));

            CreateMap<DataContracts.ItemData, Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<DataContracts.ItemContainerData, Domain.ItemContainerData>()
                .Include<DataContracts.ItemCorpseData, Domain.ItemCorpseData>();
            CreateMap<DataContracts.ItemContainerData, Domain.ItemContainerData>();
            CreateMap<DataContracts.ItemCorpseData, Domain.ItemCorpseData>();

            CreateMap<DataContracts.EquipedItemData, Domain.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<DataContracts.CurrentQuestData, Domain.CurrentQuestData>();
            CreateMap<DataContracts.CurrentQuestObjectiveData, Domain.CurrentQuestObjectiveData>();

            CreateMap<DataContracts.AuraData, Domain.AuraData>()
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapAuraModifier(x.Modifier)))
                .ForMember(x => x.AmountOperator, expression => expression.MapFrom(x => MapAmountOperator(x.AmountOperator)));
        }

        private DataContracts.PairData<TKey, TValue>[] MapFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) => dictionary.Select(x => new DataContracts.PairData<TKey, TValue>(x.Key, x.Value)).ToArray();

        private Dictionary<TKey, TValue> MapToDictionary<TKey, TValue>(DataContracts.PairData<TKey, TValue>[] array) => array.ToDictionary(x => x.Key, x => x.Value);

        private Domain.WiznetFlags MapWiznetFlags(int flags)
        {
            return (Domain.WiznetFlags)flags;
        }

        private int MapWiznetFlags(Domain.WiznetFlags flags)
        {
            return (int)flags;
        }

        private Domain.AdminLevels MapAdminLevel(int level)
        {
            switch (level)
            {
                case 0:
                    return Domain.AdminLevels.Angel;
                case 1:
                    return Domain.AdminLevels.DemiGod;
                case 2:
                    return Domain.AdminLevels.Immortal;
                case 3:
                    return Domain.AdminLevels.God;
                case 4:
                    return Domain.AdminLevels.Deity;
                case 5:
                    return Domain.AdminLevels.Supremacy;
                case 6:
                    return Domain.AdminLevels.Creator;
                case 7:
                    return Domain.AdminLevels.Implementor;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AdminLevels {level} while reading pfile");
                    return 0;
            }
        }

        private int MapAdminLevel(Domain.AdminLevels level)
        {
            switch (level)
            {
                case Domain.AdminLevels.Angel:
                    return 0;
                case Domain.AdminLevels.DemiGod:
                    return 1;
                case Domain.AdminLevels.Immortal:
                    return 2;
                case Domain.AdminLevels.God:
                    return 3;
                case Domain.AdminLevels.Deity:
                    return 4;
                case Domain.AdminLevels.Supremacy:
                    return 5;
                case Domain.AdminLevels.Creator:
                    return 6;
                case Domain.AdminLevels.Implementor:
                    return 7;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AdminLevels {level} while writing pfile");
                    return 0;
            }
        }

        private Domain.Sex MapSex(int sex)
        {
            switch (sex)
            {
                case 0:
                    return Domain.Sex.Neutral;
                case 1:
                    return Domain.Sex.Male;
                case 2:
                    return Domain.Sex.Female;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sex {sex} while reading pfile");
                    return Domain.Sex.Neutral;
            }
        }

        private int MapSex(Domain.Sex sex)
        {
            switch (sex)
            {
                case Domain.Sex.Neutral:
                    return 0;
                case Domain.Sex.Male:
                    return 1;
                case Domain.Sex.Female:
                    return 2;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sex {sex} while writing pfile");
                    return 0;
            }
        }

        private Domain.EquipmentSlots MapEquimentSlot(int slot)
        {
            switch (slot)
            {
                case 0:
                    return Domain.EquipmentSlots.None;
                case 1:
                    return Domain.EquipmentSlots.Light;
                case 2:
                    return Domain.EquipmentSlots.Head;
                case 3:
                    return Domain.EquipmentSlots.Amulet;
                case 4:
                    return Domain.EquipmentSlots.Shoulders;
                case 5:
                    return Domain.EquipmentSlots.Chest;
                case 6:
                    return Domain.EquipmentSlots.Cloak;
                case 7:
                    return Domain.EquipmentSlots.Waist;
                case 8:
                    return Domain.EquipmentSlots.Wrists;
                case 9:
                    return Domain.EquipmentSlots.Arms;
                case 10:
                    return Domain.EquipmentSlots.Hands;
                case 11:
                    return Domain.EquipmentSlots.Ring;
                case 12:
                    return Domain.EquipmentSlots.Legs;
                case 13:
                    return Domain.EquipmentSlots.Feet;
                case 14:
                    return Domain.EquipmentSlots.Trinket;
                case 15:
                    return Domain.EquipmentSlots.MainHand;
                case 16:
                    return Domain.EquipmentSlots.OffHand;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid EquipmentSlots {slot} while reading pfile");
                    return 0;
            }
        }

        private int MapEquimentSlot(Domain.EquipmentSlots slot)
        {
            switch (slot)
            {
                case Domain.EquipmentSlots.None:
                    return 0;
                case Domain.EquipmentSlots.Light:
                    return 1;
                case Domain.EquipmentSlots.Head:
                    return 2;
                case Domain.EquipmentSlots.Amulet:
                    return 3;
                case Domain.EquipmentSlots.Shoulders:
                    return 4;
                case Domain.EquipmentSlots.Chest:
                    return 5;
                case Domain.EquipmentSlots.Cloak:
                    return 6;
                case Domain.EquipmentSlots.Waist:
                    return 7;
                case Domain.EquipmentSlots.Wrists:
                    return 8;
                case Domain.EquipmentSlots.Arms:
                    return 9;
                case Domain.EquipmentSlots.Hands:
                    return 10;
                case Domain.EquipmentSlots.Ring:
                    return 11;
                case Domain.EquipmentSlots.Legs:
                    return 12;
                case Domain.EquipmentSlots.Feet:
                    return 13;
                case Domain.EquipmentSlots.Trinket:
                    return 14;
                case Domain.EquipmentSlots.MainHand:
                    return 15;
                case Domain.EquipmentSlots.OffHand:
                    return 16;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid EquipmentSlots {slot} while writing pfile");
                    return 0;
            }
        }

        private Domain.ItemFlags MapItemFlags(int flags)
        {
            return (Domain.ItemFlags)flags;
        }

        private int MapItemFlags(Domain.ItemFlags flags)
        {
            return (int)flags;
        }

        private Domain.AuraModifiers MapAuraModifier(int modifier)
        {
            switch (modifier)
            {
                case 0: return Domain.AuraModifiers.None;
                case 1: return Domain.AuraModifiers.Strength;
                case 2: return Domain.AuraModifiers.Agility;
                case 3: return Domain.AuraModifiers.Stamina;
                case 4: return Domain.AuraModifiers.Intellect;
                case 5: return Domain.AuraModifiers.Spirit;
                case 6: return Domain.AuraModifiers.Characteristics;
                case 7: return Domain.AuraModifiers.AttackSpeed;
                case 8: return Domain.AuraModifiers.AttackPower;
                case 9: return Domain.AuraModifiers.SpellPower;
                case 10: return Domain.AuraModifiers.MaxHitPoints;
                case 11: return Domain.AuraModifiers.DamageAbsorb;
                case 12: return Domain.AuraModifiers.HealAbsorb;
                case 13: return Domain.AuraModifiers.Armor;
                case 14: return Domain.AuraModifiers.Critical;
                case 15: return Domain.AuraModifiers.Dodge;
                case 16: return Domain.AuraModifiers.Parry;
                case 17: return Domain.AuraModifiers.Block;
                case 18: return Domain.AuraModifiers.CharacterFlags;
                case 19: return Domain.AuraModifiers.SavingThrow;
                case 20: return Domain.AuraModifiers.HitRoll;
                case 21: return Domain.AuraModifiers.DamRoll;
                case 22: return Domain.AuraModifiers.Immunities;
                case 23: return Domain.AuraModifiers.Resistances;
                case 24: return Domain.AuraModifiers.Vulnerabilities;
                case 25: return Domain.AuraModifiers.Sex;
                case 26: return Domain.AuraModifiers.ItemFlags;
                case 27: return Domain.AuraModifiers.Dexterity;
                case 28: return Domain.AuraModifiers.MaxMovePoints;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraModifier {modifier} while reading pfile");
                    return Domain.AuraModifiers.None;
            }
        }

        private int MapAuraModifier(Domain.AuraModifiers modifier)
        {
            switch (modifier)
            {
                case Domain.AuraModifiers.None: return 0;
                case Domain.AuraModifiers.Strength: return 1;
                case Domain.AuraModifiers.Agility: return 2;
                case Domain.AuraModifiers.Stamina: return 3;
                case Domain.AuraModifiers.Intellect: return 4;
                case Domain.AuraModifiers.Spirit: return 5;
                case Domain.AuraModifiers.Characteristics: return 6;
                case Domain.AuraModifiers.AttackSpeed: return 7;
                case Domain.AuraModifiers.AttackPower: return 8;
                case Domain.AuraModifiers.SpellPower: return 9;
                case Domain.AuraModifiers.MaxHitPoints: return 10;
                case Domain.AuraModifiers.DamageAbsorb: return 11;
                case Domain.AuraModifiers.HealAbsorb: return 12;
                case Domain.AuraModifiers.Armor: return 13;
                case Domain.AuraModifiers.Critical: return 14;
                case Domain.AuraModifiers.Dodge: return 15;
                case Domain.AuraModifiers.Parry: return 16;
                case Domain.AuraModifiers.Block: return 17;
                case Domain.AuraModifiers.CharacterFlags: return 18;
                case Domain.AuraModifiers.SavingThrow: return 19;
                case Domain.AuraModifiers.HitRoll: return 20;
                case Domain.AuraModifiers.DamRoll: return 21;
                case Domain.AuraModifiers.Immunities: return 22;
                case Domain.AuraModifiers.Resistances: return 23;
                case Domain.AuraModifiers.Vulnerabilities: return 24;
                case Domain.AuraModifiers.Sex: return 25;
                case Domain.AuraModifiers.ItemFlags: return 26;
                case Domain.AuraModifiers.Dexterity: return 27;
                case Domain.AuraModifiers.MaxMovePoints: return 28;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraModifier {modifier} while writing pfile");
                    return 0;
            }
        }

        private Domain.AmountOperators MapAmountOperator(int op)
        {
            switch (op)
            {
                case 0: return Domain.AmountOperators.None;
                case 1: return Domain.AmountOperators.Fixed;
                case 2: return Domain.AmountOperators.Percentage;
                case 3: return Domain.AmountOperators.Flags;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraOperator {op} while reading pfile");
                    return Domain.AmountOperators.None;
            }
        }

        private int MapAmountOperator(Domain.AmountOperators op)
        {
            switch (op)
            {
                case Domain.AmountOperators.None: return 0;
                case Domain.AmountOperators.Fixed: return 1;
                case Domain.AmountOperators.Percentage: return 2;
                case Domain.AmountOperators.Flags: return 3;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraOperator {op} while writing pfile");
                    return 0;
            }
        }
    }
}
