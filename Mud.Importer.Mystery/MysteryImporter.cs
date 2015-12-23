using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Logger;

namespace Mud.Importer.Mystery
{
    public class MysteryImporter : TextBasedImporter
    {
        private const string AreaDataHeader = "#AREADATA";
        private const string AreaDataHeader2 = "#AREA";
        private const string MobilesHeader = "#MOBILES";
        private const string ObjectsHeader = "#OBJECTS";
        private const string ResetsHeader = "#RESETS";
        private const string RoomsHeader = "#ROOMS";
        private const string ShopsHeader = "#SHOPS";
        private const string SpecialsHeader = "#SPECIALS";

        private readonly List<AreaData> _areas;
        private readonly List<MobileData> _mobiles;
        private readonly List<ObjectData> _objects;

        public IReadOnlyCollection<AreaData> Areas
        {
            get { return new ReadOnlyCollection<AreaData>(_areas); }
        }

        public IReadOnlyCollection<MobileData> Mobiles
        {
            get { return new ReadOnlyCollection<MobileData>(_mobiles);}
        }

        public IReadOnlyCollection<ObjectData> Objects
        {
            get { return new ReadOnlyCollection<ObjectData>(_objects); }
        }

        public MysteryImporter()
        {
            _areas = new List<AreaData>();
            _mobiles = new List<MobileData>();
            _objects = new List<ObjectData>();
        }

        public override void Parse()
        {
            while (true)
            {
                string word = ReadWord();

                if (word[0] == '$')
                    break; // done
                if (word == AreaDataHeader || word == AreaDataHeader2)
                    ParseAreaData();
                else if (word == MobilesHeader)
                    ParseMobiles();
                else if (word == ObjectsHeader)
                    ParseObjects();
                else if (word == ResetsHeader)
                    ParseResets();
                else if (word == RoomsHeader)
                    ParseRooms();
                else if (word == ShopsHeader)
                    ParseShops();
                else if (word == SpecialsHeader)
                    ParseSpecials();
                else
                {
                    Log.Default.WriteLine(LogLevels.Error, "Bad section name");
                    throw new ParseException("Bad section name");
                }
            }
        }

        private void ParseAreaData()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Area section");

            AreaData area = new AreaData();
            bool parseComplete = false;
            while (!parseComplete)
            {
                if (IsEof())
                    break;
                string word = ReadWord();
                switch (word)
                {
                    case "Builders":
                        area.Builders = ReadString();
                        Log.Default.WriteLine(LogLevels.Debug, "Builders: " + area.Builders);
                        break;
                    case "Credits":
                        area.Credits = ReadString();
                        Log.Default.WriteLine(LogLevels.Debug, "Credits: " + area.Credits);
                        break;
                    case "Flags":
                        area.Flags = ReadFlags();
                        Log.Default.WriteLine(LogLevels.Debug, "Flags: {0:X}", area.Flags);
                        break;
                    case "Name":
                        area.Name = ReadString();
                        Log.Default.WriteLine(LogLevels.Debug, "Name: " + area.Name);
                        break;
                    case "Security":
                        area.Security = (int) ReadNumber();
                        Log.Default.WriteLine(LogLevels.Debug, "Security: " + area.Security);
                        break;
                    case "VNUMs":
                        area.MinVNum = (int) ReadNumber();
                        area.MaxVNum = (int) ReadNumber();
                        Log.Default.WriteLine(LogLevels.Debug, "VNums: {0} -> {1}", area.MinVNum, area.MaxVNum);
                        break;
                        // Parse complete
                    case "End":
                        parseComplete = true;
                        break;
                }
            }
            if (parseComplete)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Area [{0}] parsed", area.Name);

                // Set unique number and filename
                area.VNum = _areas.Count;
                area.FileName = CurrentFilename;
                // Save area
                _areas.Add(area);
            }
        }

        private void ParseMobiles()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Mobiles section");

            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                {
                    Log.Default.WriteLine(LogLevels.Error, "ParseMobiles: # not found");
                    throw new ParseException("ParseMobiles: # not found");
                }

                int vnum = (int) ReadNumber();
                if (vnum == 0)
                    break; // parsed

                if (_mobiles.Any(x => x.VNum == vnum))
                {
                    Log.Default.WriteLine(LogLevels.Error, "ParseMobiles: vnum {0} duplicated.", vnum);
                    throw new ParseException("ParseMobiles: vnum {0} duplicated.", vnum);
                }

                MobileData mobileData = new MobileData();
                mobileData.VNum = vnum;
                mobileData.Name = ReadString();
                mobileData.ShortDescr = ReadString();
                mobileData.LongDescr = UpperCaseFirst(ReadString());
                mobileData.Description = UpperCaseFirst(ReadString());
                mobileData.Race = ReadString();
                mobileData.Classes = ReadFlags();
                mobileData.Act = ReadFlags();
                mobileData.AffectedBy = ReadFlags();
                mobileData.AffectedBy2 = ReadFlags();
                mobileData.Etho = (int) ReadNumber();
                mobileData.Alignment = (int) ReadNumber();
                mobileData.Group = (int) ReadNumber();
                mobileData.Level = (int) ReadNumber();
                mobileData.HitRoll = (int) ReadNumber();
                ReadDice(mobileData.Hit);
                ReadDice(mobileData.Mana);
                ReadDice(mobileData.Psp);
                ReadDice(mobileData.Damage);
                mobileData.DamageType = ReadWord();
                mobileData.Armor[0] = (int) ReadNumber()*10;
                mobileData.Armor[1] = (int)ReadNumber() * 10;
                mobileData.Armor[2] = (int)ReadNumber() * 10;
                mobileData.Armor[3] = (int)ReadNumber() * 10;
                mobileData.OffFlags = ReadFlags();
                mobileData.ImmFlags = ReadFlags();
                mobileData.ResFlags = ReadFlags();
                mobileData.VulnFlags = ReadFlags();
                mobileData.StartPos = ReadWord();
                mobileData.DefaultPos = ReadWord();
                mobileData.Sex = ReadWord();
                mobileData.Wealth = ReadNumber();
                mobileData.Form = ReadFlags();
                mobileData.Parts = ReadFlags();
                mobileData.Size = ReadWord();
                mobileData.Material = ReadWord();

                while (true)
                {
                    letter = ReadLetter();

                    if (letter == 'F')
                    {
                        string category = ReadWord();
                        long vector = ReadFlags();
                        if (category.StartsWith("act"))
                            mobileData.Act &= ~vector;
                        else if (category.StartsWith("aff"))
                            mobileData.AffectedBy &= ~vector;
                        else if (category.StartsWith("aff2"))
                            mobileData.AffectedBy2 &= ~vector;
                        else if (category.StartsWith("off"))
                            mobileData.OffFlags &= ~vector;
                        else if (category.StartsWith("imm"))
                            mobileData.ImmFlags &= ~vector;
                        else if (category.StartsWith("res"))
                            mobileData.ResFlags &= ~vector;
                        else if (category.StartsWith("vul"))
                            mobileData.VulnFlags &= ~vector;
                        else if (category.StartsWith("for"))
                            mobileData.Form &= ~vector;
                        else if (category.StartsWith("par"))
                            mobileData.Parts &= ~vector;
                    }
                    else if (letter == 'D')
                    {
                        long dummy = ReadFlags(); // not used
                    }
                    else if (letter == 'M')
                    {
                        mobileData.Program = ReadWord();
                    }
                    else if (letter == 'Y')
                    {
                        // TODO: affects (see db2.C:419)
                        string where = ReadWord();
                        string location = ReadWord();
                        long value1 = ReadNumber();
                        long value2 = ReadNumber();
                    }
                    else
                    {
                        UngetChar();
                        break;
                    }
                }

                // TODO: convert act flags (see db.C:626)
                // TODO: fix parts (see db.C:520)

                Log.Default.WriteLine(LogLevels.Debug, "Mobile [{0}] parsed", vnum);

                // Save mobile data
                _mobiles.Add(mobileData);
            }
        }

        private void ParseObjects()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Objects section");

            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                {
                    Log.Default.WriteLine(LogLevels.Error, "ParseObjects: # not found");
                    throw new ParseException("ParseObjects: # not found");
                }

                int vnum = (int)ReadNumber();
                if (vnum == 0)
                    break; // parsed

                if (_objects.Any(x => x.VNum == vnum))
                {
                    Log.Default.WriteLine(LogLevels.Error, "ParseObjects: vnum {0} duplicated.", vnum);
                    throw new ParseException("ParseObjects: vnum {0} duplicated.", vnum);
                }

                ObjectData objectData = new ObjectData();
                objectData.VNum = vnum;
                objectData.Name = ReadString();
                objectData.ShortDescr = ReadString();
                objectData.Description = ReadString();
                objectData.Material = ReadString();
                objectData.ItemType = ReadWord();
                objectData.ExtraFlags = ReadFlags();
                objectData.WearFlags = ReadFlags();

                // Specific value depending on ItemType (see db2.C:564->681)
                switch (objectData.ItemType)
                {
                    case "component":
                        ReadWord();
                        ReadWord();
                        ReadWord();
                        ReadWord();
                        ReadWord();
                        break;
                    case "weapon":
                        ReadWord();
                        ReadNumber();
                        ReadNumber();
                        ReadWord();
                        ReadFlags();
                        break;
                    case "container":
                        ReadNumber();
                        ReadFlags();
                        ReadNumber();
                        ReadNumber();
                        ReadNumber();
                        break;
                    case "drink":
                    case "fountain":
                        ReadNumber();
                        ReadNumber();
                        ReadWord();
                        ReadNumber();
                        ReadFlags();
                        break;
                    case "wand":
                    case "staff":
                        ReadNumber();
                        ReadNumber();
                        ReadNumber();
                        ReadWord();
                        ReadNumber();
                        break;
                    case "potion":
                    case "pill":
                    case "scroll":
                    case "template":
                        ReadNumber();
                        ReadWord();
                        ReadWord();
                        ReadWord();
                        ReadWord();
                        break;
                    default:
                        objectData.Values[0] = ReadFlags();
                        objectData.Values[1] = ReadFlags();
                        objectData.Values[2] = ReadFlags();
                        objectData.Values[3] = ReadFlags();
                        objectData.Values[4] = ReadFlags();
                        break;
                }

                objectData.Level = (int) ReadNumber();
                objectData.Weight = (int) ReadNumber();
                objectData.Cost = ReadNumber();
                objectData.Condition = ReadLetter();

                while (true)
                {
                    letter = ReadLetter();
                    if (letter == 'S') // code moved into while loop
                    {
                        objectData.Size = ReadWord();
                    }
                    else if (letter == 'R')
                    {
                        // TODO: restriction (see db2.C:746)
                        string type = ReadWord();
                        long value = ReadNumber();
                        long notR = ReadNumber();
                    }
                    else if (letter == 'W')
                    {
                        // TODO: restriction (see db2.C:771)
                        string skill = ReadWord();
                        long value = ReadNumber();
                        long notR = ReadNumber();
                    }
                    else if (letter == 'Z')
                    {
                        // TODO: ability upgrade (see db2.C:811)
                        string skill = ReadWord();
                        long value = ReadNumber();
                    }
                    else if (letter == 'A')
                    {
                        // TODO: oldstyle affects (see db2.C:841)
                        int location = (int)ReadNumber();
                        int modifier = (int) ReadNumber();
                    }
                    else if (letter == 'F')
                    {
                        // TODO: affects (see db2.C:863)
                        letter = ReadLetter();
                        int location = (int)ReadNumber();
                        int modifier = (int)ReadNumber();
                        long vector = ReadFlags();
                    }
                    else if (letter == 'E')
                    {
                        string keyword = ReadString();
                        string description = ReadString();
                        objectData.ExtraDescr.Add(keyword, description);
                    }
                    else if (letter == 'Y')
                    {
                        // TODO: affects (see db2.C:948)
                        string where = ReadWord();
                        string location = ReadWord();
                        long value1 = ReadNumber();
                        long value2 = ReadNumber();
                    }
                    else if (letter == 'M')
                    {
                        objectData.Program = ReadWord();
                    }
                    else
                    {
                        UngetChar();
                        break;
                    }
                }

                Log.Default.WriteLine(LogLevels.Debug, "Object [{0}] parsed", vnum);

                // Save object data
                _objects.Add(objectData);
            }
        }

        private void ParseResets()
        {
        }

        private void ParseRooms()
        {
        }

        private void ParseShops()
        {
        }

        private void ParseSpecials()
        {
        }
    }
}
