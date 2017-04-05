using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace JavaScriptToNet.Test.utils
{
    class JsonHelper
    {
        public static void LoadFile()
        {
            string text;
            using (var streamReader = new StreamReader(@"jQuery\core.json", Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }
           //var  proxy =  JObject.Parse(text);
           var jsonStructures = JsonConvert.DeserializeObject<jsonStructure>(text);
        }

    }
}
