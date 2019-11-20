using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nonogram
{
    public partial class FormNonogram : Form
    {
        private int _cellSize;
        private int _verticalOffset;
        private int _horizontalOffset;

        private Grid _grid;

        public FormNonogram()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formNew = new FormNew();
            var result = formNew.ShowDialog();
            if (result == DialogResult.OK)
            {
                _grid = new Grid(formNew.sizeX, formNew.sizeY);
                Draw(_grid);
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (_grid != null)
            {
                var clickedCellX = (pictureBox.PointToClient(Cursor.Position).X - _horizontalOffset)/_cellSize;
                var clickedCellY = (pictureBox.PointToClient(Cursor.Position).Y - _verticalOffset)/_cellSize;
                if (e.Button == MouseButtons.Left)
                {
                    //clicked on vertical row
                    if ((clickedCellY < _grid.VerticalMax + 1) && (clickedCellX > _grid.HorizontalMax))
                    {
                        var rowNumber = clickedCellX - _grid.HorizontalMax - 1;
                        var index = clickedCellY - (_grid.VerticalMax - _grid.ColumnAmount(rowNumber)) - 1;
                        _grid.IncVertical(rowNumber, index);
                    }
                    //clicked on horizontal row
                    if ((clickedCellX < _grid.HorizontalMax + 1) && (clickedCellY > _grid.VerticalMax))
                    {
                        var rowNumber = clickedCellY - _grid.VerticalMax - 1;
                        var index = clickedCellX - (_grid.HorizontalMax - _grid.RowAmount(rowNumber)) - 1;
                        _grid.IncHorizontal(rowNumber, index);
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    //clicked on vertical row
                    if ((clickedCellY < _grid.VerticalMax + 1) && (clickedCellX > _grid.HorizontalMax))
                    {
                        var rowNumber = clickedCellX - _grid.HorizontalMax - 1;
                        var index = clickedCellY - (_grid.VerticalMax - _grid.ColumnAmount(rowNumber)) - 1;
                        _grid.DecVertical(rowNumber, index);
                    }
                    //clicked on horizontal row
                    if ((clickedCellX < _grid.HorizontalMax + 1) && (clickedCellY > _grid.VerticalMax))
                    {
                        var rowNumber = clickedCellY - _grid.VerticalMax - 1;
                        var index = clickedCellX - (_grid.HorizontalMax - _grid.RowAmount(rowNumber)) - 1;
                        _grid.DecHorizontal(rowNumber, index);
                    }
                }
                Draw(_grid);
            }
        }

        private void solveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_grid != null)
            {
                var solved = _grid.Solve();
                if (solved)
                    Draw(_grid);
                else
                {
                    MessageBox.Show("This puzzle doesn't have a solution");
                    _grid.Clear();
                }
            }
            else
            {
                MessageBox.Show("No puzzle to solve");
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_grid != null)
            {
                _grid = new Grid(_grid.SizeX, _grid.SizeY);
                Draw(_grid);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Griddlers file (*.grid)|*.grid";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _grid = new Grid(openFileDialog.FileName);
                Draw(_grid);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_grid != null)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Griddlers file (*.grid)|*.grid";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    _grid.SaveToFile(saveFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("No puzzle to save");
            }
        }

        private void Draw(Grid toDraw)
        {
            //calculating size of grid cell
            if ((toDraw.SizeX + toDraw.HorizontalMax)/((float) toDraw.SizeY + toDraw.VerticalMax) >
                pictureBox.Width/(float) pictureBox.Height)
                _cellSize = (pictureBox.Width - 1)/(toDraw.SizeX + toDraw.HorizontalMax + 1);
            else
                _cellSize = (pictureBox.Height - 1)/(toDraw.SizeY + toDraw.VerticalMax + 1);
            _horizontalOffset = (pictureBox.Width - _cellSize * (toDraw.SizeX + toDraw.HorizontalMax + 1)) / 2;
            _verticalOffset = (pictureBox.Height - _cellSize * (toDraw.SizeY + toDraw.VerticalMax + 1)) / 2;
            //inicializing drawing board
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            var gr = Graphics.FromImage(pictureBox.Image);
            Pen linePen;
            var thinLinePen = new Pen(Color.Black, 1);
            var thickLinePen = new Pen(Color.Black, 2);
            var filledSquare = Brushes.Black;
            var emptySquare = Brushes.White;
            //drawing grid
            for (var i = 0; i <= toDraw.SizeX + toDraw.HorizontalMax + 1; i++)
            {
                if ((i >= toDraw.HorizontalMax + 1) && (i < toDraw.SizeX + toDraw.HorizontalMax + 1) &&
                    ((i - toDraw.HorizontalMax - 1)%5 == 0))
                {
                    linePen = thickLinePen;
                }
                else
                    linePen = thinLinePen;
                int position = _horizontalOffset + _cellSize * i;
                int top = _verticalOffset;
                int bottom = _verticalOffset + _cellSize * (toDraw.SizeY + toDraw.VerticalMax + 1);
                gr.DrawLine(linePen, position, top, position, bottom);
            }
            for (var i = 0; i <= toDraw.SizeY + toDraw.VerticalMax + 1; i++)
            {
                if ((i >= toDraw.VerticalMax + 1) && (i < toDraw.SizeY + toDraw.VerticalMax + 1) &&
                    ((i - toDraw.VerticalMax - 1)%5 == 0))
                    linePen = thickLinePen;
                else
                    linePen = thinLinePen;

                int position = _verticalOffset + _cellSize * i;
                int left = _horizontalOffset;
                int right = _horizontalOffset + _cellSize * (toDraw.SizeX + toDraw.HorizontalMax + 1);
                gr.DrawLine(linePen, left, position, right, position);
            }
            //drawing numbers 
            for (var i = 0; i < toDraw.SizeX; i++)
            {
                var positionX = _horizontalOffset + _cellSize*(toDraw.HorizontalMax + 1) + _cellSize*i;
                var positionY = _verticalOffset + _cellSize*(toDraw.VerticalMax - toDraw.ColumnAmount(i));
                gr.DrawString("0", DefaultFont, Brushes.Black, positionX, positionY);
                for (var j = 0; j < toDraw.ColumnAmount(i); j++)
                {
                    positionY = _verticalOffset + _cellSize*(toDraw.VerticalMax - toDraw.ColumnAmount(i) + j + 1);
                    gr.DrawString(toDraw.VerticalRowValue(i, j).ToString(), DefaultFont, Brushes.Black, positionX,
                        positionY);
                }
            }
            for (var i = 0; i < toDraw.SizeY; i++)
            {
                var positionY = _verticalOffset + _cellSize*(toDraw.VerticalMax + 1) + _cellSize*i;
                var positionX = _horizontalOffset + _cellSize*(toDraw.HorizontalMax - toDraw.RowAmount(i));
                gr.DrawString("0", DefaultFont, Brushes.Black, positionX, positionY);
                for (var j = 0; j < toDraw.RowAmount(i); j++)
                {
                    positionX = _horizontalOffset + _cellSize*(toDraw.HorizontalMax - toDraw.RowAmount(i) + j + 1);
                    gr.DrawString(toDraw.HorizontalRowValue(i, j).ToString(), DefaultFont, Brushes.Black, positionX,
                        positionY);
                }
            }
            //drawing solution
            //if (toDraw.Solved)
            //{
            for (var i = 0; i < toDraw.SizeX; i++)
                for (var j = 0; j < toDraw.SizeY; j++)
                {
                    var posX = _horizontalOffset + _cellSize*(toDraw.HorizontalMax + i + 1);
                    var posY = _verticalOffset + _cellSize*(toDraw.VerticalMax + j + 1);
                    if (toDraw[i, j] == Status.Filled)
                        gr.FillRectangle(filledSquare, posX, posY, _cellSize, _cellSize);
                    else if (toDraw[i, j] == Status.Empty)
                        gr.FillRectangle(emptySquare, posX, posY, _cellSize, _cellSize);
                }
            //}
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new FormAbout();
            form.Show();
        }


        private void FormNonogram_Resize(object sender, EventArgs e)
        {
            if (_grid != null && pictureBox.Width > 0 && pictureBox.Height > 0) 
            {
                Draw(_grid);
            }
        }
    }
}