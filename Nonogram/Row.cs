namespace Nonogram
{
    class Row : Values
    {
        public Row(Grid grid, int no) : base(grid, no)
        {
        }
        public Row(int[] values, Grid grid, int no) : base(values, grid, no)
        {
        }
        protected override int RespectibleSize()
        {
            return Grid.SizeX;
        }

        protected override void SetStatusAt(int index, Status status)
        {
            Grid[index, No] = status;
        }

        protected override Status StatusAt(int index)
        {
            return Grid[index, No];
        }

    }
}
