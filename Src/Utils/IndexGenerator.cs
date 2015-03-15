using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualificationTask
{
    public class IndexGenerator
    {
        private int _index = 0;

        public void SetIndex(int index)
        {
            _index = index;
        }

        public int GetIndex()
        {
            return _index++;
        }
    }
}
