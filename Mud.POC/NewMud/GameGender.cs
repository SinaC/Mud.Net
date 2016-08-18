namespace Mud.POC.NewMud
{
    public class GameGender
    {
        public string Name { get; }

        public string Abbreviation { get; }

        public GameGender(string name, string abbreviation)
        {
            Name = name;
            Abbreviation = abbreviation;
        }
    }
}
