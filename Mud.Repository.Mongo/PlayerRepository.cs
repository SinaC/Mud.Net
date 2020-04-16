using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Mud.Repository.Mongo.Common;

namespace Mud.Repository.Mongo
{
    public class PlayerRepository : RepositoryBase<Domain.PlayerData>, IPlayerRepository
    {
        public PlayerRepository()
            : base("Player")
        {
        }

        public Mud.Domain.PlayerData Load(string playerName)
        {
            Domain.PlayerData playerData = MongoRepository.Collection.AsQueryable().FirstOrDefault(x => x.Name == playerName);

            if (playerData == null)
                return null;

            var mapped = Mapper.Map<Domain.PlayerData, Mud.Domain.PlayerData>(playerData);
            return mapped;
        }

        public void Save(Mud.Domain.PlayerData playerData)
        {
            var mapped = Mapper.Map<Mud.Domain.PlayerData, Domain.PlayerData>(playerData);

            MongoRepository.Collection.ReplaceOne(x => x.Name == playerData.Name, mapped, new ReplaceOptions { IsUpsert = true });
        }

        public void Delete(string playerName)
        {
            MongoRepository.Collection.DeleteOne(x => x.Name == playerName);
        }

        public IEnumerable<string> GetAvatarNames()
        {
            return MongoRepository.Collection.AsQueryable().SelectMany(x => x.Characters).Select(x => x.Name).ToList();
        }
    }
}
