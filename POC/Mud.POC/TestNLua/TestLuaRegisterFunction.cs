using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using NLua;

namespace Mud.POC.TestNLua
{
    //https://www.gamedev.net/articles/programming/general-and-gameplay-programming/using-lua-with-c-r2275/

    [AttributeUsage(AttributeTargets.Method)]
    public class AttrLuaFunc : Attribute
    {
        private string FunctionName;
        private string FunctionDoc;
        private string[] FunctionParameters = null!;

        public AttrLuaFunc(string strFuncName, string strFuncDoc, params string[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParamDocs;
        }

        public AttrLuaFunc(string strFuncName, string strFuncDoc)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
        }

        public string getFuncName()
        {
            return FunctionName;
        }

        public string getFuncDoc()
        {
            return FunctionDoc;
        }

        public string[] getFuncParams()
        {
            return FunctionParameters;
        }
    }

    public class LuaFuncDescriptor
    {
        private string FunctionName;
        private string FunctionDoc;
        private ArrayList FunctionParameters;
        private ArrayList FunctionParamDocs;
        private string FunctionDocString;

        public LuaFuncDescriptor(string strFuncName, string strFuncDoc, ArrayList strParams, ArrayList strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParams;
            FunctionParamDocs = strParamDocs;

            string strFuncHeader = strFuncName + "(%params%) - " + strFuncDoc;
            string strFuncBody = "\n\n";
            string strFuncParams = "";

            bool bFirst = true;

            foreach (var t in strParams)
            {
                if (!bFirst)
                    strFuncParams += ", ";

                strFuncParams += strParams;
                strFuncBody += "\t" + strParams + "\t\t" + strParamDocs + "\n";

                bFirst = false;
            }

            strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);
            if (bFirst)
                strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);

            FunctionDocString = strFuncHeader.Replace("%params%", strFuncParams) + strFuncBody;
        }

        public string getFuncName()
        {
            return FunctionName;
        }

        public string getFuncDoc()
        {
            return FunctionDoc;
        }

        public ArrayList getFuncParams()
        {
            return FunctionParameters;
        }

        public ArrayList getFuncParamDocs()
        {
            return FunctionParamDocs;
        }

        public string getFuncHeader()
        {
            if (FunctionDocString.IndexOf("\n") == -1)
                return FunctionDocString;

            return FunctionDocString.Substring(0, FunctionDocString.IndexOf("\n"));
        }

        public string getFuncFullDoc()
        {
            return FunctionDocString;
        }
    }

    public class TestRegisterFunctionEntity
    {
        public Guid Id { get; }
        public string Name { get; }

        public TestRegisterFunctionEntity(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            Debug.WriteLine($"Id: {Id}");
        }


        [AttrLuaFunc("quit", "Exit the program.")]
        public void quit()
        {
            Debug.WriteLine($"quit[{Id}]");
        }

        [AttrLuaFunc("pouet", "Pouet.", "param1 is the first param", "param2 is the second param")]
        public void pouet(int param1, string param2)
        {
            Debug.WriteLine($"pouet[{Id}]:{param1} {param2}");
        }

        //[AttrLuaFunc("help", "List available commands.")]
        //public void help()
        //{
        //    Debug.WriteLine("Available commands: ");
        //    Debug.WriteLine();

        //    IDictionaryEnumerator Funcs = pLuaFuncs.GetEnumerator();
        //    while (Funcs.MoveNext())
        //    {
        //        Debug.WriteLine(((LuaFuncDescriptor)Funcs.Value).getFuncHeader());
        //    }
        //}

        //[AttrLuaFunc("helpcmd", "Show help for a given command", "Command to get help of.")]
        //public void help(String strCmd)
        //{
        //    if (!pLuaFuncs.ContainsKey(strCmd))
        //    {
        //        Debug.WriteLine("No such function or package: " + strCmd);
        //        return;
        //    }

        //    LuaFuncDescriptor pDesc = (LuaFuncDescriptor)pLuaFuncs[strCmd];
        //    Debug.WriteLine(pDesc.getFuncFullDoc());
        //}
    }

    public class TestLuaRegisterFunction
    {
        public NLua.Lua pLuaVM { get; }
        public Hashtable pLuaFuncs { get; }

        public TestLuaRegisterFunction()
        {
            pLuaVM = new NLua.Lua();
            pLuaFuncs = new Hashtable();
        }

        public void Test()
        {
            TestRegisterFunctionEntity entity1 = new TestRegisterFunctionEntity("Entity1");
            registerLuaFunctions(entity1);
            TestRegisterFunctionEntity entity2 = new TestRegisterFunctionEntity("Entity2");
            registerLuaFunctions(entity2); // this will overwrite functions registered by previous registerLuaFunctions

            pLuaVM.RegisterFunction("print", typeof(LuaOutput).GetMethod("Print"));

            for (int i = 0; i < 5; i++)
            {
                Debug.WriteLine($"RUN {i}");
                pLuaVM.DoString(@"
quit();
pouet(5, 'test');
print(tostring(Id));
print(tostring(self));
");
            }
        }

        public void registerLuaFunctions(object pTarget)
        {
            // Sanity checks
            if (pLuaVM == null || pLuaFuncs == null)
                return;

            // Get the target type
            Type pTrgType = pTarget.GetType();

            // ... and simply iterate through all it's methods
            foreach (MethodInfo mInfo in pTrgType.GetMethods())
            {
                // ... then through all this method's attributes
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    // and if they happen to be one of our AttrLuaFunc attributes
                    if (attr.GetType() == typeof(AttrLuaFunc))
                    {
                        AttrLuaFunc pAttr = (AttrLuaFunc)attr;
                        Hashtable pParams = new Hashtable();

                        // Get the desired function name and doc string, along with parameter info
                        string strFName = pAttr.getFuncName();
                        string strFDoc = pAttr.getFuncDoc();
                        string[] pPrmDocs = pAttr.getFuncParams();

                        // Now get the expected parameters from the MethodInfo object
                        ParameterInfo[] pPrmInfo = mInfo.GetParameters();

                        // If they don't match, someone forgot to add some documentation to the
                        // attribute, complain and go to the next method
                        if (pPrmInfo.Length > 0 && pPrmInfo.Length != pPrmDocs.Length)
                        {
                            Debug.WriteLine("Function " + mInfo.Name + " (exported as " +
                                              strFName + ") argument number mismatch. Declared " +
                                              pPrmDocs.Length + " but requires " +
                                              pPrmInfo.Length + ".");
                            break;
                        }

                        // Build a parameter <-> parameter doc hashtable
                        for (int i = 0; i < pPrmInfo.Length; i++)
                        {
                            pParams.Add(pPrmInfo[i].Name!, pPrmDocs[i]);
                        }

                        // TODO: fix
                        //// Get a new function descriptor from this information
                        //LuaFuncDescriptor pDesc = new LuaFuncDescriptor(strFName, strFDoc, pParams);

                        //// Add it to the global hashtable
                        //pLuaFuncs.Add(strFName, pDesc);

                        // And tell the VM to register it.
                        pLuaVM.RegisterFunction(strFName, pTarget, mInfo);
                        Debug.WriteLine($"RegisterFunction:{strFName}");
                    }
                }
            }
        }
    }
}
