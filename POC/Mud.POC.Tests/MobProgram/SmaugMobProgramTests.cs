using Mud.POC.MobProgram;
using Mud.POC.MobProgram.SmaugMobProgram;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.Tests.MobProgram;

[TestClass]
public class SmaugMobProgramTests
{
    [TestMethod]
    public void Sample1()
    {
        string script = @"
>greet_prog 100
if ispc($n)
  say Hello $n!
elseif isnpc($n)
  say I only talk to players.
else
  say I see nothing.
endif
~";

        var parser = new MobProgParser(script);
        var program = parser.Parse();

        var context = new MobContext
        {
            MobName = "Guardian",
            PlayerName = "Alice"
        };

        var interpreter = new MobProgInterpreter(program);
        interpreter.Execute(context);
    }

    [TestMethod]
    public void Sample2() 
    {
        string script = @"
>greet_prog 100
if carries $n 'raven tattoo'
or wears $n 'raven tattoo'
if name $n 'saphyre'
 obj echoat $n Your {Dblack{W, {wr{Da{wv{De{wn tattoo{x becomes real, and quickly grows to monstrous proportions, and carries you off.
  obj echoaround $n $n's {Dblack{W, {wr{Da{wv{De{wn tattoo{x becomes real, and quickly grows to monstrous proportions, and carries $m off.{x
 obj transfer $n 27506
else
 if isimmort $n
  obj echoat $n The {Dblack{W, {wr{Da{wv{De{wn tattoo{x becomes real, and quickly grows to monstrous proportions, and carries you home.
  obj echoaround $n $n's {Dblack{W, {wr{Da{wv{De{wn tattoo{x becomes real, and quickly grows to monstrous proportions, and carries $m off.{x
  obj force $n goto
 else
  obj echoaround $n The {Dblack{W, {wr{Da{wv{De{wn tattoo{x lets out a soul-crushing caw, and rips off of $n.
  obj echoat $n The {Dblack{W, {wr{Da{wv{De{wn tattoo{x lets out a soul-crushing caw, and rips off of you.
  obj damage $n 1000 1000 lethal
  obj remove $n 27812 
 endif
endif
else
 break
endif";


        var parser = new MobProgParser(script);
        var program = parser.Parse();

        var context = new MobContext
        {
            MobName = "Guardian",
            PlayerName = "Alice"
        };

        var interpreter = new MobProgInterpreter(program);
        interpreter.Execute(context);
    }
}
