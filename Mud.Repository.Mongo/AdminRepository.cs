using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MongoDB.Driver;
using Mud.Repository.Mongo.Common;
using Mud.Settings;

namespace Mud.Repository.Mongo
{
    public class AdminRepository : RepositoryBase<Domain.AdminData>, IAdminRepository
    {
        public AdminRepository(IMapper mapper, ISettings settings)
            : base(mapper, settings, "Admin")
        {
        }

        public Mud.Domain.AdminData Load(string adminName)
        {
            Domain.AdminData adminData = Collection.AsQueryable().FirstOrDefault(x => x.Name == adminName);

            if (adminData == null)
                return null;

            var mapped = Mapper.Map<Domain.AdminData, Mud.Domain.AdminData>(adminData);
            return mapped;
        }

        public void Save(Mud.Domain.AdminData adminData)
        {
            var mapped = Mapper.Map<Mud.Domain.AdminData, Domain.AdminData>(adminData);

            Collection.ReplaceOne(x => x.Name == adminData.Name, mapped, new ReplaceOptions { IsUpsert = true });
        }

        public IEnumerable<string> GetAvatarNames() 
        {
            return Collection.AsQueryable().SelectMany(x => x.Characters).Select(x => x.Name).ToList();
        }
    }
}
