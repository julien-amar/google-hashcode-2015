using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualificationTask.Model
{
    public class Server
    {
        public enum StateEnum
        {
            Unused,
            Used,
            NeedAnalyze,
            Ignore
        }

        public Server(int index, int size, int capacity)
        {
            Index = index;
            Size = size;
            Capacity = capacity;
            State = StateEnum.NeedAnalyze;
        }

        public int Index { get; set; }
        public int Size { get; set; }
        public int Capacity { get; set; }

        public int Row { get; set; }
        public int Slot { get; set; }

        public Group Group { get; set; }

        public StateEnum State { get; set; }

        public float Score
        {
            get { return Capacity/(float)Size; }
        }
    }
}
