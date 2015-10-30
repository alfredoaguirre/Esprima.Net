using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        public static void emit(string directory)
        {
            directory = directory.Replace(@"\", ".");
            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName();
            am.Name = directory;
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Save);
            ModuleBuilder mb = ab.DefineDynamicModule(directory, directory +".dll");

            string[] filePaths = Directory.GetFiles(directory);
            foreach (var source in filePaths.Where(x => x.EndsWith(".js")))
            {
                var typeBuilder = GetTypeBuilder(source, mb, ab);
            }
            ab.Save(directory + ".dll");
        }

        public static TypeBuilder GetTypeBuilder(string fileName, ModuleBuilder mb, AssemblyBuilder ab)
        {
            StreamReader file = new StreamReader(fileName);
            TypeBuilder tb = mb.DefineType( fileName.Substring(0,fileName.Length-3).Replace(@"\", "."), TypeAttributes.Public);
            var code = file.ReadToEnd();
            var esprima = new Esprima.NET.Esprima();
            var node = esprima.parse(code, new Options());

            foreach (var fcuntionDef in node.body.Where(x => x.type == "FunctionDeclaration"))
            {
                var argumentNames = fcuntionDef.@params.Select(x => typeof(Object)).ToArray();
                MethodBuilder metb = tb.DefineMethod(fcuntionDef.id.name, MethodAttributes.Public | MethodAttributes.Static, null, argumentNames);
                ab.SetEntryPoint(metb);
                ILGenerator il = metb.GetILGenerator();
                il.EmitWriteLine("Hello World");
                il.Emit(OpCodes.Ret);
                il.Emit(OpCodes.Ret);
            }
            tb.CreateType();
            return tb;
        }
    }
}
