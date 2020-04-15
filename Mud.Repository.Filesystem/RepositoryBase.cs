using AutoMapper;
using Mud.Container;
using Mud.Settings;

namespace Mud.Repository.Filesystem
{
    public abstract class RepositoryBase
    {
        protected IMapper Mapper => DependencyContainer.Instance.GetInstance<IMapper>();
        protected ISettings Settings => DependencyContainer.Instance.GetInstance<ISettings>();
    }
}
