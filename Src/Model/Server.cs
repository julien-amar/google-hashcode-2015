using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualificationTask.Model
{
    public class Server
    {
        public Server(int index, int slots, int capacity)
        {
            Index = index;
            Slots = slots;
            Capacity = capacity;
        }

        public int Index { get; set; }
        public int Slots { get; set; }
        public int Capacity { get; set; }
    }
}
