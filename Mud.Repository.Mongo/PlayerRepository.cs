﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MongoDB.Driver;
using Mud.Repository.Mongo.Common;
using Mud.Settings.Interfaces;

namespace Mud.Repository.Mongo
{
    public class PlayerRepository : RepositoryBase<Domain.PlayerData>, IPlayerRepository
    {
        public PlayerRepository(IMapper mapper, ISettings settings)
            : base(mapper, settings, "Player")
        {
        }

        public Mud.Domain.PlayerData Load(string playerName)
        {
            Domain.PlayerData playerData = Collection.AsQueryable().FirstOrDefault(x => x.Name == playerName);

            if (playerData == null)
                return null;

            var mapped = Mapper.Map<Domain.PlayerData, Mud.Domain.PlayerData>(playerData);
            return mapped;
        }

        public void Save(Mud.Domain.PlayerData playerData)
        {
            var mapped = Mapper.Map<Mud.Domain.PlayerData, Domain.PlayerData>(playerData);

            Collection.ReplaceOne(x => x.Name == playerData.Name, mapped, new ReplaceOptions { IsUpsert = true });
        }

        public void Delete(string playerName)
        {
            Collection.DeleteOne(x => x.Name == playerName);
        }

        public IEnumerable<string> GetAvatarNames()
        {
            return Collection.AsQueryable().SelectMany(x => x.Characters).Select(x => x.Name).ToList();
        }
    }
}
