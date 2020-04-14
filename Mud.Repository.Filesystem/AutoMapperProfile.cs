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

            CreateMap<Domain.ItemData, DataContracts.ItemData>();

            CreateMap<Domain.EquipedItemData, DataContracts.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));
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

            CreateMap<DataContracts.ItemData, Domain.ItemData>();

            CreateMap<DataContracts.EquipedItemData, Domain.EquipedItemData>()
                .ForMember(x => x.Slot, expression => expression.MapFrom(x => MapEquimentSlot(x.Slot)));
        }

        private List<DataContracts.PairData<TKey, TValue>> MapFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) => dictionary.Select(x => new DataContracts.PairData<TKey, TValue>(x.Key, x.Value)).ToList();

        private Dictionary<TKey, TValue> MapToDictionary<TKey, TValue>(List<DataContracts.PairData<TKey, TValue>> list) => list.ToDictionary(x => x.Key, x => x.Value);

        private Domain.WiznetFlags MapWiznetFlags(int flags)
        {
            return (Domain.WiznetFlags) flags;
        }

        private int MapWiznetFlags(Domain.WiznetFlags flags)
        {
            return (int) flags;
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
                    return Domain.EquipmentSlots.RingLeft;
                case 12:
                    return Domain.EquipmentSlots.RingRight;
                case 13:
                    return Domain.EquipmentSlots.Legs;
                case 14:
                    return Domain.EquipmentSlots.Feet;
                case 15:
                    return Domain.EquipmentSlots.Trinket1;
                case 16:
                    return Domain.EquipmentSlots.Trinket2;
                case 17:
                    return Domain.EquipmentSlots.Wield;
                case 18:
                    return Domain.EquipmentSlots.Wield2;
                case 19:
                    return Domain.EquipmentSlots.Hold;
                case 20:
                    return Domain.EquipmentSlots.Shield;
                case 21:
                    return Domain.EquipmentSlots.Wield2H;
                case 22:
                    return Domain.EquipmentSlots.Wield3;
                case 23:
                    return Domain.EquipmentSlots.Wield4;
                case 24:
                    return Domain.EquipmentSlots.Wield2H2;
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
                case Domain.EquipmentSlots.RingLeft:
                    return 11;
                case Domain.EquipmentSlots.RingRight:
                    return 12;
                case Domain.EquipmentSlots.Legs:
                    return 13;
                case Domain.EquipmentSlots.Feet:
                    return 14;
                case Domain.EquipmentSlots.Trinket1:
                    return 15;
                case Domain.EquipmentSlots.Trinket2:
                    return 16;
                case Domain.EquipmentSlots.Wield:
                    return 17;
                case Domain.EquipmentSlots.Wield2:
                    return 18;
                case Domain.EquipmentSlots.Hold:
                    return 19;
                case Domain.EquipmentSlots.Shield:
                    return 20;
                case Domain.EquipmentSlots.Wield2H:
                    return 21;
                case Domain.EquipmentSlots.Wield3:
                    return 22;
                case Domain.EquipmentSlots.Wield4:
                    return 23;
                case Domain.EquipmentSlots.Wield2H2:
                    return 24;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"Invalid EquipmentSlots {slot} while writing pfile");
                    return 0;
            }
        }
    }
}
