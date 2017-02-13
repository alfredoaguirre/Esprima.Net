using System;

namespace JavaScriptToNet.Test
{
	public class jsonAPIObject
	{
		public string name;
		public string type;
		public String[] args;
		public String[] properties;
		public jsonAPIMethod[] methods;
	}
    
	public class jsonAPIMethod 
	{
		public string name;
		public String[] args;
	}
	
    public class jsonStructure
    {
		public jsonAPIObject[] apiNames;
	}
}
