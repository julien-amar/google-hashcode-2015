using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualificationTask.Model
{
    public class UnavailableCell
    {
        public int Row { get; set; }
        public int Slot { get; set; }

        public UnavailableCell(int row, int slot)
        {
            Row = row;
            Slot = slot;
        }
    }
}
