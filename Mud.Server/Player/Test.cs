using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Player
{
    [PlayerCommand("test", "!!Test!!")]
    public class Test : PlayerGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            TableGenerator<Tuple<string, string, int>> generator = new TableGenerator<Tuple<string, string, int>>();
            generator.AddColumn("Header1", 10, tuple => tuple.Item1);
            generator.AddColumn("Header2", 15, tuple => tuple.Item2);
            generator.AddColumn("Header3", 8, tuple => tuple.Item3.ToString());
            StringBuilder sb = generator.Generate("Test column duplicate", 3, Enumerable.Range(0, 50).Select(x => new Tuple<string, string, int>("Value1_" + x.ToString(), "Value2_" + (50 - x).ToString(), x)));
            Actor.Send(sb);

            //if (Impersonating != null)
            //{
            //    // Add quest to impersonated character is any
            //    QuestBlueprint questBlueprint1 = World.GetQuestBlueprint(1);
            //    QuestBlueprint questBlueprint2 = World.GetQuestBlueprint(2);
            //    INonPlayableCharacter questor = World.NonPlayableCharacters.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("questor"));

            //    IQuest quest1 = new Quest.Quest(questBlueprint1, Impersonating, questor);
            //    Impersonating.AddQuest(quest1);
            //    IQuest quest2 = new Quest.Quest(questBlueprint2, Impersonating, questor);
            //    Impersonating.AddQuest(quest2);
            //}

            //return true;

            //IQuest quest1 = new Quest.Quest(questBlueprint1, mob1, mob2);
            //mob1.AddQuest(quest1);
            //IQuest quest2 = new Quest.Quest(questBlueprint2, mob1, mob2);
            //mob1.AddQuest(quest2);

            ////Send("Player: DoTest" + Environment.NewLine);
            ////StringBuilder lorem = new StringBuilder("1/Lorem ipsum dolor sit amet, " + Environment.NewLine +
            ////                                        "2/consectetur adipiscing elit, " + Environment.NewLine +
            ////                                        "3/sed do eiusmod tempor incididunt " + Environment.NewLine +
            ////                                        "4/ut labore et dolore magna aliqua. " + Environment.NewLine +
            ////                                        "5/Ut enim ad minim veniam, " + Environment.NewLine +
            ////                                        "6/quis nostrud exercitation ullamco " + Environment.NewLine +
            ////                                        "7/laboris nisi ut aliquip ex " + Environment.NewLine +
            ////                                        "8/ea commodo consequat. " + Environment.NewLine +
            ////                                        "9/Duis aute irure dolor in " + Environment.NewLine +
            ////                                        "10/reprehenderit in voluptate velit " + Environment.NewLine +
            ////                                        "11/esse cillum dolore eu fugiat " + Environment.NewLine +
            ////                                        "12/nulla pariatur. " + Environment.NewLine +
            ////                                        "13/Excepteur sint occaecat " + Environment.NewLine +
            ////                                        "14/cupidatat non proident, " + Environment.NewLine +
            ////                                        "15/sunt in culpa qui officia deserunt " + Environment.NewLine //+
            ////                                        //"16/mollit anim id est laborum." + Environment.NewLine
            ////                                        );
            ////Page(lorem);
            //string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            //StringBuilder sb = new StringBuilder();
            //foreach (string word in lorem.Split(' ', ',', ';', '.'))
            //    sb.AppendLine(word);
            //Page(sb);
            //return true;
        }
    }
}
