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
    }
}
