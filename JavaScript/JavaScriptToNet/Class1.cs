using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Esprima.NET;

namespace JavaScriptToNet
{
    public static class Class1
    {
        public static void  emit(string filename , string code)
        {
              var esprima = new Esprima.NET.Esprima();
            var node = esprima.parse(code, new Options());

            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName();
            am.Name = filename;
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Save);
            ModuleBuilder mb = ab.DefineDynamicModule(filename + "." + filename, "filename.dll");
            TypeBuilder tb = mb.DefineType(filename + "." + filename, TypeAttributes.Public);

            foreach (var fcuntionDef in node.body.Where(x => x.type == "FunctionDeclaration"))
            {
                var argumentNames = fcuntionDef.@params.Select(x => typeof(Object)).ToArray();
                MethodBuilder metb = tb.DefineMethod(fcuntionDef.id.name, MethodAttributes.Public | MethodAttributes.Static, null, argumentNames);
                ab.SetEntryPoint(metb); 
                ILGenerator il = metb.GetILGenerator();
                il.EmitWriteLine("Hello World");
                il.Emit(OpCodes.Ret);
             
            }
               tb.CreateType();
           

           
            ab.Save("TestAsm.exe");

        }
    }
}
