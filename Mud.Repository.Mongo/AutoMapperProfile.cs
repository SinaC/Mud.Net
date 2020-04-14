using AutoMapper;

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
            CreateMap<Mud.Domain.AdminData, Domain.AdminData>()
                .ForMember(x => x.Level, expression => expression.MapFrom(x => MapAdminLevel(x.Level)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Mud.Domain.CharacterData, Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)));

            CreateMap<Mud.Domain.PlayerData, Domain.PlayerData>();
        }

        private void InternalToExternal()
        {
            CreateMap<Domain.AdminData, Mud.Domain.AdminData>()
                .ForMember(x => x.Level, expression => expression.MapFrom(x => MapAdminLevel(x.Level)))
                .ForMember(x => x.WiznetFlags, expression => expression.MapFrom(x => MapWiznetFlags(x.WiznetFlags)));

            CreateMap<Domain.CharacterData, Mud.Domain.CharacterData>()
                .ForMember(x => x.Sex, expression => expression.MapFrom(x => MapSex(x.Sex)));

            CreateMap<Domain.PlayerData, Mud.Domain.PlayerData>();
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
            return (Mud.Domain.AdminLevels)level;
        }

        private int MapAdminLevel(Mud.Domain.AdminLevels levels)
        {
            return (int)levels;
        }

        private Mud.Domain.Sex MapSex(int sex)
        {
            return (Mud.Domain.Sex)sex;
        }

        private int MapSex(Mud.Domain.Sex sex)
        {
            return (int)sex;
        }
    }
}
