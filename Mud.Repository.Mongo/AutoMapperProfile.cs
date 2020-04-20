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
                .Include<Mud.Domain.EquipedItemData, Domain.EquipedItemData>();

            CreateMap<Mud.Domain.EquipedItemData, Domain.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));
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
                .Include<Domain.EquipedItemData, Mud.Domain.EquipedItemData>();

            CreateMap<Domain.EquipedItemData, Mud.Domain.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));
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
    }
}
