﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Mud.Logger;
using Mud.Repository.Filesystem.Common;
using Mud.Settings.Interfaces;

namespace Mud.Repository.Filesystem
{
    public class PlayerRepository : RepositoryBase, IPlayerRepository
    {
        private string PlayerRepositoryPath => Settings.PlayerRepositoryPath;

        private string BuildFilename(string playerName) => Path.Combine(PlayerRepositoryPath, playerName + ".data");

        public PlayerRepository(IMapper mapper, ISettings settings)
            : base(mapper, settings)
        {
        }

        #region IPlayerRepository

        public Mud.Domain.PlayerData Load(string playerName)
        {
            CreateDirectoryIfNeeded();
            string filename = BuildFilename(playerName);
            if (!File.Exists(filename))
                return null;

            Domain.PlayerData playerData = Load<Domain.PlayerData>(filename);
            var mapped = Mapper.Map<Domain.PlayerData, Mud.Domain.PlayerData>(playerData);

            return mapped;
        }

        public void Save(Mud.Domain.PlayerData playerData)
        {
            CreateDirectoryIfNeeded();
            var mapped = Mapper.Map<Mud.Domain.PlayerData, Domain.PlayerData>(playerData);

            string filename = BuildFilename(playerData.Name);
            Save(mapped, filename);
        }

        public void Delete(string playerName)
        {
            CreateDirectoryIfNeeded();
            string filename = BuildFilename(playerName);

            if (File.Exists(filename))
                File.Delete(filename);
        }

        public IEnumerable<string> GetAvatarNames()
        {
            CreateDirectoryIfNeeded();
            List<string> avatarNames = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(PlayerRepositoryPath))
            {
                Domain.PlayerData playerData = Load<Domain.PlayerData>(filename);
                if (playerData.Characters.Any())
                    avatarNames.AddRange(playerData.Characters.Select(x => x.Name));
            }
            return avatarNames.ToArray();
        }

        #endregion

        private void CreateDirectoryIfNeeded()
        {
            string directory = Path.GetDirectoryName(PlayerRepositoryPath);
            if (directory != null)
                Directory.CreateDirectory(directory);
            else
                Log.Default.WriteLine(LogLevels.Error, "Invalid directory in player path: {0}", PlayerRepositoryPath);
        }
    }
}
