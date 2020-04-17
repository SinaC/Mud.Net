﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace Mud.POC.TestLua
{
    //https://www.gamedev.net/articles/programming/general-and-gameplay-programming/using-lua-with-c-r2275/

    [AttributeUsage(AttributeTargets.Method)]
    public class AttrLuaFunc : Attribute
    {
        private String FunctionName;
        private String FunctionDoc;
        private String[] FunctionParameters = null;

        public AttrLuaFunc(String strFuncName, String strFuncDoc, params String[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParamDocs;
        }

        public AttrLuaFunc(String strFuncName, String strFuncDoc)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        public String[] getFuncParams()
        {
            return FunctionParameters;
        }
    }

    public class LuaFuncDescriptor
    {
        private String FunctionName;
        private String FunctionDoc;
        private ArrayList FunctionParameters;
        private ArrayList FunctionParamDocs;
        private String FunctionDocString;

        public LuaFuncDescriptor(String strFuncName, String strFuncDoc, ArrayList strParams, ArrayList strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParams;
            FunctionParamDocs = strParamDocs;

            String strFuncHeader = strFuncName + "(%params%) - " + strFuncDoc;
            String strFuncBody = "\n\n";
            String strFuncParams = "";

            Boolean bFirst = true;

            for (int i = 0; i < strParams.Count; i++)
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

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
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

        public String getFuncHeader()
        {
            if (FunctionDocString.IndexOf("\n") == -1)
                return FunctionDocString;

            return FunctionDocString.Substring(0, FunctionDocString.IndexOf("\n"));
        }

        public String getFuncFullDoc()
        {
            return FunctionDocString;
        }
    }

    public class TestRegisterFunctionEntity
    {
        public Guid Id { get; }

        public TestRegisterFunctionEntity()
        {
            Id = Guid.NewGuid();
            Console.WriteLine($"Id: {Id}");
        }
    }

    public class TestRegisterFunction
    {
        public Lua pLuaVM { get; }
        public Hashtable pLuaFuncs { get; }


        public TestRegisterFunction()
        {
            pLuaVM = new Lua();
            pLuaFuncs = new Hashtable();
        }

        public void Test()
        {
            Console.WriteLine($"Testing with Id {Id}");

            registerLuaFunctions(this);

            pLuaVM.DoString(@"
quit();
pouet(5, 'test');
");
            var _g = pLuaVM["_G"] as LuaTable;
            var quit = _g["quit"] as LuaFunction;
        }

        public void registerLuaFunctions(Object pTarget)
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
                        String strFName = pAttr.getFuncName();
                        String strFDoc = pAttr.getFuncDoc();
                        String[] pPrmDocs = pAttr.getFuncParams();

                        // Now get the expected parameters from the MethodInfo object
                        ParameterInfo[] pPrmInfo = mInfo.GetParameters();

                        // If they don't match, someone forgot to add some documentation to the
                        // attribute, complain and go to the next method
                        if (pPrmInfo.Length > 0 && pPrmInfo.Length != pPrmDocs.Length)
                        {
                            Console.WriteLine("Function " + mInfo.Name + " (exported as " +
                                              strFName + ") argument number mismatch. Declared " +
                                              pPrmDocs.Length + " but requires " +
                                              pPrmInfo.Length + ".");
                            break;
                        }

                        // Build a parameter <-> parameter doc hashtable
                        for (int i = 0; i < pPrmInfo.Length; i++)
                        {
                            pParams.Add(pPrmInfo[i].Name, pPrmDocs[i]);
                        }

                        // TODO: fix
                        //// Get a new function descriptor from this information
                        //LuaFuncDescriptor pDesc = new LuaFuncDescriptor(strFName, strFDoc, pParams);

                        //// Add it to the global hashtable
                        //pLuaFuncs.Add(strFName, pDesc);

                        // And tell the VM to register it.
                        pLuaVM.RegisterFunction(strFName, new TestRegisterFunction(), mInfo);
                        Console.WriteLine($"RegisterFunction:{strFName}");
                    }
                }
            }
        }

        [AttrLuaFunc("quit", "Exit the program.")]
        public void quit()
        {
            Console.WriteLine($"quit [{Id}]");
        }

        [AttrLuaFunc("pouet", "Pouet.", "param1 is the first param", "param2 is the second param")]
        public void pouet(int param1, string param2)
        {
            Console.WriteLine($"pouet  [{Id}]:{param1} {param2}");
        }

        [AttrLuaFunc("help", "List available commands.")]
        public void help()
        {
            Console.WriteLine("Available commands: ");
            Console.WriteLine();

            IDictionaryEnumerator Funcs = pLuaFuncs.GetEnumerator();
            while (Funcs.MoveNext())
            {
                Console.WriteLine(((LuaFuncDescriptor)Funcs.Value).getFuncHeader());
            }
        }

        [AttrLuaFunc("helpcmd", "Show help for a given command", "Command to get help of.")]
        public void help(String strCmd)
        {
            if (!pLuaFuncs.ContainsKey(strCmd))
            {
                Console.WriteLine("No such function or package: " + strCmd);
                return;
            }

            LuaFuncDescriptor pDesc = (LuaFuncDescriptor)pLuaFuncs[strCmd];
            Console.WriteLine(pDesc.getFuncFullDoc());
        }
    }
}
