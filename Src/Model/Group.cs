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
        public int Index { get; set; }

        public Group(int index)
        {
            Index = index;
            Servers = new List<Server>();
        }

        public int TotalCapacity
        {
            get { return Servers.Sum(x => x.Capacity); }
        }

        public void Dump()
        {
            Console.WriteLine("Group {0}:", Index);

            foreach (var server in Servers)
                Console.WriteLine("\t[{0}] (cap: {1}, size: {2}, score: {3})", server.Index, server.Capacity, server.Size, server.Score);

            Console.WriteLine();
        }
    }
}
