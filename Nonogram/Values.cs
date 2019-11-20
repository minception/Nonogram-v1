using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    internal abstract class Values
    {
        //constants for this line
        protected readonly Grid Grid;
        protected readonly int No;

        //variables used while solving
        private List<bool> _finished;
        private bool _completelyFinished;
        
        //variables set by user
        private readonly List<int> _values;

        /// <summary>
        ///     Creates a new line withe default presets
        /// </summary>
        /// <param name="grid">Grid respective to this line</param>
        /// <param name="no">Line number</param>
        public Values(Grid grid, int no)
        {
            _values = new List<int>();
            _finished = new List<bool>();
            MinReqArea = 0;

            Grid = grid;
            No = no;
            FirstNotFinished = 0;
            LastNotFinished = -1;
            UnfinishedRangeBeg = 0;
            UnfinishedRangeEnd = RespectibleSize() - 1;
            _completelyFinished = false;
        }

        /// <summary>
        ///     Creates new line with values already set
        /// </summary>
        /// <param name="values">Array of integers representing values in the line</param>
        /// <param name="grid">Grid respective to this line</param>
        /// <param name="no">Line number</param>
        public Values(int[] values, Grid grid, int no)
        {
            if (values[0] != 0)
                _values = new List<int>(values);
            else
                _values = new List<int>();
            _finished = new List<bool>();
            for (var i = 0; i < Amount; i++)
                _finished.Add(false);
            MinReqArea = _values.Sum() + Amount - 1;

            Grid = grid;
            No = no;
            FirstNotFinished = 0;
            LastNotFinished = Amount - 1;
            UnfinishedRangeBeg = 0;
            UnfinishedRangeEnd = RespectibleSize() - 1;
            _completelyFinished = false;
        }

        /// <summary>
        ///     Returns a value at index
        /// </summary>
        /// <param name="index">Index of value to return</param>
        /// <returns></returns>
        public int this[int index]
        {
            get { return _values[index]; }
        }

        /// <summary>
        ///     Amount of values in row
        /// </summary>
        public int Amount
        {
            get { return _values.Count; }
        }

        /// <summary>
        ///     Minimal area required to put values in
        /// </summary>
        public int MinReqArea { get; private set; }

        /// <summary>
        ///     First value from the start that hasn't been finished
        /// </summary>
        private int FirstNotFinished { get; set; }

        /// <summary>
        ///     First value from an end that hasn't been finished
        /// </summary>
        public int LastNotFinished { get; set; }

        /// <summary>
        ///     returns true if row contains no values
        /// </summary>
        public bool Empty
        {
            get { return Amount == 0; }
        }

        public int UnfinishedRangeBeg { get; set; }

        public int UnfinishedRangeEnd { get; set; }

        /// <summary>
        ///     Increases value at index
        /// </summary>
        /// <param name="index">Index of value to increase</param>
        public void Inc(int index)
        {
            if ((Amount > index) && (index >= 0))
            {
                _values[index]++;
                MinReqArea++;
            }
            else if (index < 0)
            {
                //insert on a first position
                _values.Insert(0, 1);
                LastNotFinished = Amount - 1;
                _finished.Add(false);
                MinReqArea++;
                if (_values.Count > 1)
                    MinReqArea++;
            }
        }

        /// <summary>
        ///     Decreases value at index
        /// </summary>
        /// <param name="index">Index of value to decrease</param>
        public void Dec(int index)
        {
            if ((Amount > index) && (index >= 0))
            {
                MinReqArea--;
                if (--_values[index] == 0)
                {
                    _values.RemoveAt(index);
                    LastNotFinished = Amount - 1;
                    _finished.RemoveAt(index);
                    if (Amount > 0)
                        MinReqArea--;
                }
            }
        }

        public bool? Solve()
        {
            bool? change = false;

            //solving row #1
            //putting from begginning
            var position = UnfinishedRangeBeg;
            var putFromBeg = new int[RespectibleSize()];
            var potentialPutFromEnd = UnfinishedRangeEnd;
            //for every value in a line
            for (var i = FirstNotFinished; i <= LastNotFinished; i++)
            {
                var isPut = false;
                while (!isPut)
                {
                    //for every step in a value
                    for (var j = 0; j < this[i]; j++)
                        if (position <= UnfinishedRangeEnd)
                        {
                            if ((StatusAt(position) == Status.Unknown) || (StatusAt(position) == Status.Filled))
                            {
                                isPut = true;
                                if ((StatusAt(position) == Status.Filled) && (i == FirstNotFinished) &&
                                    (potentialPutFromEnd > position))
                                    potentialPutFromEnd = position;
                                position++;
                            }
                            else if (StatusAt(position) == Status.Empty)
                            {
                                isPut = false;
                                if (i == FirstNotFinished)
                                {
                                    potentialPutFromEnd = UnfinishedRangeEnd;
                                    int clearPosition = position;
                                    while (clearPosition >= FirstNotFinished)
                                    {
                                        if (StatusAt(clearPosition) == Status.Unknown)
                                        {
                                            SetStatusAt(clearPosition,Status.Empty);
                                            change = true;
                                        }
                                        clearPosition--;
                                    }
                                }
                                position++;
                                break;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    if (isPut)
                    {
                        while ((position < UnfinishedRangeEnd) && (StatusAt(position) == Status.Filled))
                        {
                            if (i == FirstNotFinished)
                            {
                                if (position < potentialPutFromEnd)
                                {
                                    potentialPutFromEnd = position;
                                }
                                if (position - this[i] >= UnfinishedRangeBeg && StatusAt(position - this[i]) == Status.Unknown)
                                {
                                    SetStatusAt(position - this[i], Status.Empty);
                                    change = true;
                                }
                            }
                            position++;
                        }
                        putFromBeg[i] = position - 1;
                    }
                }
                //adding space between values
                position++;
            }
            //putting from an end
            position = UnfinishedRangeEnd;
            var putFromEnd = new int[RespectibleSize()];
            var potentialPutFromBeg = 0;
            //for every value in a line
            for (var i = LastNotFinished; i >= FirstNotFinished; i--)
            {
                var isPut = false;
                while (!isPut)
                {
                    //for every step in a value
                    for (var j = 0; j < this[i]; j++)
                        if (position >= UnfinishedRangeBeg)
                        {
                            if ((StatusAt(position) == Status.Unknown) || (StatusAt(position) == Status.Filled))
                            {
                                isPut = true;
                                if ((StatusAt(position) == Status.Filled) && (i == LastNotFinished) &&
                                    (potentialPutFromBeg < position))
                                    potentialPutFromBeg = position;
                                position--;
                            }
                            else if (StatusAt(position) == Status.Empty)
                            {
                                isPut = false;
                                if (i == LastNotFinished)
                                {
                                    potentialPutFromBeg = UnfinishedRangeBeg;
                                    int clearPosition = position;
                                    while (clearPosition <= LastNotFinished)
                                    {
                                        if (StatusAt(clearPosition) == Status.Unknown)
                                        {
                                            SetStatusAt(clearPosition, Status.Empty);
                                            change = true;
                                        }
                                        clearPosition++;
                                    }
                                }
                                position--;
                                break;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    if (isPut)
                    {
                        while ((position > UnfinishedRangeBeg) && (StatusAt(position) == Status.Filled))
                        {
                            if (i == LastNotFinished && position > potentialPutFromBeg)
                            {
                                if (position > potentialPutFromBeg)
                                {
                                    potentialPutFromBeg = position;
                                }
                                if (position + this[i] <= UnfinishedRangeEnd &&
                                    StatusAt(position + this[i]) == Status.Unknown)
                                {
                                    SetStatusAt(position + this[i], Status.Empty);
                                    change = true;
                                }
                                
                            }
                            position--;
                        }
                        if ((i == FirstNotFinished) && (potentialPutFromEnd < position + 1))
                            putFromEnd[i] = potentialPutFromEnd;
                        else
                            putFromEnd[i] = position + 1;
                        if ((i == LastNotFinished) && (potentialPutFromBeg > putFromBeg[i]))
                            putFromBeg[i] = potentialPutFromBeg;
                    }
                }
                //adding space between values
                position--;
            }
            //coloring
            for (var i = FirstNotFinished; i <= LastNotFinished; i++)
                if (!IsFinished(i))
                    if (putFromBeg[i] >= putFromEnd[i])
                    {
                        for (var j = putFromEnd[i]; j <= putFromBeg[i]; j++)
                            if (StatusAt(j) == Status.Unknown)
                            {
                                SetStatusAt(j, Status.Filled);
                                change = true;
                            }
                        //if colored whole value put empty spaces around it

                        if (putFromBeg[i] - putFromEnd[i] + 1 == this[i])
                        {
                            SetFinished(i);
                            if ((putFromBeg[i] < UnfinishedRangeEnd) && (StatusAt(putFromBeg[i] + 1) == Status.Unknown))
                            {
                                SetStatusAt(putFromBeg[i] + 1, Status.Empty);
                                change = true;
                            }
                            else if ((putFromBeg[i] < UnfinishedRangeEnd) && (StatusAt(putFromBeg[i] + 1) == Status.Filled))
                            {
                                return null;
                            }
                            if ((putFromEnd[i] > UnfinishedRangeBeg) && (StatusAt(putFromEnd[i] - 1) == Status.Unknown))
                            {
                                SetStatusAt(putFromEnd[i] - 1, Status.Empty);
                                change = true;
                            }
                            else if ((putFromEnd[i] > UnfinishedRangeBeg) && (StatusAt(putFromEnd[i] - 1) == Status.Filled))
                            {
                                return null;
                            }
                            while ((FirstNotFinished <= LastNotFinished) && IsFinished(FirstNotFinished))
                                FirstNotFinished++;
                            while ((LastNotFinished >= FirstNotFinished) && IsFinished(LastNotFinished))
                                LastNotFinished--;
                        }
                    }
            
            //solving row #3
            //putting empty spaces between finished pairs of values
            position = 0;
            for (var i = 0; i < FirstNotFinished; i++)
            {
                while ((StatusAt(position) == Status.Empty) || (StatusAt(position) == Status.Unknown))
                {
                    if (StatusAt(position) == Status.Unknown)
                    {
                        SetStatusAt(position, Status.Empty);
                        change = true;
                    }
                    if (position < RespectibleSize() - 1)
                        position++;
                    else
                        break;
                }
                var drawnSize = 0;
                while (StatusAt(position) == Status.Filled)
                {
                    drawnSize++;
                    if (position < RespectibleSize() - 1)
                        position++;
                    else
                        break;
                }
                if (drawnSize != this[i])
                    return null;
            }
            while (StatusAt(position) == Status.Empty)
                if (position < RespectibleSize() - 1)
                    position++;
                else
                    break;
            UnfinishedRangeBeg = position;

            position = RespectibleSize() - 1;
            for (var i = Amount - 1; i > LastNotFinished; i--)
            {
                while ((StatusAt(position) == Status.Empty) || (StatusAt(position) == Status.Unknown))
                {
                    if (StatusAt(position) == Status.Unknown)
                    {
                        SetStatusAt(position, Status.Empty);
                        change = true;
                    }
                    if (position > 0)
                        position--;
                    else
                        break;
                }
                var drawnSize = 0;
                while (StatusAt(position) == Status.Filled)
                {
                    drawnSize++;
                    if (position > 0)
                        position--;
                    else
                        break;
                }
                if (drawnSize != this[i])
                    return null;
            }
            while ((position > 0) && (StatusAt(position) == Status.Empty))
                if (position > 0)
                    position--;
                else
                    break;
            UnfinishedRangeEnd = position;

            if (UnfinishedRangeEnd < UnfinishedRangeBeg)
            {
                position = 0;
                {
                    while (position < RespectibleSize())
                    {
                        if (StatusAt(position) == Status.Unknown)
                            SetStatusAt(position, Status.Empty);
                        position++;
                    }
                }
                _completelyFinished = true;
            }
            if (LastNotFinished < FirstNotFinished)
            {
                position = 0;
                {
                    while (position < RespectibleSize())
                    {
                        if (StatusAt(position) == Status.Unknown)
                            SetStatusAt(position, Status.Empty);
                        position++;
                    }
                }
                _completelyFinished = true;
            }

            //solving row #2
            //single unfinished value in row
            if (FirstNotFinished == LastNotFinished)
            {
                int? firstColoredPosition = null;
                int? lastColoradPosition = null;
                for (int i = UnfinishedRangeBeg; i <= UnfinishedRangeEnd; i++)
                {
                    if (StatusAt(i) == Status.Filled)
                    {
                        firstColoredPosition = i;
                        break;
                    }
                }
                for (int i = UnfinishedRangeEnd; i >= UnfinishedRangeBeg; i--)
                {
                    if (StatusAt(i) == Status.Filled)
                    {
                        lastColoradPosition = i;
                        break;
                    }
                }
                //coloring in the value
                if (firstColoredPosition != null && lastColoradPosition != null)
                {
                    for (int i = (int)firstColoredPosition; i <= (int)lastColoradPosition; i++)
                    {
                        if (StatusAt(i) == Status.Unknown)
                        {
                            SetStatusAt(i, Status.Filled);
                            change = true;
                        }
                    }

                    //emptying area around colored value
                    for (int i = (int)firstColoredPosition - this[FirstNotFinished]; i >= UnfinishedRangeBeg; i--)
                    {
                        if (StatusAt(i) == Status.Unknown)
                        {
                            SetStatusAt(i, Status.Empty);
                            change = true;
                        }
                    }
                    for (int i = (int)lastColoradPosition + this[FirstNotFinished]; i <= UnfinishedRangeEnd; i++)
                    {
                        if (StatusAt(i) == Status.Unknown)
                        {
                            SetStatusAt(i, Status.Empty);
                            change = true;
                        }
                    }
                }

            }


            return change;
        }

        /// <summary>
        /// Setsh status of whole line to empty
        /// </summary>
        /// <returns>Returns true if change occured, null if errorr occured</returns>
        public bool? Clean()
        {
            int position = 0;
            bool? result = false;
            while (position < RespectibleSize())
            {
                if (StatusAt(position) == Status.Unknown || StatusAt(position) == Status.Empty)
                {
                    if (StatusAt(position) == Status.Unknown)
                    {
                        result = true;
                        SetStatusAt(position, Status.Empty);
                    }
                    position++;
                }
                else if (StatusAt(position) == Status.Filled)
                {
                    return null;
                }
            }
            _completelyFinished = true;
            return result;
        }
        /// <summary>
        /// Clears solving process of line
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Amount; i++)
                _finished[i] = false;

            FirstNotFinished = 0;
            LastNotFinished = Amount - 1;
            UnfinishedRangeBeg = 0;
            UnfinishedRangeEnd = RespectibleSize() - 1;
            _completelyFinished = false;
        }
        /// <summary>
        /// Returns status of cell in grid in respective line on given position
        /// </summary>
        /// <param name="index">Position of the cell</param>
        /// <returns>Status at given position</returns>
        protected abstract Status StatusAt(int index);
        /// <summary>
        /// Sets status in grid in respective line on given position
        /// </summary>
        /// <param name="index">Position of cell which status to change</param>
        /// <param name="status">To what status to change the cell</param>
        protected abstract void SetStatusAt(int index, Status status);
        /// <summary>
        /// Size respectible to given line
        /// </summary>
        protected abstract int RespectibleSize();

        private bool IsFinished(int index)
        {
            return _finished[index];
        }

        private void SetFinished(int index)
        {
            _finished[index] = true;
        }

        public bool Finished()
        {
            return _completelyFinished;
        }
    }
}