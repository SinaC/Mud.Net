using System.Diagnostics;
using NLua;

namespace Mud.POC.TestLua
{
    public interface ILuaComplexEntity
    {
        void ShouldBeVisibleInLua();
    }

    public interface IComplexEntity
    {
        void ShouldNotBeVisibleInLua();
    }

    public class ComplexEntity : IComplexEntity, ILuaComplexEntity
    {
        public void ShouldNotBeVisibleInLua()
        {
            Debug.WriteLine("!!SHOULD NOT BE VISIBLE BECAUSE OF INTERFACE !!");
        }

        public void ShouldBeVisibleInLua()
        {
           Debug.WriteLine("OK");
        }

        private void PrivateMethod()
        {
            Debug.WriteLine("!!SHOULD NOT BE VISIBLE BECAUSE OF PRIVATE!!");
        }
    }

    public class CompleEntityScript : LuaScript<ILuaComplexEntity>
    {
        public CompleEntityScript(Lua lua, ILuaComplexEntity entity, string scriptName)
            : base(lua, entity, scriptName)
        {
        }

        public void Test()
        {
            LuaFunction luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity);
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
entity1={}
function entity1:Test()
    print('entity1:test');
    print(tostring(self));
    self:ShouldBeVisibleInLua();
    self:ShouldNotBeVisibleInLua();
    --self:PrivateMethod(); -- is hidden
end");

            ComplexEntity complexEntity = new ComplexEntity();
            CompleEntityScript complexEntityScript = new CompleEntityScript(lua, complexEntity, "entity1");

            complexEntityScript.Test();
        }
    }
}
