using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLua;
using NLua.Exceptions;

namespace Mud.POC.TestLua
{
    public interface ILuaComplexEntityTrigger
    {
        void Trigger(string param1, int param2);
    }

    public interface ILuaComplexEntity
    {
        string Name { get; }

        void ShouldBeVisibleInLua(int param1, string param2);
    }

    public interface IComplexEntity
    {
        int Value { get; }

        void ShouldNotBeVisibleInLua(string param1);
    }

    public class ComplexEntity : IComplexEntity, ILuaComplexEntity
    {
        public int Value => 5;

        public void ShouldNotBeVisibleInLua(string param1)
        {
            Debug.WriteLine("!!SHOULD NOT BE VISIBLE BECAUSE OF INTERFACE !!");
        }

        public string Name => "ComplexEntity";

        public void ShouldBeVisibleInLua(int param1, string param2)
        {
           Debug.WriteLine($"OK: '{param1}' '{param2}'");
        }

        private void PrivateMethod(string param1)
        {
            Debug.WriteLine("!!SHOULD NOT BE VISIBLE BECAUSE OF PRIVATE!!");
        }
    }

    public class ComplexEntityScript : LuaScript<ILuaComplexEntity>, ILuaComplexEntityTrigger
    {
        public ComplexEntityScript(Lua lua, ILuaComplexEntity entity, string scriptName)
            : base(lua, entity, scriptName)
        {
        }

        private int _triggerCallCount = 0;

        public void Trigger(string param1, int param2)
        {
            if (_triggerCallCount > 0)
                throw new InvalidOperationException("RecursiveTriger");
            _triggerCallCount++;
            Debug.WriteLine("DEBUG: ComplexEntityScript:Trigger");
            LuaFunction luaFunction = GetLuaFunction("Trigger");
            luaFunction?.Call(Entity, param1, param2);
        }
    }

    public class AlmostCorrectComplexEntityScript : LuaScript<ComplexEntity>, ILuaComplexEntity, ILuaComplexEntityTrigger
    {
        public AlmostCorrectComplexEntityScript(Lua lua, ComplexEntity entity, string tableName)
            : base(lua, entity, tableName)
        {
        }

        // Wrap around visible entity functions
        public string Name => "AlmostCorrectComplexEntityScript";

        public void ShouldBeVisibleInLua(int param1, string param2)
        {
            Entity.ShouldBeVisibleInLua(param1, param2);
        }

        private int _triggerCallCount = 0;

        public void Trigger(string param1, int param2) // !! This method can be called from Lua which will cause a recursive call
        {
            if (_triggerCallCount > 0)
                throw new InvalidOperationException("RecursiveTrigger");
            _triggerCallCount++;

            Debug.WriteLine("DEBUG: CorrectComplexEntityScript:Trigger");
            LuaFunction luaFunction = GetLuaFunction("Trigger");
            luaFunction?.Call(this, param1, param2); // Don't use 'Entity' as 'self' to avoid giving access to non-public methods
        }
    }

    public class CorrectComplexEntityScript : LuaScript<ComplexEntity>, ILuaComplexEntityTrigger
    {
        private readonly LuaComplexEntity _luaComplexEntity;

        public CorrectComplexEntityScript(Lua lua, ComplexEntity entity, string tableName)
            : base(lua, entity, tableName)
        {
            _luaComplexEntity = new LuaComplexEntity(entity);
        }

        private class LuaComplexEntity : ILuaComplexEntity // class implementing lua accessible methods/properties (simply a wrapper)
        {
            private readonly ComplexEntity _complexEntity;

            public LuaComplexEntity(ComplexEntity complexEntity)
            {
                _complexEntity = complexEntity;
            }

            public string Name => "LuaComplexEntity";

            public void ShouldBeVisibleInLua(int param1, string param2)
            {
                _complexEntity.ShouldBeVisibleInLua(param1, param2);
            }
        }

        private int _triggerCallCount = 0;

        public void Trigger(string param1, int param2) // !! This method can be called from Lua which will cause a recursive call
        {
            if (_triggerCallCount > 0)
                throw new InvalidOperationException("RecursiveTrigger");
            _triggerCallCount++;

            Debug.WriteLine("DEBUG: CorrectComplexEntityScript:Trigger");
            LuaFunction luaFunction = GetLuaFunction("Trigger");
            luaFunction?.Call(_luaComplexEntity, param1, param2); // Don't use 'Entity' neither 'this' as 'self' to avoid giving access to non-public methods and avoid recursive call
        }
    }

    public class NewCorrectComplexEntityScript : NewLuaScript<ComplexEntity, ILuaComplexEntity>, ILuaComplexEntityTrigger
    {
        public NewCorrectComplexEntityScript(Lua lua, ComplexEntity entity, string tableName)
            :base(lua, entity, new LuaComplexEntity(entity), tableName)
        {
        }

        private class LuaComplexEntity : ILuaComplexEntity // class implementing lua accessible methods/properties (simply a wrapper)
        {
            private readonly ComplexEntity _complexEntity;

            public LuaComplexEntity(ComplexEntity complexEntity)
            {
                _complexEntity = complexEntity;
            }

            public string Name => "LuaComplexEntity";

            public void ShouldBeVisibleInLua(int param1, string param2)
            {
                _complexEntity.ShouldBeVisibleInLua(param1, param2);
            }
        }

        private int _triggerCallCount = 0;

        public void Trigger(string param1, int param2) // !! This method can be called from Lua which will cause a recursive call
        {
            if (_triggerCallCount > 0)
                throw new InvalidOperationException("RecursiveTrigger");
            _triggerCallCount++;

            Debug.WriteLine("DEBUG: CorrectComplexEntityScript:Trigger");
            CallLuaFunction("Trigger", param1, param2);
        }
    }

    public class TestLuaFunctionHiding
    {
        public void Test()
        {
            Lua lua = new Lua();
            lua.RegisterFunction("print", typeof(LuaOutput).GetMethod("Print"));

            lua.DoString(
                @"
import = function () end -- avoid end-user importing external modules
");

            lua.DoString(
                @"
testVisible={}
function testVisible:Trigger(param1, param2)
    print('testVisible:Trigger:'..tostring(param1)..','..tostring(param2));
    print(tostring(self));

    self:ShouldBeVisibleInLua(7,'tsekwa');
end

testNotVisible={}
function testNotVisible:Trigger(param1, param2)
    print('testNotVisible:Trigger:'..tostring(param1)..','..tostring(param2));
    print(tostring(self));

    self:ShouldNotBeVisibleInLua('toto');
end

testPrivate={}
function testPrivate:Trigger(param1, param2)
    print('testPrivate:Trigger:'..tostring(param1)..','..tostring(param2));
    print(tostring(self));

    self:PrivateMethod();
end

testRecursive={}
function testRecursive:Trigger(param1, param2)
    print('testRecursive:Trigger:'..tostring(param1)..','..tostring(param2));
    print(tostring(self));

    self:Trigger('toto', 5); -- recursive call
end

testValue={}
function testValue:Trigger(param1, param2)
    print('testValue:Trigger:'..tostring(param1)..','..tostring(param2));
    print(tostring(self));

    print('Value: '..tostring(self.Value)); -- this should generate an exception or return nil but it returns a function
end

testName={}
function testName:Trigger(param1, param2)
    print('testName:Trigger:'..tostring(param1)..','..tostring(param2));
    print(tostring(self));

    print('Name: '..tostring(self.Name));
end
");

            ComplexEntity complexEntity = new ComplexEntity();

            //
            TestScripts(nameof(ComplexEntityScript), scriptName => new ComplexEntityScript(lua, complexEntity, scriptName));

            //
            TestScripts(nameof(AlmostCorrectComplexEntityScript), scriptName => new AlmostCorrectComplexEntityScript(lua, complexEntity, scriptName));

            //
            TestScripts(nameof(CorrectComplexEntityScript), scriptName => new CorrectComplexEntityScript(lua, complexEntity, scriptName));

            //
            TestScripts(nameof(NewCorrectComplexEntityScript), scriptName => new NewCorrectComplexEntityScript(lua, complexEntity, scriptName));

            //
            lua.Close();
        }

        public void TestScripts(string scriptHandlerName, Func<string, ILuaComplexEntityTrigger> getScriptHandlerFunc)
        {
            Debug.WriteLine("**************************************************************");
            Debug.WriteLine("***********************"+ scriptHandlerName+"*********************");
            Debug.WriteLine("**************************************************************");
            // testVisible cannot generate exception
            try
            {
                ILuaComplexEntityTrigger script = getScriptHandlerFunc("testVisible");
                script.Trigger(scriptHandlerName, 1);

                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testVisible: OK");
            }
            catch (LuaScriptException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testVisible: FAILED");
            }

            // testNotVisible must generate exception
            try
            {
                ILuaComplexEntityTrigger script = getScriptHandlerFunc("testNotVisible");
                script.Trigger(scriptHandlerName, 2);

                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testNotVisible: FAILED");
            }
            catch (LuaScriptException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testNotVisible: OK");
            }

            // testPrivate must generate exception
            try
            {
                ILuaComplexEntityTrigger script = getScriptHandlerFunc("testPrivate");
                script.Trigger(scriptHandlerName, 3);

                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testPrivate: FAILED");
            }
            catch (LuaScriptException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testPrivate: OK");
            }

            // testRecursive must generate exception
            try
            {
                ILuaComplexEntityTrigger script = getScriptHandlerFunc("testRecursive");
                script.Trigger(scriptHandlerName, 4);

                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testRecursive: FAILED");
            }
            catch (LuaScriptException ex)
                when (ex.InnerException is InvalidOperationException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testRecursive: FAILED");
            }
            catch (LuaScriptException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testRecursive: OK");
            }

            // testValue should generate exception (TODO current nlua version returns a function instead of nil or an exception)
            try
            {
                ILuaComplexEntityTrigger script = getScriptHandlerFunc("testValue");
                script.Trigger(scriptHandlerName, 5);

                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testValue: FAILED");
            }
            catch (LuaScriptException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testValue: OK");
            }

            // testName cannot generate exception
            try
            {
                ILuaComplexEntityTrigger script = getScriptHandlerFunc("testName");
                script.Trigger(scriptHandlerName, 6);

                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testName: OK");
            }
            catch (LuaScriptException)
            {
                Debug.WriteLine($"------>RESULT: {scriptHandlerName} testName: FAILED");
            }

        }
    }
}
