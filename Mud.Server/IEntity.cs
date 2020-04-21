using System;
using System.Collections.Generic;

namespace Mud.Server
{
    public interface IEntity : IActor
    {
        Guid Id { get; }
        bool IsValid { get; } // always true unless entity has been removed from the game
        string Name { get; }
        IEnumerable<string> Keywords { get; }
        string DisplayName { get; }
        string Description { get; }
        string DebugName { get; }

        bool Incarnatable { get; }
        IAdmin IncarnatedBy { get; }

        bool ChangeIncarnation(IAdmin admin);

        string RelativeDisplayName(INonPlayableCharacter beholder, bool capitalizeFirstLetter = false); // Use to get DisplayName relative to Beholder. If Beholder cannot see 'this', it will return Someone or Something. It 'this' is quest objective, (Quest) will be prefixed

        string RelativeDisplayName(IPlayableCharacter beholder, bool capitalizeFirstLetter = false); // Use to get DisplayName relative to Beholder. If Beholder cannot see 'this', it will return Someone or Something. It 'this' is quest objective, (Quest) will be prefixed

        string RelativeDescription(INonPlayableCharacter beholder); // Add (Quest) to description if beholder is on a quest with 'this' as objective

        string RelativeDescription(IPlayableCharacter beholder); // Add (Quest) to description if beholder is on a quest with 'this' as objective

        void OnRemoved(); // called before removing an item from the game
    }
}
