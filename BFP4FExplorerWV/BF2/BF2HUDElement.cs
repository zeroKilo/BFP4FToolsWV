using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFP4FExplorerWV
{
    public class BF2HUDElement
    {
        public string name;
        public BF2HUDElement parent;
        public List<string> parameter;

        public BF2HUDElement(string _name, BF2HUDElement _parent)
        {
            name = _name;
            parent = _parent;
            parameter = new List<string>();
        }
    }
}
