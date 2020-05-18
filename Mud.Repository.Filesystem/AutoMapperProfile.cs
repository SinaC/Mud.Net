using System;
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
                .ForMember(x => x.AdminLevel, expression => expression.MapFrom(x => MapAdminLevel(x.AdminLevel)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Domain.CharacterData, DataContracts.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)))
                .ForMember(x => x.CharacterFlags, expression => expression.MapFrom(x => MapCharacterFlags(x.CharacterFlags)))
                .ForMember(x => x.Immunities, expression => expression.MapFrom(x => MapIRVFlags(x.Immunities)))
                .ForMember(x => x.Resistances, expression => expression.MapFrom(x => MapIRVFlags(x.Resistances)))
                .ForMember(x => x.Vulnerabilities, expression => expression.MapFrom(x => MapIRVFlags(x.Vulnerabilities)))
                .ForMember(x => x.Attributes, expression => expression.MapFrom(x => MapFromDictionary(x.Attributes, MapCharacterAttributes)))
                .ForMember(x => x.CurrentResources, expression => expression.MapFrom(x => MapFromDictionary(x.CurrentResources, MapResourceKind)))
                .ForMember(x => x.MaxResources, expression => expression.MapFrom(x => MapFromDictionary(x.MaxResources, MapResourceKind)))
                .ForMember(x => x.Conditions, expression => expression.MapFrom(x => MapFromDictionary(x.Conditions, MapConditions)))
                .ForMember(x => x.Size, expression => expression.MapFrom(x => MapSizes(x.Size)))
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapFromDictionary(x.Aliases)))
                .ForMember(x => x.Cooldowns, expression => expression.MapFrom(x => MapFromDictionary(x.Cooldowns, y => y)));

            CreateMap<Domain.ItemData, DataContracts.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Domain.ItemContainerData, DataContracts.ItemContainerData>()
                .Include<Domain.ItemCorpseData, DataContracts.ItemCorpseData>()
                .Include<Domain.ItemWeaponData, DataContracts.ItemWeaponData>()
                .Include<Domain.ItemDrinkContainerData, DataContracts.ItemDrinkContainerData>()
                .Include<Domain.ItemFoodData, DataContracts.ItemFoodData>()
                .Include<Domain.ItemPortalData, DataContracts.ItemPortalData>()
                .Include<Domain.ItemStaffData, DataContracts.ItemStaffData>()
                .Include<Domain.ItemWandData, DataContracts.ItemWandData>()
                .Include<Domain.ItemLightData, DataContracts.ItemLightData>();
            CreateMap<Domain.ItemContainerData, DataContracts.ItemContainerData>()
                .ForMember(x => x.ContainerFlags, expression => expression.MapFrom(x => MapContainerFlags(x.ContainerFlags)));
            CreateMap<Domain.ItemCorpseData, DataContracts.ItemCorpseData>();
            CreateMap<Domain.ItemWeaponData, DataContracts.ItemWeaponData>()
                .ForMember(x => x.WeaponFlags, expression => expression.MapFrom(x => MapWeaponFlags(x.WeaponFlags)));
            CreateMap<Domain.ItemDrinkContainerData, DataContracts.ItemDrinkContainerData>();
            CreateMap<Domain.ItemFoodData, DataContracts.ItemFoodData>();
            CreateMap<Domain.ItemPortalData, DataContracts.ItemPortalData>()
                .ForMember(x => x.PortalFlags, expression => expression.MapFrom(x => MapPortalFlags(x.PortalFlags)));
            CreateMap<Domain.ItemStaffData, DataContracts.ItemStaffData>();
            CreateMap<Domain.ItemWandData, DataContracts.ItemWandData>();
            CreateMap<Domain.ItemLightData, DataContracts.ItemLightData>();

            CreateMap<Domain.EquippedItemData, DataContracts.EquippedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<Domain.CurrentQuestData, DataContracts.CurrentQuestData>();
            CreateMap<Domain.CurrentQuestObjectiveData, DataContracts.CurrentQuestObjectiveData>();

            CreateMap<Domain.AuraData, DataContracts.AuraData>()
                .ForMember(x => x.AuraFlags, expression => expression.MapFrom(x => MapAuraFlags(x.AuraFlags)));

            CreateMap<Domain.AffectDataBase, DataContracts.AffectDataBase>()
                .Include<Domain.CharacterAttributeAffectData, DataContracts.CharacterAttributeAffectData>()
                .Include<Domain.CharacterFlagsAffectData, DataContracts.CharacterFlagsAffectData>()
                .Include<Domain.CharacterIRVAffectData, DataContracts.CharacterIRVAffectData>()
                .Include<Domain.CharacterSexAffectData, DataContracts.CharacterSexAffectData>()
                .Include<Domain.ItemFlagsAffectData, DataContracts.ItemFlagsAffectData>()
                .Include<Domain.ItemWeaponFlagsAffectData, DataContracts.ItemWeaponFlagsAffectData>()
                .Include<Domain.CharacterSizeAffectData, DataContracts.CharacterSizeAffectData>();
            CreateMap<Domain.CharacterAttributeAffectData, DataContracts.CharacterAttributeAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapCharacterAttributeAffectLocations(x.Location)));
            CreateMap<Domain.CharacterFlagsAffectData, DataContracts.CharacterFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapCharacterFlags(x.Modifier)));
            CreateMap<Domain.CharacterIRVAffectData, DataContracts.CharacterIRVAffectData>()
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapIRVAffectLocations(x.Location)))
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapIRVFlags(x.Modifier)));
            CreateMap<Domain.CharacterSexAffectData, DataContracts.CharacterSexAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSex(x.Value)));
            CreateMap<Domain.ItemFlagsAffectData, DataContracts.ItemFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapItemFlags(x.Modifier)));
            CreateMap<Domain.ItemWeaponFlagsAffectData, DataContracts.ItemWeaponFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapWeaponFlags(x.Modifier)));
            CreateMap<Domain.CharacterSizeAffectData, DataContracts.CharacterSizeAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSizes(x.Value)));

            CreateMap<Domain.KnownAbilityData, DataContracts.KnownAbilityData>()
                .ForMember(x => x.ResourceKind, expression => expression.MapFrom(x => MapNullableResourceKind(x.ResourceKind)))
                .ForMember(x => x.CostAmountOperator, expression => expression.MapFrom(x => MapCostAmountOperator(x.CostAmountOperator)));
        }

        private void InternalToExternal()
        {
            CreateMap<DataContracts.PlayerData, Domain.PlayerData>()
                .Include<DataContracts.AdminData, Domain.AdminData>()
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapToDictionary(x.Aliases)));
            CreateMap<DataContracts.AdminData, Domain.AdminData>()
                .ForMember(x => x.AdminLevel, expression => expression.MapFrom(x => MapAdminLevel(x.AdminLevel)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<DataContracts.CharacterData, Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)))
                .ForMember(x => x.CharacterFlags, expression => expression.MapFrom(x => MapCharacterFlags(x.CharacterFlags)))
                .ForMember(x => x.Immunities, expression => expression.MapFrom(x => MapIRVFlags(x.Immunities)))
                .ForMember(x => x.Resistances, expression => expression.MapFrom(x => MapIRVFlags(x.Resistances)))
                .ForMember(x => x.Vulnerabilities, expression => expression.MapFrom(x => MapIRVFlags(x.Vulnerabilities)))
                .ForMember(x => x.Attributes, expression => expression.MapFrom(x => MapToDictionary(x.Attributes, MapCharacterAttributes)))
                .ForMember(x => x.CurrentResources, expression => expression.MapFrom(x => MapToDictionary(x.CurrentResources, MapResourceKind)))
                .ForMember(x => x.MaxResources, expression => expression.MapFrom(x => MapToDictionary(x.MaxResources, MapResourceKind)))
                .ForMember(x => x.Conditions, expression => expression.MapFrom(x => MapToDictionary(x.Conditions, MapConditions)))
                .ForMember(x => x.Size, expression => expression.MapFrom(x => MapSizes(x.Size)))
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapToDictionary(x.Aliases)))
                .ForMember(x => x.Cooldowns, expression => expression.MapFrom(x => MapToDictionary(x.Cooldowns, y => y)));

            CreateMap<DataContracts.ItemData, Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<DataContracts.ItemContainerData, Domain.ItemContainerData>()
                .Include<DataContracts.ItemCorpseData, Domain.ItemCorpseData>()
                .Include<DataContracts.ItemWeaponData, Domain.ItemWeaponData>()
                .Include<DataContracts.ItemDrinkContainerData, Domain.ItemDrinkContainerData>()
                .Include<DataContracts.ItemFoodData, Domain.ItemFoodData>()
                .Include<DataContracts.ItemPortalData, Domain.ItemPortalData>()
                .Include<DataContracts.ItemStaffData, Domain.ItemStaffData>()
                .Include<DataContracts.ItemWandData, Domain.ItemWandData>()
                .Include<DataContracts.ItemLightData, Domain.ItemLightData>();
            CreateMap<DataContracts.ItemContainerData, Domain.ItemContainerData>()
                .ForMember(x => x.ContainerFlags, expression => expression.MapFrom(x => MapContainerFlags(x.ContainerFlags)));
            CreateMap<DataContracts.ItemCorpseData, Domain.ItemCorpseData>();
            CreateMap<DataContracts.ItemWeaponData, Domain.ItemWeaponData>()
                .ForMember(x => x.WeaponFlags, expression => expression.MapFrom(x => MapWeaponFlags(x.WeaponFlags)));
            CreateMap<DataContracts.ItemDrinkContainerData, Domain.ItemDrinkContainerData>();
            CreateMap<DataContracts.ItemFoodData, Domain.ItemFoodData>();
            CreateMap<DataContracts.ItemPortalData, Domain.ItemPortalData>()
                .ForMember(x => x.PortalFlags, expression => expression.MapFrom(x => MapPortalFlags(x.PortalFlags)));
            CreateMap<DataContracts.ItemStaffData, Domain.ItemStaffData>();
            CreateMap<DataContracts.ItemWandData, Domain.ItemWandData>();
            CreateMap<DataContracts.ItemLightData, Domain.ItemLightData>();

            CreateMap<DataContracts.EquippedItemData, Domain.EquippedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<DataContracts.CurrentQuestData, Domain.CurrentQuestData>();
            CreateMap<DataContracts.CurrentQuestObjectiveData, Domain.CurrentQuestObjectiveData>();

            CreateMap<DataContracts.AuraData, Domain.AuraData>()
                .ForMember(x => x.AuraFlags, expression => expression.MapFrom(x => MapAuraFlags(x.AuraFlags)));

            CreateMap<DataContracts.AffectDataBase, Domain.AffectDataBase>()
                .Include<DataContracts.CharacterAttributeAffectData, Domain.CharacterAttributeAffectData>()
                .Include<DataContracts.CharacterFlagsAffectData, Domain.CharacterFlagsAffectData>()
                .Include<DataContracts.CharacterIRVAffectData, Domain.CharacterIRVAffectData>()
                .Include<DataContracts.CharacterSexAffectData, Domain.CharacterSexAffectData>()
                .Include<DataContracts.ItemFlagsAffectData, Domain.ItemFlagsAffectData>()
                .Include<DataContracts.ItemWeaponFlagsAffectData, Domain.ItemWeaponFlagsAffectData>()
                .Include<DataContracts.CharacterSizeAffectData, Domain.CharacterSizeAffectData>();
            CreateMap<DataContracts.CharacterAttributeAffectData, Domain.CharacterAttributeAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapCharacterAttributeAffectLocations(x.Location)));
            CreateMap<DataContracts.CharacterFlagsAffectData, Domain.CharacterFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapCharacterFlags(x.Modifier)));
            CreateMap<DataContracts.CharacterIRVAffectData, Domain.CharacterIRVAffectData>()
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapIRVAffectLocations(x.Location)))
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapIRVFlags(x.Modifier)));
            CreateMap<DataContracts.CharacterSexAffectData, Domain.CharacterSexAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSex(x.Value)));
            CreateMap<DataContracts.ItemFlagsAffectData, Domain.ItemFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapItemFlags(x.Modifier)));
            CreateMap<DataContracts.ItemWeaponFlagsAffectData, Domain.ItemWeaponFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapWeaponFlags(x.Modifier)));
            CreateMap<DataContracts.CharacterSizeAffectData, Domain.CharacterSizeAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSizes(x.Value)));

            CreateMap<DataContracts.KnownAbilityData, Domain.KnownAbilityData>()
                .ForMember(x => x.ResourceKind, expression => expression.MapFrom(x => MapNullableResourceKind(x.ResourceKind)))
                .ForMember(x => x.CostAmountOperator, expression => expression.MapFrom(x => MapCostAmountOperator(x.CostAmountOperator)));
        }

        private DataContracts.PairData<string, string>[] MapFromDictionary(Dictionary<string, string> dictionary) => dictionary?.Select(x => new DataContracts.PairData<string, string>(x.Key, x.Value)).ToArray();

        private Dictionary<string, string> MapToDictionary(DataContracts.PairData<string, string>[] array) => array?.ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, x => x.First());

        private DataContracts.PairData<int, int>[] MapFromDictionary<T>(Dictionary<T, int> dictionary, Func<T,int> mapKeyFunc) => dictionary?.Select(x => new DataContracts.PairData<int, int>(mapKeyFunc(x.Key), x.Value)).ToArray();

        private Dictionary<T, int> MapToDictionary<T>(DataContracts.PairData<int, int>[] array, Func<int,T> mapKeyFunc) => array?.ToLookup(x => mapKeyFunc(x.Key), x => x.Value).ToDictionary(x => x.Key, x => x.First());

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

        private Mud.Domain.EquipmentSlots MapEquimentSlot(int slot)
        {
            switch (slot)
            {
                case 0:
                    return Mud.Domain.EquipmentSlots.None;
                case 1:
                    return Mud.Domain.EquipmentSlots.Light;
                case 2:
                    return Mud.Domain.EquipmentSlots.Head;
                case 3:
                    return Mud.Domain.EquipmentSlots.Amulet;
                case 4:
                    return Mud.Domain.EquipmentSlots.Chest;
                case 5:
                    return Mud.Domain.EquipmentSlots.Cloak;
                case 6:
                    return Mud.Domain.EquipmentSlots.Waist;
                case 7:
                    return Mud.Domain.EquipmentSlots.Wrists;
                case 8:
                    return Mud.Domain.EquipmentSlots.Arms;
                case 9:
                    return Mud.Domain.EquipmentSlots.Hands;
                case 10:
                    return Mud.Domain.EquipmentSlots.Ring;
                case 11:
                    return Mud.Domain.EquipmentSlots.Legs;
                case 12:
                    return Mud.Domain.EquipmentSlots.Feet;
                case 13:
                    return Mud.Domain.EquipmentSlots.MainHand;
                case 14:
                    return Mud.Domain.EquipmentSlots.OffHand;
                case 15:
                    return Mud.Domain.EquipmentSlots.Float;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid EquipmentSlots {slot} while reading pfile");
                    return 0;
            }
        }

        private int MapEquimentSlot(Mud.Domain.EquipmentSlots slot)
        {
            switch (slot)
            {
                case Mud.Domain.EquipmentSlots.None:
                    return 0;
                case Mud.Domain.EquipmentSlots.Light:
                    return 1;
                case Mud.Domain.EquipmentSlots.Head:
                    return 2;
                case Mud.Domain.EquipmentSlots.Amulet:
                    return 3;
                case Mud.Domain.EquipmentSlots.Chest:
                    return 4;
                case Mud.Domain.EquipmentSlots.Cloak:
                    return 5;
                case Mud.Domain.EquipmentSlots.Waist:
                    return 6;
                case Mud.Domain.EquipmentSlots.Wrists:
                    return 7;
                case Mud.Domain.EquipmentSlots.Arms:
                    return 8;
                case Mud.Domain.EquipmentSlots.Hands:
                    return 9;
                case Mud.Domain.EquipmentSlots.Ring:
                    return 10;
                case Mud.Domain.EquipmentSlots.Legs:
                    return 11;
                case Mud.Domain.EquipmentSlots.Feet:
                    return 12;
                case Mud.Domain.EquipmentSlots.MainHand:
                    return 13;
                case Mud.Domain.EquipmentSlots.OffHand:
                    return 14;
                case Mud.Domain.EquipmentSlots.Float:
                    return 15;
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

        private Domain.AuraFlags MapAuraFlags(int flags)
        {
            return (Domain.AuraFlags) flags;
        }

        private int MapAuraFlags(Domain.AuraFlags flags)
        {
            return (int) flags;
        }

        private Domain.AffectOperators MapAffectOperators(int op)
        {
            switch (op)
            {
                case 0:
                    return Domain.AffectOperators.Add;
                case 1:
                    return Domain.AffectOperators.Or;
                case 2:
                    return Domain.AffectOperators.Assign;
                case 3: 
                    return Domain.AffectOperators.Nor;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AffectOperators {op} while reading pfile");
                    return 0;
            }
        }

        private int MapAffectOperators(Domain.AffectOperators op)
        {
            switch (op)
            {
                case Domain.AffectOperators.Add:
                    return 0;
                case Domain.AffectOperators.Or:
                    return 1;
                case Domain.AffectOperators.Assign:
                    return 2;
                case Domain.AffectOperators.Nor:
                    return 3;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AffectOperators {op} while writing pfile");
                    return 0;
            }
        }

        private Domain.CharacterAttributeAffectLocations MapCharacterAttributeAffectLocations(int location)
        {
            switch (location)
            {
                case 0: return Domain.CharacterAttributeAffectLocations.None;
                case 1: return Domain.CharacterAttributeAffectLocations.Strength;
                case 2: return Domain.CharacterAttributeAffectLocations.Intelligence;
                case 3: return Domain.CharacterAttributeAffectLocations.Wisdom;
                case 4: return Domain.CharacterAttributeAffectLocations.Dexterity;
                case 5: return Domain.CharacterAttributeAffectLocations.Constitution;
                case 6: return Domain.CharacterAttributeAffectLocations.Characteristics;
                case 7: return Domain.CharacterAttributeAffectLocations.MaxHitPoints;
                case 8: return Domain.CharacterAttributeAffectLocations.SavingThrow;
                case 9: return Domain.CharacterAttributeAffectLocations.HitRoll;
                case 10: return Domain.CharacterAttributeAffectLocations.DamRoll;
                case 11: return Domain.CharacterAttributeAffectLocations.MaxMovePoints;
                case 12: return Domain.CharacterAttributeAffectLocations.ArmorBash;
                case 13: return Domain.CharacterAttributeAffectLocations.ArmorPierce;
                case 14: return Domain.CharacterAttributeAffectLocations.ArmorSlash;
                case 15: return Domain.CharacterAttributeAffectLocations.ArmorMagic;
                case 16: return Domain.CharacterAttributeAffectLocations.AllArmor;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributeAffectLocations {location} while reading pfile");
                    return Domain.CharacterAttributeAffectLocations.None;
            }
        }

        private int MapCharacterAttributeAffectLocations(Domain.CharacterAttributeAffectLocations location)
        {
            switch (location)
            {
                case Domain.CharacterAttributeAffectLocations.None: return 0;
                case Domain.CharacterAttributeAffectLocations.Strength: return 1;
                case Domain.CharacterAttributeAffectLocations.Intelligence: return 2;
                case Domain.CharacterAttributeAffectLocations.Wisdom: return 3;
                case Domain.CharacterAttributeAffectLocations.Dexterity: return 4;
                case Domain.CharacterAttributeAffectLocations.Constitution: return 5;
                case Domain.CharacterAttributeAffectLocations.Characteristics: return 6; // Strength + Intelligence + Wisdom + Dexterity + Constitution
                case Domain.CharacterAttributeAffectLocations.MaxHitPoints: return 7;
                case Domain.CharacterAttributeAffectLocations.SavingThrow: return 8;
                case Domain.CharacterAttributeAffectLocations.HitRoll: return 9;
                case Domain.CharacterAttributeAffectLocations.DamRoll: return 10;
                case Domain.CharacterAttributeAffectLocations.MaxMovePoints: return 11;
                case Domain.CharacterAttributeAffectLocations.ArmorBash: return 12;
                case Domain.CharacterAttributeAffectLocations.ArmorPierce: return 13;
                case Domain.CharacterAttributeAffectLocations.ArmorSlash: return 14;
                case Domain.CharacterAttributeAffectLocations.ArmorMagic: return 15;
                case Domain.CharacterAttributeAffectLocations.AllArmor: return 16;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributeAffectLocations {location} while writing pfile");
                    return 0;
            }
        }

        private Domain.CharacterFlags MapCharacterFlags(int flags)
        {
            return (Domain.CharacterFlags)flags;
        }

        private int MapCharacterFlags(Domain.CharacterFlags flags)
        {
            return (int)flags;
        }

        private Domain.IRVAffectLocations MapIRVAffectLocations(int location)
        {
            switch (location)
            {
                case 0: return Domain.IRVAffectLocations.None;
                case 1: return Domain.IRVAffectLocations.Immunities;
                case 2: return Domain.IRVAffectLocations.Resistances;
                case 3: return Domain.IRVAffectLocations.Vulnerabilities;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid IRVAffectLocations {location} while reading pfile");
                    return 0;
            }
        }

        private int MapIRVAffectLocations(Domain.IRVAffectLocations location)
        {
            switch (location)
            {
                case Domain.IRVAffectLocations.None: return 0;
                case Domain.IRVAffectLocations.Immunities: return 1;
                case Domain.IRVAffectLocations.Resistances: return 2;
                case Domain.IRVAffectLocations.Vulnerabilities: return 2;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid IRVAffectLocations {location} while writing pfile");
                    return 0;
            }
        }

        private Domain.IRVFlags MapIRVFlags(int flags)
        {
            return (Domain.IRVFlags)flags;
        }

        private int MapIRVFlags(Domain.IRVFlags flags)
        {
            return (int)flags;
        }

        private Domain.WeaponFlags MapWeaponFlags(int flags)
        {
            return (Domain.WeaponFlags)flags;
        }

        private int MapWeaponFlags(Domain.WeaponFlags flags)
        {
            return (int)flags;
        }

        private Domain.CharacterAttributes MapCharacterAttributes(int attr)
        {
            switch (attr)
            {
                case 0: return Domain.CharacterAttributes.Strength;
                case 1: return Domain.CharacterAttributes.Intelligence;
                case 2: return Domain.CharacterAttributes.Wisdom;
                case 3: return Domain.CharacterAttributes.Dexterity;
                case 4: return Domain.CharacterAttributes.Constitution;
                case 5: return Domain.CharacterAttributes.MaxHitPoints;
                case 6: return Domain.CharacterAttributes.SavingThrow;
                case 7: return Domain.CharacterAttributes.HitRoll;
                case 8: return Domain.CharacterAttributes.DamRoll;
                case 9: return Domain.CharacterAttributes.MaxMovePoints;
                case 10: return Domain.CharacterAttributes.ArmorBash;
                case 11: return Domain.CharacterAttributes.ArmorPierce;
                case 12: return Domain.CharacterAttributes.ArmorSlash;
                case 13: return Domain.CharacterAttributes.ArmorExotic;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributes {attr} while reading pfile");
                    return Domain.CharacterAttributes.Strength;
            }
        }

        private int MapCharacterAttributes(Domain.CharacterAttributes attr)
        {
            switch (attr)
            {
                case Domain.CharacterAttributes.Strength: return 0;
                case Domain.CharacterAttributes.Intelligence: return 1;
                case Domain.CharacterAttributes.Wisdom: return 2;
                case Domain.CharacterAttributes.Dexterity: return 3;
                case Domain.CharacterAttributes.Constitution: return 4;
                case Domain.CharacterAttributes.MaxHitPoints: return 5;
                case Domain.CharacterAttributes.SavingThrow: return 6;
                case Domain.CharacterAttributes.HitRoll: return 7;
                case Domain.CharacterAttributes.DamRoll: return 8;
                case Domain.CharacterAttributes.MaxMovePoints: return 9;
                case Domain.CharacterAttributes.ArmorBash: return 10;
                case Domain.CharacterAttributes.ArmorPierce: return 11;
                case Domain.CharacterAttributes.ArmorSlash: return 12;
                case Domain.CharacterAttributes.ArmorExotic: return 13;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributes {attr} while writing pfile");
                    return 0;
            }
        }

        private Domain.ResourceKinds MapResourceKind(int resource)
        {
            switch (resource)
            {
                case 0: return Domain.ResourceKinds.Mana;
                case 1: return Domain.ResourceKinds.Psy;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while reading pfile");
                    return Domain.ResourceKinds.Mana;
            }
        }

        private int MapResourceKind(Domain.ResourceKinds resource)
        {
            switch (resource)
            {
                case Domain.ResourceKinds.Mana: return 0;
                case Domain.ResourceKinds.Psy: return 1;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while writing pfile");
                    return 0;
            }
        }

        private Domain.ResourceKinds? MapNullableResourceKind(int resource)
        {
            switch (resource)
            {
                case -1: return null;
                case 0: return Domain.ResourceKinds.Mana;
                case 1: return Domain.ResourceKinds.Psy;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while reading pfile");
                    return Domain.ResourceKinds.Mana;
            }
        }

        private int MapNullableResourceKind(Domain.ResourceKinds? resource)
        {
            if (!resource.HasValue)
                return -1;
            switch (resource.Value)
            {
                case Domain.ResourceKinds.Mana: return 0;
                case Domain.ResourceKinds.Psy: return 1;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while writing pfile");
                    return 0;
            }
        }

        private Domain.CostAmountOperators MapCostAmountOperator(int op)
        {
            switch (op)
            {
                case 0: return Domain.CostAmountOperators.None;
                case 1: return Domain.CostAmountOperators.Fixed;
                case 2: return Domain.CostAmountOperators.Percentage;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CostAmountOperators {op} while reading pfile");
                    return 0;
            }
        }

        private int MapCostAmountOperator(Domain.CostAmountOperators op)
        {
            switch (op)
            {
                case Domain.CostAmountOperators.None: return 0;
                case Domain.CostAmountOperators.Fixed: return 1;
                case Domain.CostAmountOperators.Percentage: return 2;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CostAmountOperators {op} while writing pfile");
                    return 0;
            }
        }

        private Domain.Conditions MapConditions(int cond)
        {
            switch (cond)
            {
                case 0: return Mud.Domain.Conditions.Drunk;
                case 1: return Mud.Domain.Conditions.Full;
                case 2: return Mud.Domain.Conditions.Thirst;
                case 3: return Mud.Domain.Conditions.Hunger;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Conditions {cond} while reading pfile");
                    return Mud.Domain.Conditions.Drunk;
            }
        }

        private int MapConditions(Domain.Conditions cond)
        {
            switch (cond)
            {
                case Mud.Domain.Conditions.Drunk: return 0;
                case Mud.Domain.Conditions.Full: return 1;
                case Mud.Domain.Conditions.Thirst: return 2;
                case Mud.Domain.Conditions.Hunger: return 3;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Conditions {cond} while writing pfile");
                    return 0;
            }
        }

        private Domain.PortalFlags MapPortalFlags(int flags)
        {
            return (Domain.PortalFlags)flags;
        }

        private int MapPortalFlags(Domain.PortalFlags flags)
        {
            return (int)flags;
        }

        private Domain.ContainerFlags MapContainerFlags(int flags)
        {
            return (Domain.ContainerFlags)flags;
        }

        private int MapContainerFlags(Domain.ContainerFlags flags)
        {
            return (int)flags;
        }

        private Domain.Sizes MapSizes(int size)
        {
            switch (size)
            {
                case 0: return Domain.Sizes.Tiny;
                case 1: return Domain.Sizes.Small;
                case 2: return Domain.Sizes.Medium;
                case 3: return Domain.Sizes.Large;
                case 4: return Domain.Sizes.Huge;
                case 5: return Domain.Sizes.Giant;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sizes {size} while reading pfile");
                    return Domain.Sizes.Tiny;
            }
        }

        private int MapSizes(Domain.Sizes size)
        {
            switch (size)
            {
                case Domain.Sizes.Tiny: return 0;
                case Domain.Sizes.Small: return 1;
                case Domain.Sizes.Medium: return 2;
                case Domain.Sizes.Large: return 3;
                case Domain.Sizes.Huge: return 4;
                case Domain.Sizes.Giant: return 5;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sizes {size} while writing pfile");
                    return 0;
            }
        }
    }
}
