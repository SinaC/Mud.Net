﻿namespace Mud.Domain
{
    public class CharacterFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public CharacterFlags Modifier { get; set; }
    }
}