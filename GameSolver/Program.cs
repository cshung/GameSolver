namespace GameSolver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            int size;
            if (!int.TryParse(Console.ReadLine(), out size))
            {
                Console.WriteLine("Wrong input");
                return;
            }
            int[,] board = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                string s = Console.ReadLine();
                if (s.Length != size)
                {
                    Console.WriteLine("Wrong input");
                    return;
                }
                for (int j = 0; j < size; j++)
                {
                    if (s[j] == 'R')
                    {
                        board[i, j] = -1;
                    }
                    else if (s[j] == 'B')
                    {
                        board[i, j] = 1;
                    }
                    else if (s[j] == 'X')
                    {
                        board[i, j] = 0;
                    }
                    else
                    {
                        Console.WriteLine("Wrong input");
                        return;
                    }
                }
            }

            List<Constraint> constraints = new List<Constraint>();

            // Each row sum to 0
            for (int row = 0; row < size; row++)
            {
                List<Tuple<int, int>> rowIndexes = new List<Tuple<int, int>>();
                for (int col = 0; col < size; col++)
                {
                    rowIndexes.Add(Tuple.Create(row, col));
                }
                constraints.Add(new SumEqualConstraint(rowIndexes, 0));
            }

            // Each col sum to 0
            for (int col = 0; col < size; col++)
            {
                List<Tuple<int, int>> colIndexes = new List<Tuple<int, int>>();
                for (int row = 0; row < size; row++)
                {
                    colIndexes.Add(Tuple.Create(row, col));
                }
                constraints.Add(new SumEqualConstraint(colIndexes, 0));
            }

            // Each row triplet sum not to 3 or -3
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size - 2; col++)
                {
                    List<Tuple<int, int>> rowIndexes = new List<Tuple<int, int>>();
                    for (int offset = 0; offset < 3; offset++)
                    {
                        rowIndexes.Add(Tuple.Create(row, col + offset));
                    }
                    constraints.Add(new SumNotEqualConstraint(rowIndexes, 3));
                    constraints.Add(new SumNotEqualConstraint(rowIndexes, -3));
                }
            }

            // Each col triplet sum not to 3 or -3
            for (int col = 0; col < size; col++)
            {
                for (int row = 0; row < size - 2; row++)
                {
                    List<Tuple<int, int>> colIndexes = new List<Tuple<int, int>>();
                    for (int offset = 0; offset < 3; offset++)
                    {
                        colIndexes.Add(Tuple.Create(row + offset, col));
                    }
                    constraints.Add(new SumNotEqualConstraint(colIndexes, 3));
                    constraints.Add(new SumNotEqualConstraint(colIndexes, -3));
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    constraints.Add(new NotIdenticalColumnConstraint(i, j, size));
                    constraints.Add(new NotIdenticalRowConstraint(i, j, size));
                }
            }

            while (true)
            {
                Console.Clear();
                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        if (board[row, col] == 0)
                        {
                            Console.Write('x');
                        }
                        else if (board[row, col] == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write('B');
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if (board[row, col] == -1)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write('R');
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    Console.WriteLine();
                }

                // Uncomment this to look at the immediate steps more carefully
                // Console.ReadLine();

                foreach (var constraint in constraints)
                {
                    constraint.Simplify(board);
                }

                bool propagated = false;
                foreach (var constraint in constraints)
                {
                    if (constraint.Propagate(board))
                    {
                        propagated = true;
                        break;
                    }
                }

                if (!propagated)
                {
                    break;
                }
            }

            Console.WriteLine();
        }
    }

    public abstract class Constraint
    {
        private static int count;
        private int id;
        public Constraint()
        {
            this.id = count++;
        }
        public abstract void Simplify(int[,] board);
        public abstract bool Propagate(int[,] board);
    }

    public class NotIdenticalColumnConstraint : Constraint
    {
        private int left;
        private int right;
        private int size;
        private int halfSize;
        private bool willNotPropagate;

        public NotIdenticalColumnConstraint(int i, int j, int size)
        {
            this.left = i;
            this.right = j;
            this.size = size;
            this.halfSize = this.size / 2;
            this.willNotPropagate = false;
        }

        public override void Simplify(int[,] board)
        {
            // There is nothing to simplify
        }

        public override bool Propagate(int[,] board)
        {
            if (!willNotPropagate)
            {
                List<int> leftRedIndexes = new List<int>();
                List<int> leftCyanIndexes = new List<int>();
                List<int> rightRedIndexes = new List<int>();
                List<int> rightCyanIndexes = new List<int>();

                for (int row = 0; row < size; row++)
                {
                    if (board[row, left] == -1)
                    {
                        leftRedIndexes.Add(row);
                    }
                    if (board[row, left] == 1)
                    {
                        leftCyanIndexes.Add(row);
                    }
                    if (board[row, right] == -1)
                    {
                        rightRedIndexes.Add(row);
                    }
                    if (board[row, right] == 1)
                    {
                        rightCyanIndexes.Add(row);
                    }
                }

                List<int> largerRedIndexes;
                List<int> smallerRedIndexes;
                bool leftIsLargerRed;
                List<int> largerCyanIndexes;
                List<int> smallerCyanIndexes;
                bool leftIsLargerCyan;

                if (leftRedIndexes.Count > rightRedIndexes.Count)
                {
                    largerRedIndexes = leftRedIndexes;
                    smallerRedIndexes = rightRedIndexes;
                    leftIsLargerRed = true;
                }
                else
                {
                    largerRedIndexes = rightRedIndexes;
                    smallerRedIndexes = leftRedIndexes;
                    leftIsLargerRed = false;
                }

                if (leftCyanIndexes.Count > rightCyanIndexes.Count)
                {
                    largerCyanIndexes = leftCyanIndexes;
                    smallerCyanIndexes = rightCyanIndexes;
                    leftIsLargerCyan = true;
                }
                else
                {
                    largerCyanIndexes = rightCyanIndexes;
                    smallerCyanIndexes = leftCyanIndexes;
                    leftIsLargerCyan = false;
                }

                if (largerRedIndexes.Count == this.halfSize)
                {
                    if (smallerRedIndexes.Except(largerRedIndexes).Count() > 0)
                    {
                        this.willNotPropagate = true;
                    }
                    else if (smallerRedIndexes.Count == this.halfSize - 1)
                    {
                        int mustMismatchIndex = largerRedIndexes.Except(smallerRedIndexes).Single();
                        if (leftIsLargerRed)
                        {
                            board[mustMismatchIndex, right] = 1;
                            return true;
                        }
                        else
                        {
                            board[mustMismatchIndex, left] = 1;
                            return true;
                        }
                    }
                }

                if (largerCyanIndexes.Count == this.halfSize)
                {
                    if (smallerCyanIndexes.Except(largerCyanIndexes).Count() > 0)
                    {
                        this.willNotPropagate = true;
                    }
                    else if (smallerCyanIndexes.Count == this.halfSize - 1)
                    {
                        int mustMismatchIndex = largerCyanIndexes.Except(smallerCyanIndexes).Single();
                        if (leftIsLargerCyan)
                        {
                            board[mustMismatchIndex, right] = -1;
                            return true;
                        }
                        else
                        {
                            board[mustMismatchIndex, left] = -1;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class NotIdenticalRowConstraint : Constraint
    {
        private int top;
        private int bottom;
        private int size;
        private int halfSize;
        private bool willNotPropagate;

        public NotIdenticalRowConstraint(int i, int j, int size)
        {
            this.top = i;
            this.bottom = j;
            this.size = size;
            this.halfSize = this.size / 2;
            this.willNotPropagate = false;
        }

        public override void Simplify(int[,] board)
        {
            // There is nothing to simplify
        }

        public override bool Propagate(int[,] board)
        {
            if (!willNotPropagate)
            {
                List<int> topRedIndexes = new List<int>();
                List<int> topCyanIndexes = new List<int>();
                List<int> bottomRedIndexes = new List<int>();
                List<int> bottomCyanIndexes = new List<int>();

                for (int col = 0; col < size; col++)
                {
                    if (board[top, col] == -1)
                    {
                        topRedIndexes.Add(col);
                    }
                    if (board[top, col] == 1)
                    {
                        topCyanIndexes.Add(col);
                    }
                    if (board[bottom, col] == -1)
                    {
                        bottomRedIndexes.Add(col);
                    }
                    if (board[bottom, col] == 1)
                    {
                        bottomCyanIndexes.Add(col);
                    }
                }

                List<int> largerRedIndexes;
                List<int> smallerRedIndexes;
                bool topIsLargerRed;
                List<int> largerCyanIndexes;
                List<int> smallerCyanIndexes;
                bool topIsLargerCyan;

                if (topRedIndexes.Count > bottomRedIndexes.Count)
                {
                    largerRedIndexes = topRedIndexes;
                    smallerRedIndexes = bottomRedIndexes;
                    topIsLargerRed = true;
                }
                else
                {
                    largerRedIndexes = bottomRedIndexes;
                    smallerRedIndexes = topRedIndexes;
                    topIsLargerRed = false;
                }

                if (topCyanIndexes.Count > bottomCyanIndexes.Count)
                {
                    largerCyanIndexes = topCyanIndexes;
                    smallerCyanIndexes = bottomCyanIndexes;
                    topIsLargerCyan = true;
                }
                else
                {
                    largerCyanIndexes = bottomCyanIndexes;
                    smallerCyanIndexes = topCyanIndexes;
                    topIsLargerCyan = false;
                }

                if (largerRedIndexes.Count == this.halfSize)
                {
                    if (smallerRedIndexes.Except(largerRedIndexes).Count() > 0)
                    {
                        this.willNotPropagate = true;
                    }
                    else if (smallerRedIndexes.Count == this.halfSize - 1)
                    {
                        int mustMismatchIndex = largerRedIndexes.Except(smallerRedIndexes).Single();
                        if (topIsLargerRed)
                        {
                            board[bottom, mustMismatchIndex] = 1;
                            return true;
                        }
                        else
                        {
                            board[top, mustMismatchIndex] = 1;
                            return true;
                        }
                    }
                }

                if (largerCyanIndexes.Count == this.halfSize)
                {
                    if (smallerCyanIndexes.Except(largerCyanIndexes).Count() > 0)
                    {
                        this.willNotPropagate = true;
                    }
                    else if (smallerCyanIndexes.Count == this.halfSize - 1)
                    {
                        int mustMismatchIndex = largerCyanIndexes.Except(smallerCyanIndexes).Single();
                        if (topIsLargerCyan)
                        {
                            board[bottom, mustMismatchIndex] = -1;
                            return true;
                        }
                        else
                        {
                            board[top, mustMismatchIndex] = -1;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class SumNotEqualConstraint : Constraint
    {
        private List<Tuple<int, int>> indexes;
        private int sumTo;

        public SumNotEqualConstraint(List<Tuple<int, int>> indexes, int sumTo)
        {
            this.indexes = indexes;
            this.sumTo = sumTo;
        }

        public override void Simplify(int[,] board)
        {
            List<Tuple<int, int>> newIndexes = new List<Tuple<int, int>>();
            int newSum = sumTo;
            foreach (var index in this.indexes)
            {
                if (board[index.Item1, index.Item2] == 0)
                {
                    newIndexes.Add(index);
                }
                else
                {
                    newSum -= board[index.Item1, index.Item2];
                }
            }

            this.indexes = newIndexes;
            this.sumTo = newSum;
        }

        public override bool Propagate(int[,] board)
        {
            if (this.indexes.Count == 1 && (this.sumTo == -1 || this.sumTo == 1))
            {
                board[this.indexes[0].Item1, this.indexes[0].Item2] = -this.sumTo;
                return true;
            }

            return false;
        }
    }


    public class SumEqualConstraint : Constraint
    {
        private List<Tuple<int, int>> indexes;
        private int sumTo;

        public SumEqualConstraint(List<Tuple<int, int>> indexes, int sumTo)
        {
            this.indexes = indexes;
            this.sumTo = sumTo;
        }

        public override void Simplify(int[,] board)
        {
            List<Tuple<int, int>> newIndexes = new List<Tuple<int, int>>();
            int newSum = sumTo;
            foreach (var index in this.indexes)
            {
                if (board[index.Item1, index.Item2] == 0)
                {
                    newIndexes.Add(index);
                }
                else
                {
                    newSum -= board[index.Item1, index.Item2];
                }
            }

            this.indexes = newIndexes;
            this.sumTo = newSum;
        }

        public override bool Propagate(int[,] board)
        {
            if (this.indexes.Count == 0)
            {
                return false;
            }

            if (this.indexes.Count == this.sumTo)
            {
                foreach (var index in this.indexes)
                {
                    board[index.Item1, index.Item2] = 1;
                }

                return true;
            }
            if (this.indexes.Count == -this.sumTo)
            {
                foreach (var index in this.indexes)
                {
                    board[index.Item1, index.Item2] = -1;
                }

                return true;
            }

            return false;
        }
    }
}