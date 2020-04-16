﻿using System.Linq;
using MongoDB.Driver;

namespace Mud.Repository.Mongo
{
    public class AdminRepository : RepositoryBase<Domain.AdminData>, IAdminRepository
    {
        public AdminRepository() : base("Admin")
        {
        }

        public Mud.Domain.AdminData Load(string adminName)
        {
            Domain.AdminData adminData = MongoRepository.Collection.AsQueryable().FirstOrDefault(x => x.Name == adminName);

            if (adminData == null)
                return null;

            var mapped = Mapper.Map<Domain.AdminData, Mud.Domain.AdminData>(adminData);
            return mapped;
        }

        public void Save(Mud.Domain.AdminData adminData)
        {
            var mapped = Mapper.Map<Mud.Domain.AdminData, Domain.AdminData>(adminData);

            MongoRepository.Collection.ReplaceOne(x => x.Name == adminData.Name, mapped, new ReplaceOptions { IsUpsert = true });
        }
    }
}