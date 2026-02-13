using Mud.Common;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Race.Interfaces;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("CharacterRaceAffect", typeof(CharacterRaceAffectData))]
public class CharacterRaceAffect : ICharacterRaceAffect
{
    private IRaceManager RaceManager { get; }

    public IRace Race { get; set; } = null!;

    public CharacterRaceAffect(IRaceManager raceManager)
    {
        RaceManager = raceManager;
    }

    public void Initialize(CharacterRaceAffectData data)
    {
        var race = RaceManager[data.Race] ?? RaceManager.PlayableRaces.First();
        Race = race;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%Race %c%by setting to %y%{0}%x%", Race.Name.ToPascalCase());
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterRaceAffectData
        {
            Race = Race.Name
        };
    }
}
