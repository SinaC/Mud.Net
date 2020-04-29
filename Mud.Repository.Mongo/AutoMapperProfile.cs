using AutoMapper;
using Mud.Logger;

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
                .ForMember(x => x.Level, expression => expression.MapFrom(x => MapAdminLevel(x.Level)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Mud.Domain.CharacterData, Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)));

            CreateMap<Mud.Domain.ItemData, Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Mud.Domain.ItemContainerData, Domain.ItemContainerData>()
                .Include<Mud.Domain.ItemCorpseData, Domain.ItemCorpseData>();
            CreateMap<Mud.Domain.ItemContainerData, Domain.ItemContainerData>();
            CreateMap<Mud.Domain.ItemCorpseData, Domain.ItemCorpseData>();

            CreateMap<Mud.Domain.EquipedItemData, Domain.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<Mud.Domain.CurrentQuestData, Domain.CurrentQuestData>();
            CreateMap<Mud.Domain.CurrentQuestObjectiveData, Domain.CurrentQuestObjectiveData>();

            CreateMap<Mud.Domain.AuraData, Domain.AuraData>()
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapAuraModifier(x.Modifier)))
                .ForMember(x => x.AmountOperator, expression => expression.MapFrom(x => MapAmountOperator(x.AmountOperator)));
        }

        private void InternalToExternal()
        {
            CreateMap<Domain.PlayerData, Mud.Domain.PlayerData>()
                .Include<Domain.AdminData, Mud.Domain.AdminData>();
            CreateMap<Domain.AdminData, Mud.Domain.AdminData>()
                .ForMember(x => x.Level, expression => expression.MapFrom(x => MapAdminLevel(x.Level)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Domain.CharacterData, Mud.Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)));

            CreateMap<Domain.ItemData, Mud.Domain.ItemData>()
                .ForMember(x => x.ItemFlags, expression => expression.MapFrom(x => MapItemFlags(x.ItemFlags)))
                .Include<Domain.ItemContainerData, Mud.Domain.ItemContainerData>()
                .Include<Domain.ItemCorpseData, Mud.Domain.ItemCorpseData>();
            CreateMap<Domain.ItemContainerData, Mud.Domain.ItemContainerData>();
            CreateMap<Domain.ItemCorpseData, Mud.Domain.ItemCorpseData>();

            CreateMap<Domain.EquipedItemData, Mud.Domain.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));

            CreateMap<Domain.CurrentQuestData, Mud.Domain.CurrentQuestData>();
            CreateMap<Domain.CurrentQuestObjectiveData, Mud.Domain.CurrentQuestObjectiveData>();

            CreateMap<Domain.AuraData, Mud.Domain.AuraData>()
                .ForMember(x => x.Modifier, expression => expression.MapFrom(x => MapAuraModifier(x.Modifier)))
                .ForMember(x => x.AmountOperator, expression => expression.MapFrom(x => MapAmountOperator(x.AmountOperator)));
        }

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

        private Mud.Domain.AuraModifiers MapAuraModifier(int modifier)
        {
            switch (modifier)
            {
                case 0: return Mud.Domain.AuraModifiers.None;
                case 1: return Mud.Domain.AuraModifiers.Strength;
                case 2: return Mud.Domain.AuraModifiers.Agility;
                case 3: return Mud.Domain.AuraModifiers.Stamina;
                case 4: return Mud.Domain.AuraModifiers.Intellect;
                case 5: return Mud.Domain.AuraModifiers.Spirit;
                case 6: return Mud.Domain.AuraModifiers.Characteristics;
                case 7: return Mud.Domain.AuraModifiers.AttackSpeed;
                case 8: return Mud.Domain.AuraModifiers.AttackPower;
                case 9: return Mud.Domain.AuraModifiers.SpellPower;
                case 10: return Mud.Domain.AuraModifiers.MaxHitPoints;
                case 11: return Mud.Domain.AuraModifiers.DamageAbsorb;
                case 12: return Mud.Domain.AuraModifiers.HealAbsorb;
                case 13: return Mud.Domain.AuraModifiers.Armor;
                case 14: return Mud.Domain.AuraModifiers.Critical;
                case 15: return Mud.Domain.AuraModifiers.Dodge;
                case 16: return Mud.Domain.AuraModifiers.Parry;
                case 17: return Mud.Domain.AuraModifiers.Block;
                case 18: return Mud.Domain.AuraModifiers.CharacterFlags;
                case 19: return Mud.Domain.AuraModifiers.SavingThrow;
                case 20: return Mud.Domain.AuraModifiers.HitRoll;
                case 21: return Mud.Domain.AuraModifiers.DamRoll;
                case 22: return Mud.Domain.AuraModifiers.Immunities;
                case 23: return Mud.Domain.AuraModifiers.Resistances;
                case 24: return Mud.Domain.AuraModifiers.Vulnerabilities;
                case 25: return Mud.Domain.AuraModifiers.Sex;
                case 26: return Mud.Domain.AuraModifiers.ItemFlags;
                case 27: return Mud.Domain.AuraModifiers.Dexterity;
                case 28: return Mud.Domain.AuraModifiers.MaxMovePoints;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraModifier {modifier} while reading pfile");
                    return Mud.Domain.AuraModifiers.None;
            }
        }

        private int MapAuraModifier(Mud.Domain.AuraModifiers modifier)
        {
            switch (modifier)
            {
                case Mud.Domain.AuraModifiers.None: return 0;
                case Mud.Domain.AuraModifiers.Strength: return 1;
                case Mud.Domain.AuraModifiers.Agility: return 2;
                case Mud.Domain.AuraModifiers.Stamina: return 3;
                case Mud.Domain.AuraModifiers.Intellect: return 4;
                case Mud.Domain.AuraModifiers.Spirit: return 5;
                case Mud.Domain.AuraModifiers.Characteristics: return 6;
                case Mud.Domain.AuraModifiers.AttackSpeed: return 7;
                case Mud.Domain.AuraModifiers.AttackPower: return 8;
                case Mud.Domain.AuraModifiers.SpellPower: return 9;
                case Mud.Domain.AuraModifiers.MaxHitPoints: return 10;
                case Mud.Domain.AuraModifiers.DamageAbsorb: return 11;
                case Mud.Domain.AuraModifiers.HealAbsorb: return 12;
                case Mud.Domain.AuraModifiers.Armor: return 13;
                case Mud.Domain.AuraModifiers.Critical: return 14;
                case Mud.Domain.AuraModifiers.Dodge: return 15;
                case Mud.Domain.AuraModifiers.Parry: return 16;
                case Mud.Domain.AuraModifiers.Block: return 17;
                case Mud.Domain.AuraModifiers.CharacterFlags: return 18;
                case Mud.Domain.AuraModifiers.SavingThrow: return 19;
                case Mud.Domain.AuraModifiers.HitRoll: return 20;
                case Mud.Domain.AuraModifiers.DamRoll: return 21;
                case Mud.Domain.AuraModifiers.Immunities: return 22;
                case Mud.Domain.AuraModifiers.Resistances: return 23;
                case Mud.Domain.AuraModifiers.Vulnerabilities: return 24;
                case Mud.Domain.AuraModifiers.Sex: return 25;
                case Mud.Domain.AuraModifiers.ItemFlags: return 26;
                case Mud.Domain.AuraModifiers.Dexterity: return 27;
                case Mud.Domain.AuraModifiers.MaxMovePoints: return 28;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraModifier {modifier} while writing pfile");
                    return 0;
            }
        }

        private Mud.Domain.AmountOperators MapAmountOperator(int op)
        {
            switch (op)
            {
                case 0: return Mud.Domain.AmountOperators.None;
                case 1: return Mud.Domain.AmountOperators.Fixed;
                case 2: return Mud.Domain.AmountOperators.Percentage;
                case 3: return Mud.Domain.AmountOperators.Flags;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraOperator {op} while reading pfile");
                    return Mud.Domain.AmountOperators.None;
            }
        }

        private int MapAmountOperator(Mud.Domain.AmountOperators op)
        {
            switch (op)
            {
                case Mud.Domain.AmountOperators.None: return 0;
                case Mud.Domain.AmountOperators.Fixed: return 1;
                case Mud.Domain.AmountOperators.Percentage: return 2;
                case Mud.Domain.AmountOperators.Flags: return 3;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid AuraOperator {op} while writing pfile");
                    return 0;
            }
        }
    }
}
