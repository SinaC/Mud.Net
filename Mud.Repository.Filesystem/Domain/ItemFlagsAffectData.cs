﻿namespace Mud.Repository.Filesystem.Domain
{
    public class ItemFlagsAffectData : AffectDataBase
    {
        public int Operator { get; set; } // Add and Or are identical

        public string Modifier { get; set; }
    }
}
