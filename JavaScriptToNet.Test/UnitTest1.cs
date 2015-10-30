using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JavaScriptToNet.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName();
            am.Name = "TestAsm";
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Save);
            ModuleBuilder mb = ab.DefineDynamicModule("aa.testmod", "TestAsm.exe");
            TypeBuilder tb = mb.DefineType("aa.mytype", TypeAttributes.Public);
            MethodBuilder metb = tb.DefineMethod("hi", MethodAttributes.Public |
            MethodAttributes.Static, null, null);
            ab.SetEntryPoint(metb);

            ILGenerator il = metb.GetILGenerator();
            il.EmitWriteLine("Hello World");
            il.Emit(OpCodes.Ret);
            tb.CreateType();
            ab.Save("TestAsm.exe");
        }

        [TestMethod]
        public void TestMethod2()
        {
              Class1.emit(@"js");  
        }
        //   [TestMethod]
        //public void TestMethodArgs()
        //{
        //    StreamReader file = new StreamReader(@"js\exampleArgs.js");
        //    Class1.emit("TestMethodArgs", file.ReadToEnd());
        //}
    }
}
