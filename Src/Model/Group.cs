using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualificationTask.Model
{
    public class Group
    {
        public List<Server> Servers { get; set; }

        public Group()
        {
            Servers = new List<Server>();
        }
        public int TotalCapacity
        {
            get { return Servers.Sum(x => x.Capacity); }
        }
    }
}
