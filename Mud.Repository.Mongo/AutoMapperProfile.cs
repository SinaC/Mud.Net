using AutoMapper;
using Mud.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Repository.Mongo
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
                .Include<Mud.Domain.AdminData, Domain.AdminData>();
            CreateMap<Mud.Domain.AdminData, Domain.AdminData>()
                .ForMember(x => x.AdminLevel, expression => expression.MapFrom(x => MapAdminLevel(x.AdminLevel)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Mud.Domain.CharacterData, Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)))
                .ForMember(x => x.CharacterFlags, expression => expression.MapFrom(x => MapCharacterFlags(x.CharacterFlags)))
                .ForMember(x => x.Immunities, expression => expression.MapFrom(x => MapIRVFlags(x.Immunities)))
                .ForMember(x => x.Resistances, expression => expression.MapFrom(x => MapIRVFlags(x.Resistances)))
                .ForMember(x => x.Vulnerabilities, expression => expression.MapFrom(x => MapIRVFlags(x.Vulnerabilities)))
                .ForMember(x => x.Attributes, expression => expression.MapFrom(x => MapFromDictionary(x.Attributes, MapCharacterAttributes)))
                .ForMember(x => x.CurrentResources, expression => expression.MapFrom(x => MapFromDictionary(x.CurrentResources, MapResourceKind)))
                .ForMember(x => x.MaxResources, expression => expression.MapFrom(x => MapFromDictionary(x.MaxResources, MapResourceKind)))
                .ForMember(x => x.Conditions, expression => expression.MapFrom(x => MapFromDictionary(x.Conditions, MapConditions)));

            CreateMap<Mud.Domain.ItemData, Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Mud.Domain.ItemContainerData, Domain.ItemContainerData>()
                .Include<Mud.Domain.ItemCorpseData, Domain.ItemCorpseData>()
                .Include<Mud.Domain.ItemWeaponData, Domain.ItemWeaponData>()
                .Include<Mud.Domain.ItemDrinkContainerData, Domain.ItemDrinkContainerData>()
                .Include<Mud.Domain.ItemFoodData, Domain.ItemFoodData>();
            CreateMap<Mud.Domain.ItemContainerData, Domain.ItemContainerData>();
            CreateMap<Mud.Domain.ItemCorpseData, Domain.ItemCorpseData>();
            CreateMap<Mud.Domain.ItemWeaponData, Domain.ItemWeaponData>()
                .ForMember(x => x.WeaponFlags, expression => expression.MapFrom(x => MapWeaponFlags(x.WeaponFlags)));
            CreateMap<Mud.Domain.ItemDrinkContainerData, Domain.ItemDrinkContainerData>();
            CreateMap<Mud.Domain.ItemFoodData, Domain.ItemFoodData>();

            CreateMap<Mud.Domain.EquipedItemData, Domain.EquipedItemData>()
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
                .Include<Mud.Domain.ItemWeaponFlagsAffectData, Domain.ItemWeaponFlagsAffectData>();
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

            CreateMap<Mud.Domain.KnownAbilityData, Domain.KnownAbilityData>()
                .ForMember(x => x.ResourceKind, expression => expression.MapFrom(x => MapNullableResourceKind(x.ResourceKind)))
                .ForMember(x => x.CostAmountOperator, expression => expression.MapFrom(x => MapCostAmountOperator(x.CostAmountOperator)));
        }

        private void InternalToExternal()
        {
            CreateMap<Domain.PlayerData, Mud.Domain.PlayerData>()
                .Include<Domain.AdminData, Mud.Domain.AdminData>();
            CreateMap<Domain.AdminData, Mud.Domain.AdminData>()
                .ForMember(x => x.AdminLevel, expression => expression.MapFrom(x => MapAdminLevel(x.AdminLevel)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Domain.CharacterData, Mud.Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)))
                .ForMember(x => x.CharacterFlags, expression => expression.MapFrom(x => MapCharacterFlags(x.CharacterFlags)))
                .ForMember(x => x.Immunities, expression => expression.MapFrom(x => MapIRVFlags(x.Immunities)))
                .ForMember(x => x.Resistances, expression => expression.MapFrom(x => MapIRVFlags(x.Resistances)))
                .ForMember(x => x.Vulnerabilities, expression => expression.MapFrom(x => MapIRVFlags(x.Vulnerabilities)))
                .ForMember(x => x.Attributes, expression => expression.MapFrom(x => MapToDictionary(x.Attributes, MapCharacterAttributes)))
                .ForMember(x => x.CurrentResources, expression => expression.MapFrom(x => MapToDictionary(x.CurrentResources, MapResourceKind)))
                .ForMember(x => x.MaxResources, expression => expression.MapFrom(x => MapToDictionary(x.MaxResources, MapResourceKind)))
                .ForMember(x => x.Conditions, expression => expression.MapFrom(x => MapToDictionary(x.Conditions, MapConditions)));

            CreateMap<Domain.ItemData, Mud.Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Domain.ItemContainerData, Mud.Domain.ItemContainerData>()
                .Include<Domain.ItemCorpseData, Mud.Domain.ItemCorpseData>()
                .Include<Domain.ItemWeaponData, Mud.Domain.ItemWeaponData>()
                .Include<Domain.ItemDrinkContainerData, Mud.Domain.ItemDrinkContainerData>()
                .Include<Domain.ItemFoodData, Mud.Domain.ItemFoodData>();
            CreateMap<Domain.ItemContainerData, Mud.Domain.ItemContainerData>();
            CreateMap<Domain.ItemCorpseData, Mud.Domain.ItemCorpseData>();
            CreateMap<Domain.ItemWeaponData, Mud.Domain.ItemWeaponData>()
                .ForMember(x => x.WeaponFlags, expression => expression.MapFrom(x => MapWeaponFlags(x.WeaponFlags)));
            CreateMap<Domain.ItemDrinkContainerData, Mud.Domain.ItemDrinkContainerData>();
            CreateMap<Domain.ItemFoodData, Mud.Domain.ItemFoodData>();

            CreateMap<Domain.EquipedItemData, Mud.Domain.EquipedItemData>()
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
                .Include<Domain.ItemWeaponFlagsAffectData, Mud.Domain.ItemWeaponFlagsAffectData>();
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

            CreateMap<Domain.KnownAbilityData, Mud.Domain.KnownAbilityData>()
                .ForMember(x => x.ResourceKind, expression => expression.MapFrom(x => MapNullableResourceKind(x.ResourceKind)))
                .ForMember(x => x.CostAmountOperator, expression => expression.MapFrom(x => MapCostAmountOperator(x.CostAmountOperator)));
        }

        private Dictionary<int, int> MapFromDictionary<T>(Dictionary<T, int> dictionary, Func<T, int> mapKeyFunc) => dictionary?.ToLookup(x => mapKeyFunc(x.Key), x => x.Value).ToDictionary(x => x.Key, x => x.First());

        private Dictionary<T, int> MapToDictionary<T>(Dictionary<int, int> dictionary, Func<int,T> mapKeyFunc) => dictionary?.ToLookup(x => mapKeyFunc(x.Key), x => x.Value).ToDictionary(x => x.Key, x => x.First());


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
                    return Mud.Domain.EquipmentSlots.Shoulders;
                case 5:
                    return Mud.Domain.EquipmentSlots.Chest;
                case 6:
                    return Mud.Domain.EquipmentSlots.Cloak;
                case 7:
                    return Mud.Domain.EquipmentSlots.Waist;
                case 8:
                    return Mud.Domain.EquipmentSlots.Wrists;
                case 9:
                    return Mud.Domain.EquipmentSlots.Arms;
                case 10:
                    return Mud.Domain.EquipmentSlots.Hands;
                case 11:
                    return Mud.Domain.EquipmentSlots.Ring;
                case 12:
                    return Mud.Domain.EquipmentSlots.Legs;
                case 13:
                    return Mud.Domain.EquipmentSlots.Feet;
                case 14:
                    return Mud.Domain.EquipmentSlots.Trinket;
                case 15:
                    return Mud.Domain.EquipmentSlots.MainHand;
                case 16:
                    return Mud.Domain.EquipmentSlots.OffHand;
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
                case Mud.Domain.EquipmentSlots.Shoulders:
                    return 4;
                case Mud.Domain.EquipmentSlots.Chest:
                    return 5;
                case Mud.Domain.EquipmentSlots.Cloak:
                    return 6;
                case Mud.Domain.EquipmentSlots.Waist:
                    return 7;
                case Mud.Domain.EquipmentSlots.Wrists:
                    return 8;
                case Mud.Domain.EquipmentSlots.Arms:
                    return 9;
                case Mud.Domain.EquipmentSlots.Hands:
                    return 10;
                case Mud.Domain.EquipmentSlots.Ring:
                    return 11;
                case Mud.Domain.EquipmentSlots.Legs:
                    return 12;
                case Mud.Domain.EquipmentSlots.Feet:
                    return 13;
                case Mud.Domain.EquipmentSlots.Trinket:
                    return 14;
                case Mud.Domain.EquipmentSlots.MainHand:
                    return 15;
                case Mud.Domain.EquipmentSlots.OffHand:
                    return 16;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid EquipmentSlots {slot} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.ItemFlags MapItemFlags(int flags)
        {
            return (Mud.Domain.ItemFlags)flags;
        }

        private int MapItemFlags(Mud.Domain.ItemFlags flags)
        {
            return (int)flags;
        }

        private Mud.Domain.AuraFlags MapAuraFlags(int flags)
        {
            return (Mud.Domain.AuraFlags)flags;
        }

        private int MapAuraFlags(Mud.Domain.AuraFlags flags)
        {
            return (int)flags;
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

        private Mud.Domain.CharacterFlags MapCharacterFlags(int flags)
        {
            return (Mud.Domain.CharacterFlags)flags;
        }

        private int MapCharacterFlags(Mud.Domain.CharacterFlags flags)
        {
            return (int)flags;
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

        private Mud.Domain.IRVFlags MapIRVFlags(int flags)
        {
            return (Mud.Domain.IRVFlags)flags;
        }

        private int MapIRVFlags(Mud.Domain.IRVFlags flags)
        {
            return (int)flags;
        }

        private Mud.Domain.WeaponFlags MapWeaponFlags(int flags)
        {
            return (Mud.Domain.WeaponFlags)flags;
        }

        private int MapWeaponFlags(Mud.Domain.WeaponFlags flags)
        {
            return (int)flags;
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
                    return Mud.Domain.CostAmountOperators.None;
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
            switch(cond)
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
    }
}
