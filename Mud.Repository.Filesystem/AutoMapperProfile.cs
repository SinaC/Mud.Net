using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mud.Logger;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

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
            CreateMap<Mud.Domain.PlayerData, Domain.PlayerData>()
                .Include<Mud.Domain.AdminData, Domain.AdminData>()
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapFromDictionary(x.Aliases)));
            CreateMap<Mud.Domain.AdminData, Domain.AdminData>()
                .ForMember(x => x.AdminLevel, expression => expression.MapFrom(x => MapAdminLevel(x.AdminLevel)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Mud.Domain.CharacterData, Domain.CharacterData>()
                .Include<Mud.Domain.PlayableCharacterData, Domain.PlayableCharacterData>()
                .Include<Mud.Domain.PetData, Domain.PetData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)))
                .ForMember(x => x.CharacterFlags, expression => expression.MapFrom(x => MapCharacterFlags(x.CharacterFlags)))
                .ForMember(x => x.Immunities, expression => expression.MapFrom(x => MapIRVFlags(x.Immunities)))
                .ForMember(x => x.Resistances, expression => expression.MapFrom(x => MapIRVFlags(x.Resistances)))
                .ForMember(x => x.Vulnerabilities, expression => expression.MapFrom(x => MapIRVFlags(x.Vulnerabilities)))
                .ForMember(x => x.Attributes, expression => expression.MapFrom(x => MapFromDictionary(x.Attributes, MapCharacterAttributes)))
                .ForMember(x => x.CurrentResources, expression => expression.MapFrom(x => MapFromDictionary(x.CurrentResources, MapResourceKind)))
                .ForMember(x => x.MaxResources, expression => expression.MapFrom(x => MapFromDictionary(x.MaxResources, MapResourceKind)))
                .ForMember(x => x.Size, expression => expression.MapFrom(x => MapSizes(x.Size)));
            CreateMap<Mud.Domain.PlayableCharacterData, Domain.PlayableCharacterData>()
                .ForMember(x => x.Conditions, expression => expression.MapFrom(x => MapFromDictionary(x.Conditions, MapConditions)))
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapFromDictionary(x.Aliases)))
                .ForMember(x => x.Cooldowns, expression => expression.MapFrom(x => MapFromDictionary(x.Cooldowns)))
                .ForMember(x => x.AutoFlags, expression => expression.MapFrom(x => MapAutoFlags(x.AutoFlags)));
            CreateMap<Mud.Domain.PetData, Domain.PetData>();

            CreateMap<Mud.Domain.ItemData, Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Mud.Domain.ItemContainerData, Domain.ItemContainerData>()
                .Include<Mud.Domain.ItemCorpseData, Domain.ItemCorpseData>()
                .Include<Mud.Domain.ItemWeaponData, Domain.ItemWeaponData>()
                .Include<Mud.Domain.ItemDrinkContainerData, Domain.ItemDrinkContainerData>()
                .Include<Mud.Domain.ItemFoodData, Domain.ItemFoodData>()
                .Include<Mud.Domain.ItemPortalData, Domain.ItemPortalData>()
                .Include<Mud.Domain.ItemStaffData, Domain.ItemStaffData>()
                .Include<Mud.Domain.ItemWandData, Domain.ItemWandData>()
                .Include<Mud.Domain.ItemLightData, Domain.ItemLightData>();
            CreateMap<Mud.Domain.ItemContainerData, Domain.ItemContainerData>()
                .ForMember(x => x.ContainerFlags, expression => expression.MapFrom(x => MapContainerFlags(x.ContainerFlags)));
            CreateMap<Mud.Domain.ItemCorpseData, Domain.ItemCorpseData>();
            CreateMap<Mud.Domain.ItemWeaponData, Domain.ItemWeaponData>()
                .ForMember(x => x.WeaponFlags, expression => expression.MapFrom(x => MapWeaponFlags(x.WeaponFlags)));
            CreateMap<Mud.Domain.ItemDrinkContainerData, Domain.ItemDrinkContainerData>();
            CreateMap<Mud.Domain.ItemFoodData, Domain.ItemFoodData>();
            CreateMap<Mud.Domain.ItemPortalData, Domain.ItemPortalData>()
                .ForMember(x => x.PortalFlags, expression => expression.MapFrom(x => MapPortalFlags(x.PortalFlags)));
            CreateMap<Mud.Domain.ItemStaffData, Domain.ItemStaffData>();
            CreateMap<Mud.Domain.ItemWandData, Domain.ItemWandData>();
            CreateMap<Mud.Domain.ItemLightData, Domain.ItemLightData>();

            CreateMap<Mud.Domain.EquippedItemData, Domain.EquippedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<Mud.Domain.CurrentQuestData, Domain.CurrentQuestData>();
            CreateMap<Mud.Domain.CurrentQuestObjectiveData, Domain.CurrentQuestObjectiveData>();

            CreateMap<Mud.Domain.AuraData, Domain.AuraData>()
                .ForMember(x => x.AuraFlags, expression => expression.MapFrom(x => MapAuraFlags(x.AuraFlags)));

            CreateMap<Mud.Domain.AffectDataBase, Domain.AffectDataBase>()
                .Include<Mud.Domain.CharacterAttributeAffectData, Domain.CharacterAttributeAffectData>()
                .Include<Mud.Domain.CharacterFlagsAffectData, Domain.CharacterFlagsAffectData>()
                .Include<Mud.Domain.CharacterIRVAffectData, Domain.CharacterIRVAffectData>()
                .Include<Mud.Domain.CharacterSexAffectData, Domain.CharacterSexAffectData>()
                .Include<Mud.Domain.ItemFlagsAffectData, Domain.ItemFlagsAffectData>()
                .Include<Mud.Domain.ItemWeaponFlagsAffectData, Domain.ItemWeaponFlagsAffectData>()
                .Include<Mud.Domain.CharacterSizeAffectData, Domain.CharacterSizeAffectData>()
                .Include<Mud.Domain.PoisonDamageAffectData, Domain.PoisonDamageAffectData>()
                .Include<Mud.Domain.PlagueSpreadAndDamageAffectData, Domain.PlagueSpreadAndDamageAffectData>();
            CreateMap<Mud.Domain.CharacterAttributeAffectData, Domain.CharacterAttributeAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapCharacterAttributeAffectLocations(x.Location)));
            CreateMap<Mud.Domain.CharacterFlagsAffectData, Domain.CharacterFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapCharacterFlags(x.Modifier)));
            CreateMap<Mud.Domain.CharacterIRVAffectData, Domain.CharacterIRVAffectData>()
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapIRVAffectLocations(x.Location)))
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapIRVFlags(x.Modifier)));
            CreateMap<Mud.Domain.CharacterSexAffectData, Domain.CharacterSexAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSex(x.Value)));
            CreateMap<Mud.Domain.ItemFlagsAffectData, Domain.ItemFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapItemFlags(x.Modifier)));
            CreateMap<Mud.Domain.ItemWeaponFlagsAffectData, Domain.ItemWeaponFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapWeaponFlags(x.Modifier)));
            CreateMap<Mud.Domain.CharacterSizeAffectData, Domain.CharacterSizeAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSizes(x.Value)));
            CreateMap<Mud.Domain.PoisonDamageAffectData, Domain.PoisonDamageAffectData>();
            CreateMap<Mud.Domain.PlagueSpreadAndDamageAffectData, Domain.PlagueSpreadAndDamageAffectData>();

            CreateMap<Mud.Domain.LearnedAbilityData, Domain.LearnedAbilityData>()
                .ForMember(x => x.ResourceKind, expression => expression.MapFrom(x => MapNullableResourceKind(x.ResourceKind)))
                .ForMember(x => x.CostAmountOperator, expression => expression.MapFrom(x => MapCostAmountOperator(x.CostAmountOperator)));
        }

        private void InternalToExternal()
        {
            CreateMap<Domain.PlayerData, Mud.Domain.PlayerData>()
                .Include<Domain.AdminData, Mud.Domain.AdminData>()
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapToDictionary(x.Aliases)));
            CreateMap<Domain.AdminData, Mud.Domain.AdminData>()
                .ForMember(x => x.AdminLevel, expression => expression.MapFrom(x => MapAdminLevel(x.AdminLevel)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Domain.CharacterData, Mud.Domain.CharacterData>()
                .Include<Domain.PlayableCharacterData, Mud.Domain.PlayableCharacterData>()
                .Include<Domain.PetData, Mud.Domain.PetData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)))
                .ForMember(x => x.CharacterFlags, expression => expression.MapFrom(x => MapCharacterFlags(x.CharacterFlags)))
                .ForMember(x => x.Immunities, expression => expression.MapFrom(x => MapIRVFlags(x.Immunities)))
                .ForMember(x => x.Resistances, expression => expression.MapFrom(x => MapIRVFlags(x.Resistances)))
                .ForMember(x => x.Vulnerabilities, expression => expression.MapFrom(x => MapIRVFlags(x.Vulnerabilities)))
                .ForMember(x => x.Attributes, expression => expression.MapFrom(x => MapToDictionary(x.Attributes, MapCharacterAttributes)))
                .ForMember(x => x.CurrentResources, expression => expression.MapFrom(x => MapToDictionary(x.CurrentResources, MapResourceKind)))
                .ForMember(x => x.MaxResources, expression => expression.MapFrom(x => MapToDictionary(x.MaxResources, MapResourceKind)))
                .ForMember(x => x.Size, expression => expression.MapFrom(x => MapSizes(x.Size)));
            CreateMap<Domain.PlayableCharacterData, Mud.Domain.PlayableCharacterData>()
                .ForMember(x => x.Conditions, expression => expression.MapFrom(x => MapToDictionary(x.Conditions, MapConditions)))
                .ForMember(x => x.Aliases, expression => expression.MapFrom(x => MapToDictionary(x.Aliases)))
                .ForMember(x => x.Cooldowns, expression => expression.MapFrom(x => MapToDictionary(x.Cooldowns)))
                .ForMember(x => x.AutoFlags, expression => expression.MapFrom(x => MapAutoFlags(x.AutoFlags)));
            CreateMap<Domain.PetData, Mud.Domain.PetData>();

            CreateMap<Domain.ItemData, Mud.Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Domain.ItemContainerData, Mud.Domain.ItemContainerData>()
                .Include<Domain.ItemCorpseData, Mud.Domain.ItemCorpseData>()
                .Include<Domain.ItemWeaponData, Mud.Domain.ItemWeaponData>()
                .Include<Domain.ItemDrinkContainerData, Mud.Domain.ItemDrinkContainerData>()
                .Include<Domain.ItemFoodData, Mud.Domain.ItemFoodData>()
                .Include<Domain.ItemPortalData, Mud.Domain.ItemPortalData>()
                .Include<Domain.ItemStaffData, Mud.Domain.ItemStaffData>()
                .Include<Domain.ItemWandData, Mud.Domain.ItemWandData>()
                .Include<Domain.ItemLightData, Mud.Domain.ItemLightData>();
            CreateMap<Domain.ItemContainerData, Mud.Domain.ItemContainerData>()
                .ForMember(x => x.ContainerFlags, expression => expression.MapFrom(x => MapContainerFlags(x.ContainerFlags)));
            CreateMap<Domain.ItemCorpseData, Mud.Domain.ItemCorpseData>();
            CreateMap<Domain.ItemWeaponData, Mud.Domain.ItemWeaponData>()
                .ForMember(x => x.WeaponFlags, expression => expression.MapFrom(x => MapWeaponFlags(x.WeaponFlags)));
            CreateMap<Domain.ItemDrinkContainerData, Mud.Domain.ItemDrinkContainerData>();
            CreateMap<Domain.ItemFoodData, Mud.Domain.ItemFoodData>();
            CreateMap<Domain.ItemPortalData, Mud.Domain.ItemPortalData>()
                .ForMember(x => x.PortalFlags, expression => expression.MapFrom(x => MapPortalFlags(x.PortalFlags)));
            CreateMap<Domain.ItemStaffData, Mud.Domain.ItemStaffData>();
            CreateMap<Domain.ItemWandData, Mud.Domain.ItemWandData>();
            CreateMap<Domain.ItemLightData, Mud.Domain.ItemLightData>();

            CreateMap<Domain.EquippedItemData, Mud.Domain.EquippedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<Domain.CurrentQuestData, Mud.Domain.CurrentQuestData>();
            CreateMap<Domain.CurrentQuestObjectiveData, Mud.Domain.CurrentQuestObjectiveData>();

            CreateMap<Domain.AuraData, Mud.Domain.AuraData>()
                .ForMember(x => x.AuraFlags, expression => expression.MapFrom(x => MapAuraFlags(x.AuraFlags)));

            CreateMap<Domain.AffectDataBase, Mud.Domain.AffectDataBase>()
                .Include<Domain.CharacterAttributeAffectData, Mud.Domain.CharacterAttributeAffectData>()
                .Include<Domain.CharacterFlagsAffectData, Mud.Domain.CharacterFlagsAffectData>()
                .Include<Domain.CharacterIRVAffectData, Mud.Domain.CharacterIRVAffectData>()
                .Include<Domain.CharacterSexAffectData, Mud.Domain.CharacterSexAffectData>()
                .Include<Domain.ItemFlagsAffectData, Mud.Domain.ItemFlagsAffectData>()
                .Include<Domain.ItemWeaponFlagsAffectData, Mud.Domain.ItemWeaponFlagsAffectData>()
                .Include<Domain.CharacterSizeAffectData, Mud.Domain.CharacterSizeAffectData>()
                .Include<Domain.PoisonDamageAffectData, Mud.Domain.PoisonDamageAffectData>()
                .Include<Domain.PlagueSpreadAndDamageAffectData, Mud.Domain.PlagueSpreadAndDamageAffectData>();
            CreateMap<Domain.CharacterAttributeAffectData, Mud.Domain.CharacterAttributeAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapCharacterAttributeAffectLocations(x.Location)));
            CreateMap<Domain.CharacterFlagsAffectData, Mud.Domain.CharacterFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapCharacterFlags(x.Modifier)));
            CreateMap<Domain.CharacterIRVAffectData, Mud.Domain.CharacterIRVAffectData>()
                .ForMember(x => x.Location, expression => expression.MapFrom(x => MapIRVAffectLocations(x.Location)))
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapIRVFlags(x.Modifier)));
            CreateMap<Domain.CharacterSexAffectData, Mud.Domain.CharacterSexAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSex(x.Value)));
            CreateMap<Domain.ItemFlagsAffectData, Mud.Domain.ItemFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapItemFlags(x.Modifier)));
            CreateMap<Domain.ItemWeaponFlagsAffectData, Mud.Domain.ItemWeaponFlagsAffectData>()
                .ForMember(x => x.Operator, expression => expression.MapFrom(x => MapAffectOperators(x.Operator)))
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapWeaponFlags(x.Modifier)));
            CreateMap<Domain.CharacterSizeAffectData, Mud.Domain.CharacterSizeAffectData>()
                .ForMember(x => x.Value, expression => expression.MapFrom(x => MapSizes(x.Value)));
            CreateMap<Domain.PoisonDamageAffectData, Mud.Domain.PoisonDamageAffectData>();
            CreateMap<Domain.PlagueSpreadAndDamageAffectData, Mud.Domain.PlagueSpreadAndDamageAffectData>();

            CreateMap<Domain.LearnedAbilityData, Mud.Domain.LearnedAbilityData>()
                .ForMember(x => x.ResourceKind, expression => expression.MapFrom(x => MapNullableResourceKind(x.ResourceKind)))
                .ForMember(x => x.CostAmountOperator, expression => expression.MapFrom(x => MapCostAmountOperator(x.CostAmountOperator)));
        }

        private Domain.PairData<string, string>[] MapFromDictionary(Dictionary<string, string> dictionary) => dictionary?.Select(x => new Domain.PairData<string, string>(x.Key, x.Value)).ToArray();

        private Dictionary<string, string> MapToDictionary(Domain.PairData<string, string>[] array) => array?.ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, x => x.First());

        private Domain.PairData<string, int>[] MapFromDictionary(Dictionary<string, int> dictionary) => dictionary?.Select(x => new Domain.PairData<string, int>(x.Key, x.Value)).ToArray();

        private Dictionary<string, int> MapToDictionary(Domain.PairData<string, int>[] array) => array?.ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, x => x.First());

        private Domain.PairData<int, int>[] MapFromDictionary<T>(Dictionary<T, int> dictionary, Func<T,int> mapKeyFunc) => dictionary?.Select(x => new Domain.PairData<int, int>(mapKeyFunc(x.Key), x.Value)).ToArray();

        private Dictionary<T, int> MapToDictionary<T>(Domain.PairData<int, int>[] array, Func<int,T> mapKeyFunc) => array?.ToLookup(x => mapKeyFunc(x.Key), x => x.Value).ToDictionary(x => x.Key, x => x.First());

        private Mud.Domain.WiznetFlags MapWiznetFlags(int flags)
        {
            return (Mud.Domain.WiznetFlags)flags;
        }

        private int MapWiznetFlags(Mud.Domain.WiznetFlags flags)
        {
            return (int)flags;
        }

        private Mud.Domain.AdminLevels MapAdminLevel(int level)
        {
            switch (level)
            {
                case 0:
                    return Mud.Domain.AdminLevels.Angel;
                case 1:
                    return Mud.Domain.AdminLevels.DemiGod;
                case 2:
                    return Mud.Domain.AdminLevels.Immortal;
                case 3:
                    return Mud.Domain.AdminLevels.God;
                case 4:
                    return Mud.Domain.AdminLevels.Deity;
                case 5:
                    return Mud.Domain.AdminLevels.Supremacy;
                case 6:
                    return Mud.Domain.AdminLevels.Creator;
                case 7:
                    return Mud.Domain.AdminLevels.Implementor;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AdminLevels {level} while reading pfile");
                    return 0;
            }
        }

        private int MapAdminLevel(Mud.Domain.AdminLevels level)
        {
            switch (level)
            {
                case Mud.Domain.AdminLevels.Angel:
                    return 0;
                case Mud.Domain.AdminLevels.DemiGod:
                    return 1;
                case Mud.Domain.AdminLevels.Immortal:
                    return 2;
                case Mud.Domain.AdminLevels.God:
                    return 3;
                case Mud.Domain.AdminLevels.Deity:
                    return 4;
                case Mud.Domain.AdminLevels.Supremacy:
                    return 5;
                case Mud.Domain.AdminLevels.Creator:
                    return 6;
                case Mud.Domain.AdminLevels.Implementor:
                    return 7;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AdminLevels {level} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.Sex MapSex(int sex)
        {
            switch (sex)
            {
                case 0:
                    return Mud.Domain.Sex.Neutral;
                case 1:
                    return Mud.Domain.Sex.Male;
                case 2:
                    return Mud.Domain.Sex.Female;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sex {sex} while reading pfile");
                    return Mud.Domain.Sex.Neutral;
            }
        }

        private int MapSex(Mud.Domain.Sex sex)
        {
            switch (sex)
            {
                case Mud.Domain.Sex.Neutral:
                    return 0;
                case Mud.Domain.Sex.Male:
                    return 1;
                case Mud.Domain.Sex.Female:
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

        private IItemFlags MapItemFlags(string flags)
        {
            return new ItemFlags(flags);
        }

        private string MapItemFlags(IItemFlags flags)
        {
            return flags?.Map();
        }

        private Mud.Domain.AuraFlags MapAuraFlags(int flags)
        {
            return (Mud.Domain.AuraFlags) flags;
        }

        private int MapAuraFlags(Mud.Domain.AuraFlags flags)
        {
            return (int) flags;
        }

        private Mud.Domain.AffectOperators MapAffectOperators(int op)
        {
            switch (op)
            {
                case 0:
                    return Mud.Domain.AffectOperators.Add;
                case 1:
                    return Mud.Domain.AffectOperators.Or;
                case 2:
                    return Mud.Domain.AffectOperators.Assign;
                case 3: 
                    return Mud.Domain.AffectOperators.Nor;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AffectOperators {op} while reading pfile");
                    return 0;
            }
        }

        private int MapAffectOperators(Mud.Domain.AffectOperators op)
        {
            switch (op)
            {
                case Mud.Domain.AffectOperators.Add:
                    return 0;
                case Mud.Domain.AffectOperators.Or:
                    return 1;
                case Mud.Domain.AffectOperators.Assign:
                    return 2;
                case Mud.Domain.AffectOperators.Nor:
                    return 3;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AffectOperators {op} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.CharacterAttributeAffectLocations MapCharacterAttributeAffectLocations(int location)
        {
            switch (location)
            {
                case 0: return Mud.Domain.CharacterAttributeAffectLocations.None;
                case 1: return Mud.Domain.CharacterAttributeAffectLocations.Strength;
                case 2: return Mud.Domain.CharacterAttributeAffectLocations.Intelligence;
                case 3: return Mud.Domain.CharacterAttributeAffectLocations.Wisdom;
                case 4: return Mud.Domain.CharacterAttributeAffectLocations.Dexterity;
                case 5: return Mud.Domain.CharacterAttributeAffectLocations.Constitution;
                case 6: return Mud.Domain.CharacterAttributeAffectLocations.Characteristics;
                case 7: return Mud.Domain.CharacterAttributeAffectLocations.MaxHitPoints;
                case 8: return Mud.Domain.CharacterAttributeAffectLocations.SavingThrow;
                case 9: return Mud.Domain.CharacterAttributeAffectLocations.HitRoll;
                case 10: return Mud.Domain.CharacterAttributeAffectLocations.DamRoll;
                case 11: return Mud.Domain.CharacterAttributeAffectLocations.MaxMovePoints;
                case 12: return Mud.Domain.CharacterAttributeAffectLocations.ArmorBash;
                case 13: return Mud.Domain.CharacterAttributeAffectLocations.ArmorPierce;
                case 14: return Mud.Domain.CharacterAttributeAffectLocations.ArmorSlash;
                case 15: return Mud.Domain.CharacterAttributeAffectLocations.ArmorMagic;
                case 16: return Mud.Domain.CharacterAttributeAffectLocations.AllArmor;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributeAffectLocations {location} while reading pfile");
                    return Mud.Domain.CharacterAttributeAffectLocations.None;
            }
        }

        private int MapCharacterAttributeAffectLocations(Mud.Domain.CharacterAttributeAffectLocations location)
        {
            switch (location)
            {
                case Mud.Domain.CharacterAttributeAffectLocations.None: return 0;
                case Mud.Domain.CharacterAttributeAffectLocations.Strength: return 1;
                case Mud.Domain.CharacterAttributeAffectLocations.Intelligence: return 2;
                case Mud.Domain.CharacterAttributeAffectLocations.Wisdom: return 3;
                case Mud.Domain.CharacterAttributeAffectLocations.Dexterity: return 4;
                case Mud.Domain.CharacterAttributeAffectLocations.Constitution: return 5;
                case Mud.Domain.CharacterAttributeAffectLocations.Characteristics: return 6; // Strength + Intelligence + Wisdom + Dexterity + Constitution
                case Mud.Domain.CharacterAttributeAffectLocations.MaxHitPoints: return 7;
                case Mud.Domain.CharacterAttributeAffectLocations.SavingThrow: return 8;
                case Mud.Domain.CharacterAttributeAffectLocations.HitRoll: return 9;
                case Mud.Domain.CharacterAttributeAffectLocations.DamRoll: return 10;
                case Mud.Domain.CharacterAttributeAffectLocations.MaxMovePoints: return 11;
                case Mud.Domain.CharacterAttributeAffectLocations.ArmorBash: return 12;
                case Mud.Domain.CharacterAttributeAffectLocations.ArmorPierce: return 13;
                case Mud.Domain.CharacterAttributeAffectLocations.ArmorSlash: return 14;
                case Mud.Domain.CharacterAttributeAffectLocations.ArmorMagic: return 15;
                case Mud.Domain.CharacterAttributeAffectLocations.AllArmor: return 16;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributeAffectLocations {location} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.IRVAffectLocations MapIRVAffectLocations(int location)
        {
            switch (location)
            {
                case 0: return Mud.Domain.IRVAffectLocations.None;
                case 1: return Mud.Domain.IRVAffectLocations.Immunities;
                case 2: return Mud.Domain.IRVAffectLocations.Resistances;
                case 3: return Mud.Domain.IRVAffectLocations.Vulnerabilities;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid IRVAffectLocations {location} while reading pfile");
                    return 0;
            }
        }

        private int MapIRVAffectLocations(Mud.Domain.IRVAffectLocations location)
        {
            switch (location)
            {
                case Mud.Domain.IRVAffectLocations.None: return 0;
                case Mud.Domain.IRVAffectLocations.Immunities: return 1;
                case Mud.Domain.IRVAffectLocations.Resistances: return 2;
                case Mud.Domain.IRVAffectLocations.Vulnerabilities: return 2;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid IRVAffectLocations {location} while writing pfile");
                    return 0;
            }
        }

        private IIRVFlags MapIRVFlags(string flags)
        {
            return new IRVFlags(flags);
        }

        private string MapIRVFlags(IIRVFlags flags)
        {
            return flags.Map();
        }

        private IWeaponFlags MapWeaponFlags(string flags)
        {
            return new WeaponFlags(flags);
        }

        private string MapWeaponFlags(IWeaponFlags flags)
        {
            return flags?.Map();
        }

        private Mud.Domain.CharacterAttributes MapCharacterAttributes(int attr)
        {
            switch (attr)
            {
                case 0: return Mud.Domain.CharacterAttributes.Strength;
                case 1: return Mud.Domain.CharacterAttributes.Intelligence;
                case 2: return Mud.Domain.CharacterAttributes.Wisdom;
                case 3: return Mud.Domain.CharacterAttributes.Dexterity;
                case 4: return Mud.Domain.CharacterAttributes.Constitution;
                case 5: return Mud.Domain.CharacterAttributes.MaxHitPoints;
                case 6: return Mud.Domain.CharacterAttributes.SavingThrow;
                case 7: return Mud.Domain.CharacterAttributes.HitRoll;
                case 8: return Mud.Domain.CharacterAttributes.DamRoll;
                case 9: return Mud.Domain.CharacterAttributes.MaxMovePoints;
                case 10: return Mud.Domain.CharacterAttributes.ArmorBash;
                case 11: return Mud.Domain.CharacterAttributes.ArmorPierce;
                case 12: return Mud.Domain.CharacterAttributes.ArmorSlash;
                case 13: return Mud.Domain.CharacterAttributes.ArmorExotic;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributes {attr} while reading pfile");
                    return Mud.Domain.CharacterAttributes.Strength;
            }
        }

        private int MapCharacterAttributes(Mud.Domain.CharacterAttributes attr)
        {
            switch (attr)
            {
                case Mud.Domain.CharacterAttributes.Strength: return 0;
                case Mud.Domain.CharacterAttributes.Intelligence: return 1;
                case Mud.Domain.CharacterAttributes.Wisdom: return 2;
                case Mud.Domain.CharacterAttributes.Dexterity: return 3;
                case Mud.Domain.CharacterAttributes.Constitution: return 4;
                case Mud.Domain.CharacterAttributes.MaxHitPoints: return 5;
                case Mud.Domain.CharacterAttributes.SavingThrow: return 6;
                case Mud.Domain.CharacterAttributes.HitRoll: return 7;
                case Mud.Domain.CharacterAttributes.DamRoll: return 8;
                case Mud.Domain.CharacterAttributes.MaxMovePoints: return 9;
                case Mud.Domain.CharacterAttributes.ArmorBash: return 10;
                case Mud.Domain.CharacterAttributes.ArmorPierce: return 11;
                case Mud.Domain.CharacterAttributes.ArmorSlash: return 12;
                case Mud.Domain.CharacterAttributes.ArmorExotic: return 13;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CharacterAttributes {attr} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.ResourceKinds MapResourceKind(int resource)
        {
            switch (resource)
            {
                case 0: return Mud.Domain.ResourceKinds.Mana;
                case 1: return Mud.Domain.ResourceKinds.Psy;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while reading pfile");
                    return Mud.Domain.ResourceKinds.Mana;
            }
        }

        private int MapResourceKind(Mud.Domain.ResourceKinds resource)
        {
            switch (resource)
            {
                case Mud.Domain.ResourceKinds.Mana: return 0;
                case Mud.Domain.ResourceKinds.Psy: return 1;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.ResourceKinds? MapNullableResourceKind(int resource)
        {
            switch (resource)
            {
                case -1: return null;
                case 0: return Mud.Domain.ResourceKinds.Mana;
                case 1: return Mud.Domain.ResourceKinds.Psy;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while reading pfile");
                    return Mud.Domain.ResourceKinds.Mana;
            }
        }

        private int MapNullableResourceKind(Mud.Domain.ResourceKinds? resource)
        {
            if (!resource.HasValue)
                return -1;
            switch (resource.Value)
            {
                case Mud.Domain.ResourceKinds.Mana: return 0;
                case Mud.Domain.ResourceKinds.Psy: return 1;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid ResourceKinds {resource} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.CostAmountOperators MapCostAmountOperator(int op)
        {
            switch (op)
            {
                case 0: return Mud.Domain.CostAmountOperators.None;
                case 1: return Mud.Domain.CostAmountOperators.Fixed;
                case 2: return Mud.Domain.CostAmountOperators.Percentage;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CostAmountOperators {op} while reading pfile");
                    return 0;
            }
        }

        private int MapCostAmountOperator(Mud.Domain.CostAmountOperators op)
        {
            switch (op)
            {
                case Mud.Domain.CostAmountOperators.None: return 0;
                case Mud.Domain.CostAmountOperators.Fixed: return 1;
                case Mud.Domain.CostAmountOperators.Percentage: return 2;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid CostAmountOperators {op} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.Conditions MapConditions(int cond)
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

        private int MapConditions(Mud.Domain.Conditions cond)
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

        private Mud.Domain.PortalFlags MapPortalFlags(int flags)
        {
            return (Mud.Domain.PortalFlags)flags;
        }

        private int MapPortalFlags(Mud.Domain.PortalFlags flags)
        {
            return (int)flags;
        }

        private Mud.Domain.ContainerFlags MapContainerFlags(int flags)
        {
            return (Mud.Domain.ContainerFlags)flags;
        }

        private int MapContainerFlags(Mud.Domain.ContainerFlags flags)
        {
            return (int)flags;
        }

        private Mud.Domain.Sizes MapSizes(int size)
        {
            switch (size)
            {
                case 0: return Mud.Domain.Sizes.Tiny;
                case 1: return Mud.Domain.Sizes.Small;
                case 2: return Mud.Domain.Sizes.Medium;
                case 3: return Mud.Domain.Sizes.Large;
                case 4: return Mud.Domain.Sizes.Huge;
                case 5: return Mud.Domain.Sizes.Giant;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sizes {size} while reading pfile");
                    return Mud.Domain.Sizes.Tiny;
            }
        }

        private int MapSizes(Mud.Domain.Sizes size)
        {
            switch (size)
            {
                case Mud.Domain.Sizes.Tiny: return 0;
                case Mud.Domain.Sizes.Small: return 1;
                case Mud.Domain.Sizes.Medium: return 2;
                case Mud.Domain.Sizes.Large: return 3;
                case Mud.Domain.Sizes.Huge: return 4;
                case Mud.Domain.Sizes.Giant: return 5;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid Sizes {size} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.AutoFlags MapAutoFlags(int flags)
        {
            return (Mud.Domain.AutoFlags) flags;
        }

        private int MapAutoFlags(Mud.Domain.AutoFlags flags)
        {
            return (int)flags;
        }

        private ICharacterFlags MapCharacterFlags(string flags)
        {
            return new CharacterFlags(flags);
        }

        private string MapCharacterFlags(ICharacterFlags flags)
        {
            return flags?.Map();
        }
    }
}
