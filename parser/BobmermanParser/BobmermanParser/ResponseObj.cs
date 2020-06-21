using System;
using System.Collections.Generic;
using System.Text;

namespace BobmermanParser
{
    public class ResponseObj
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int Score { get; set; }
		public object Day { get; set; }
		public DateTime Time { get; set; }
		public string Server { get; set; }
		public bool Winner { get; set; }

        public override string ToString()
        {
            return $"{Name}\t{Time}\t{Score}\t\t\t";
        }
    }
}
