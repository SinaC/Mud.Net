using AutoMapper;
using Mud.Container;

namespace Mud.Repository.Filesystem
{
    public abstract class RepositoryBase
    {
        protected IMapper Mapper => DependencyContainer.Instance.GetInstance<IMapper>();
    }
}
