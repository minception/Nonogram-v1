using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonogram
{
    class Column : Values
    {
        public Column(Grid grid, int no) : base(grid, no)
        {
        }

        public Column(int[] values, Grid grid, int no) : base(values, grid, no)
        {
        }

        protected override Status StatusAt(int index)
        {
            return Grid[No, index];
        }

        protected override void SetStatusAt(int index, Status status)
        {
            Grid[No, index] = status;
        }

        protected override int RespectibleSize()
        {
            return Grid.SizeY;
        }
    }
}
