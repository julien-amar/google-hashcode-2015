using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualificationTask
{
    public class IndexGenerator
    {
        private static int _index = 0;

        public static void SetIndex(int index)
        {
            _index = index;
        }

        public static int GetIndex()
        {
            return _index++;
        }
    }
}
