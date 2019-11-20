using System;
using System.Collections.Generic;

namespace Nonogram
{
    class Grid
    {
        public readonly int SizeX, SizeY;
        private List<Row> _rows;
        private int _verticalMax;
        private List<Column> _columns;
        private int _horizontalMax;
        private List<Status> _grid;
        private bool _solved;

        /// <summary>
        /// Create grid with default values
        /// </summary>
        /// <param name="sizeX">Width of the grid</param>
        /// <param name="sizeY">Height of the grid</param>
        public Grid(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;

            _columns = new List<Column>();
            _verticalMax = 0;
            _rows = new List<Row>();
            _horizontalMax = 0;
            _grid = new List<Status>();
            for (int i = 0; i < sizeX; i++)
            {
                Column column = new Column(this, i);
                _columns.Add(column);
                for (int j = 0; j < sizeY; j++)
                {
                    _grid.Add(Status.Unknown);
                }
            }
            for (int i = 0; i < sizeY; i++)
            {
                Row row = new Row(this, i);
                _rows.Add(row);
            }
            _solved = false;
        }

        /// <summary>
        /// Create new grid from file
        /// </summary>
        /// <param name="fileName">Name of file from which to create grid</param>
        public Grid(string fileName)
        {
            _columns = new List<Column>();
            _verticalMax = 0;
            _rows = new List<Row>();
            _horizontalMax = 0;
            _grid = new List<Status>();

            System.IO.StreamReader reader = new System.IO.StreamReader(fileName);
            string size = reader.ReadLine();
            string verticalValues = reader.ReadLine();
            string horizontalValues = reader.ReadLine();
            reader.Close();

            string[] strings;
            int[] ints;

            //parsing sizes
            strings = size.Split('x');
            ints = Array.ConvertAll(strings, int.Parse);

            SizeX = ints[0];
            SizeY = ints[1];

            //parsing vertical values
            if (verticalValues != null) strings = verticalValues.Split('|');
            int no = 0;
            foreach (string item in strings)
            {
                string[] rows = item.Split('*');
                ints = Array.ConvertAll(rows, int.Parse);
                Column column = new Column(ints, this, no);
                _columns.Add(column);
                if (column.Amount > VerticalMax) _verticalMax = column.Amount;
                no++;
            }


            //parsing horizontal values
            if (horizontalValues != null) strings = horizontalValues.Split('|');
            no = 0;
            foreach (string item in strings)
            {
                string[] rows = item.Split('*');
                ints = Array.ConvertAll(rows, int.Parse);
                Row row = new Row(ints, this, no);
                _rows.Add(row);
                if (row.Amount > HorizontalMax) _horizontalMax = row.Amount;
                no++;
            }

            //generate empty grid
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    _grid.Add(Status.Unknown);
                }
            }

            _solved = false;

        }

        public Status this[int x, int y]
        {
            get
            {
                if (x < SizeX && x >= 0 && y < SizeY && y >= 0)
                {
                    return _grid[(y*SizeX) + x];
                }
                else
                {
                    throw new Exception("Out of bounds");
                }
            }
            set
            {
                if (x < SizeX && x >= 0 && y < SizeY && y >= 0)
                {
                    _grid[(y*SizeX) + x] = value;
                }
                else
                {
                    throw new Exception("Out of bounds");
                }

            }
        }

        /// <summary>
        /// Maximum number of values in all vertical rows
        /// </summary>
        public int VerticalMax
        {
            get { return _verticalMax; }
        }

        /// <summary>
        /// Maximum number of values in all horizontal rows
        /// </summary>
        public int HorizontalMax
        {
            get { return _horizontalMax; }
        }

        /// <summary>
        /// Returns amount of values of specified column
        /// </summary>
        /// <param name="index">Index of column</param>
        /// <returns>Amount of values in column</returns>

        public int ColumnAmount(int index)
        {
            if (index >= 0 && index < SizeX)
            {
                return (_columns[index].Amount);
            }
            else return 0;
        }

        /// <summary>
        /// Returns amount of values of specified row
        /// </summary>
        /// <param name="index">Index of row</param>
        /// <returns>Amount of values in row</returns>
        public int RowAmount(int index)
        {
            if (index >= 0 && index < SizeY)
            {
                return (_rows[index].Amount);
            }
            else return 0;
        }

        public int VerticalRowValue(int rowNumber, int index)
        {
            return (_columns[rowNumber][index]);
        }

        public int HorizontalRowValue(int rowNumber, int index)
        {
            return (_rows[rowNumber][index]);
        }

        /// <summary>
        /// Solves puzzle
        /// </summary>
        /// <returns>Returns true if solved successfully, false if error occured</returns>
        public bool Solve()
        {
            bool change;
            do
            {
                change = false;
                bool? result;
                foreach (Column column in _columns)
                {
                    if (!column.Finished())
                    {
                        if (!column.Empty)
                        {
                            result = column.Solve();
                            if (result == null)
                            {
                                return false;
                            }
                            if ((bool) result)
                            {
                                change = true;
                            }
                        }
                        else
                        {
                            result = column.Clean();
                            if (result == null)
                            {
                                return false;
                            }
                            if ((bool) result)
                            {
                                change = true;
                            }
                        }
                    }
                }
                foreach (Row row in _rows)
                {
                    if (!row.Finished())
                    {
                        result = row.Solve();
                        if (result == null)
                        {
                            return false;
                        }
                        if ((bool) result)
                        {
                            change = true;
                        }
                    }
                }
            } while (change);
            _solved = true;
            return true;
        }

        public bool Solved
        {
            get { return _solved; }
        }

        public void SaveToFile(string fileName)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName);
            string size = SizeX.ToString() + 'x' + SizeY.ToString();
            string columns = "";
            foreach (Column column in _columns)
            {
                for (int i = 0; i < column.Amount; i++)
                {
                    columns += column[i].ToString() + '*';
                }
                //remove the extra star
                if (column.Amount > 0)
                {
                    columns = columns.Remove(columns.Length - 1);
                }
                else
                {
                    columns += '0';
                }
                columns += '|';
            }
            //remove the extra vertical bar
            columns = columns.Remove(columns.Length - 1);

            string rows = "";
            foreach (Row row in _rows)
            {
                for (int i = 0; i < row.Amount; i++)
                {
                    rows += row[i].ToString() + '*';
                }
                //remove the extra star
                if (row.Amount > 0)
                {
                    rows = rows.Remove(rows.Length - 1);
                }
                else
                {
                    rows += 0;
                }
                rows += '|';
            }
            //remove the extra vertical bar
            rows = rows.Remove(rows.Length - 1);

            writer.WriteLine(size);
            writer.WriteLine(columns);
            writer.WriteLine(rows);
            writer.Close();

        }

        public void IncVertical(int rowNumber, int index)
        {
            if (rowNumber < SizeX && rowNumber >= 0)
            {
                _columns[rowNumber].Inc(index);
                if (_columns[rowNumber].MinReqArea > SizeY)
                {
                    if (index < 0)
                    {
                        index = 0;
                    }
                    _columns[rowNumber].Dec(index);
                }
                if (_columns[rowNumber].Amount > VerticalMax)
                {
                    _verticalMax = _columns[rowNumber].Amount;
                }
            }
        }

        public void IncHorizontal(int rowNumber, int index)
        {
            if (rowNumber < SizeY && rowNumber >= 0)
            {
                _rows[rowNumber].Inc(index);
                if (_rows[rowNumber].MinReqArea > SizeX)
                {
                    if (index < 0)
                    {
                        index = 0;
                    }
                    _rows[rowNumber].Dec(index);
                }
                if (_rows[rowNumber].Amount > HorizontalMax)
                {
                    _horizontalMax = _rows[rowNumber].Amount;
                }
            }
        }

        public void DecVertical(int rowNumber, int index)
        {
            if (rowNumber < SizeX && rowNumber >= 0)
            {
                _columns[rowNumber].Dec(index);
                if (_columns[rowNumber].Amount == VerticalMax - 1)
                {
                    _verticalMax = _columns[rowNumber].Amount;
                    for (int i = 0; i < SizeX; i++)
                    {
                        if (_columns[i].Amount > VerticalMax)
                        {
                            _verticalMax = _columns[i].Amount;
                        }
                    }
                }
            }
        }

        public void DecHorizontal(int rowNumber, int index)
        {
            if (rowNumber < SizeY && rowNumber >= 0)
            {
                _rows[rowNumber].Dec(index);
                if (_rows[rowNumber].Amount == HorizontalMax - 1)
                {
                    _horizontalMax = _rows[rowNumber].Amount;
                    for (int i = 0; i < SizeY; i++)
                    {
                        if (_rows[i].Amount > HorizontalMax)
                        {
                            _horizontalMax = _rows[i].Amount;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Clears solving progress
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < SizeX*SizeY; i++) 
            {
                _grid[i] = Status.Unknown;
            }
            for (int i = 0; i < SizeX; i++)
            {
                _columns[i].Clear();
            }
            for (int i = 0; i < SizeY; i++)
            {
                _rows[i].Clear();
            }
        }

    public void SolveOneRow(int lineNumber)
        {
            _rows[lineNumber].Solve();
        }

        public void SolveOneColumn(int lineNumber)
        {
            _columns[lineNumber].Solve();
        }
    }
}
