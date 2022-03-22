using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    class ReversiBotTrivial : ReversiBot
    {
        public ReversiBotTrivial(int _maxDepth) : base(_maxDepth)
        {
        }

        public override double CalculateBoardScore(ReversiBoard board, Player player)
        {
            double score = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board.board[x, y] == Player.None) continue;
                    if (board.board[x, y] == player) score += 1;
                    else score -= 1;
                }
            }

            return score;
        }

    }

    class ReversiBotLockedPosition : ReversiBot
    {
        protected double lockedPositionScore;

        public ReversiBotLockedPosition(int _maxDepth, double _lockedPositionScore) : base(_maxDepth)
        {
            lockedPositionScore = _lockedPositionScore;
        }

        public override double CalculateBoardScore(ReversiBoard board, Player player)
        {
            double score = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board.board[x, y] == Player.None) continue;

                    double scoreIncrease = 1.0;
                    if (IsPositionLocked(board, new BoardPosition(x, y)))
                        scoreIncrease = lockedPositionScore;

                    if (board.board[x, y] == player) score += scoreIncrease;
                    else score -= scoreIncrease;
                }
            }

            return score;
        }

        public override void Print(ReversiBoard board)
        {
            Console.WriteLine("Turn: " + board.turn.ToString());
            Console.WriteLine("Winner: " + board.winner.ToString());

            Console.Write("  ");
            for (int x = 0; x < 8; x++)
            {
                Console.Write((char)('A' + x));
                Console.Write(" ");
            }
            Console.WriteLine("");

            for (int y = 0; y < 8; y++)
            {
                Console.Write(y + 1);
                Console.Write(" ");

                for (int x = 0; x < 8; x++)
                {
                    if (IsPositionLocked(board, new BoardPosition(x, y)))
                    {
                        Console.Write(PlayerToChar(board.board[x, y]) + " ");
                    }
                    else
                    {
                        Console.Write(PlayerToCharMinor(board.board[x, y]) + " ");
                    }

                }

                Console.WriteLine("");
            }
        }

        static char PlayerToCharMinor(Player player)
        {
            switch (player)
            {
                case Player.None:
                    return '-';
                case Player.White:
                    return 'w';
                case Player.Black:
                    return 'b';
                default:
                    return '?';
            }
        }

        static char PlayerToChar(Player player)
        {
            switch (player)
            {
                case Player.None:
                    return '-';
                case Player.White:
                    return 'W';
                case Player.Black:
                    return 'B';
                default:
                    return '?';
            }
        }

        bool IsPositionLocked(ReversiBoard board, BoardPosition position)
        {
            Player player = board.board[position.x, position.y];
            foreach (var direction in OneSideDirections)
            {
                int x = position.x;
                int y = position.y;

                bool lockedOneSide = false;

                while (true)
                {
                    x += direction.x;
                    y += direction.y;

                    if (x < 0 || x >= 8 || y < 0 || y >= 8)
                    {
                        lockedOneSide = true;
                        break;
                    }

                    Player cell = board.board[x, y];
                    if (cell != player)
                    {
                        break;
                    }
                }

                if (!lockedOneSide)
                {
                    x = position.x;
                    y = position.y;
                    while (true)
                    {
                        x -= direction.x;
                        y -= direction.y;

                        if (x < 0 || x >= 8 || y < 0 || y >= 8)
                        {
                            lockedOneSide = true;
                            break;
                        }

                        Player cell = board.board[x, y];
                        if (cell != player)
                        {
                            break;
                        }
                    }
                }

                if (!lockedOneSide) return false;
            }

            return true;
        }
    }

    abstract class ReversiBot
    {
        protected BoardPosition InvalidPosition = new BoardPosition(-1, -1);
        protected List<BoardPosition> Directions = new List<BoardPosition>();
        protected List<BoardPosition> OneSideDirections = new List<BoardPosition>();
        protected Random rand = new Random();

        public int maxDepth = 1;
        public bool debug = false;
        public double timeLimitSecs = 10;
        
        protected Stopwatch timer;


        struct BoardPositionAndScore
        {
            public BoardPosition position;
            public double score;

            public BoardPositionAndScore(BoardPosition _position, double _score)
            {
                position = _position;
                score = _score;
            }
        }

        public ReversiBot(int _maxDepth)
        {
            maxDepth = _maxDepth;
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) continue;
                    Directions.Add(new BoardPosition(x, y));
                }
            }

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) break;
                    OneSideDirections.Add(new BoardPosition(x, y));
                }
            }
        }

        Player OtherPlayer(Player p) { return 3 - p; }

        public ReversiBoard GetBoardAfterCalculatedMove(ReversiBoard board)
        {
            ReversiBoard newBoard = GenerateNewBoardAfterMove(board, CalculateNextMove(board));
            CalculateWinnerAndNextPlayer(newBoard);

            return newBoard;
        }

        public ReversiBoard GetBoardAfterMove(ReversiBoard board, BoardPosition position)
        {
            if (!IsPossibleMove(board, board.turn, position))
            {
                Console.WriteLine("Invalid movement");
                return board;
            }


            ReversiBoard newBoard = GenerateNewBoardAfterMove(board, position);
            CalculateWinnerAndNextPlayer(newBoard);

            return newBoard;
        }



        public BoardPosition CalculateNextMove(ReversiBoard board)
        {
            timer = new Stopwatch();
            timer.Start();

            BoardPositionAndScore positionAndScore = CalculateNextMoveAndScore(board, board.turn, 0);

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));

            return positionAndScore.position;
        }

        BoardPositionAndScore CalculateNextMoveAndScore(ReversiBoard board, Player playerToCalculateScore, int depth)
        {
            List<BoardPosition> possibleMoves = GetPossibleMoves(board, board.turn);
            if (possibleMoves.Count == 0)
            {
                board.turn = OtherPlayer(board.turn);
                possibleMoves = GetPossibleMoves(board, board.turn);

                if (possibleMoves.Count == 0)
                {
                    double finalScore = CalculateTrivialScore(board, playerToCalculateScore);
                    return new BoardPositionAndScore(InvalidPosition, finalScore * 1000);
                }
            }

            bool canContinueRecursion = depth < maxDepth && timer.ElapsedMilliseconds < timeLimitSecs * 1000;

            BoardPosition selectedMove = InvalidPosition;
            double selectedScore = board.turn == playerToCalculateScore ? double.NegativeInfinity : double.PositiveInfinity;

            foreach (BoardPosition move in possibleMoves)
            {
                double score;
                ReversiBoard newBoard = GenerateNewBoardAfterMove(board, move);
                if (canContinueRecursion)
                {
                    BoardPositionAndScore next = CalculateNextMoveAndScore(newBoard, playerToCalculateScore, depth + 1);
                    score = next.score;
                }
                else
                {
                    score = CalculateBoardScore(newBoard, playerToCalculateScore);
                }

                if ((board.turn == playerToCalculateScore && score > selectedScore) ||
                    (board.turn != playerToCalculateScore && score < selectedScore) ||
                    (score == selectedScore && rand.NextDouble() < 0.2))
                {
                    selectedScore = score;
                    selectedMove = move;
                }
            }

            return new BoardPositionAndScore(selectedMove, selectedScore);
        }

        List<BoardPosition> GetPossibleMoves(ReversiBoard board, Player player)
        {
            List<BoardPosition> moves = new List<BoardPosition>();
            BoardPosition move = new BoardPosition(0, 0);

            for (move.x = 0; move.x < 8; move.x++)
            {
                for (move.y = 0; move.y < 8; move.y++)
                {
                    if (IsPossibleMove(board, player, move))
                        moves.Add(move);
                }
            }

            return moves;
        }

        public bool IsPossibleMove(ReversiBoard board, Player player, BoardPosition position)
        {
            if (board.board[position.x, position.y] != Player.None)
                return false;

            foreach (var direction in Directions)
            {
                if (FindMovementEdge(board, position, player, direction).IsValid())
                    return true;
            }

            return false;
        }

        BoardPosition FindMovementEdge(ReversiBoard board, BoardPosition position, Player player, BoardPosition direction)
        {
            int x = position.x + direction.x;
            int y = position.y + direction.y;

            if (x < 0 || x >= 8 || y < 0 || y >= 8)
            {
                return InvalidPosition;
            }

            if (board.board[x, y] != OtherPlayer(player))
            {
                return InvalidPosition;
            }

            while (true)
            {
                x += direction.x;
                y += direction.y;

                if (x < 0 || x >= 8 || y < 0 || y >= 8)
                {
                    return InvalidPosition;
                }

                Player cell = board.board[x, y];
                if (cell == player)
                {
                    return new BoardPosition(x, y);
                }

                if (cell == Player.None)
                {
                    return InvalidPosition;
                }
            }
        }

        ReversiBoard GenerateNewBoardAfterMove(ReversiBoard oldBoard, BoardPosition position)
        {
            ReversiBoard newBoard = oldBoard.Clone();
            Player player = oldBoard.turn;
            newBoard.turn = OtherPlayer(oldBoard.turn);

            if (position.IsValid())
            {
                foreach (var direction in Directions)
                {
                    BoardPosition movementEdge = FindMovementEdge(oldBoard, position, player, direction);
                    if (!movementEdge.IsValid()) continue;
                    int x = position.x;
                    int y = position.y;
                    while (x != movementEdge.x || y != movementEdge.y)
                    {
                        x += direction.x;
                        y += direction.y;
                        newBoard.board[x, y] = player;
                    }

                }

                newBoard.board[position.x, position.y] = player;
            }

            return newBoard;
        }

        double CalculateTrivialScore(ReversiBoard board, Player player)
        {
            double score = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board.board[x, y] == Player.None) continue;
                    if (board.board[x, y] == player) score += 1;
                    else score -= 1;
                }
            }

            return score;
        }

        void CalculateWinnerAndNextPlayer(ReversiBoard board)
        {
            bool whiteCanMove = GetPossibleMoves(board, Player.White).Count != 0;
            bool blackCanMove = GetPossibleMoves(board, Player.Black).Count != 0;
            if (!whiteCanMove && !blackCanMove)
            {
                double score = CalculateTrivialScore(board, Player.Black);

                if (score > 0) board.winner = Player.Black;
                else if (score < 0) board.winner = Player.White;
                else board.winner = Player.Tie;
            }
            else if (!whiteCanMove && board.turn == Player.White)
            {
                board.turn = Player.Black;
            }
            else if (!blackCanMove && board.turn == Player.Black)
            {
                board.turn = Player.White;
            }
        }

        public abstract double CalculateBoardScore(ReversiBoard board, Player player);

        public virtual void Print(ReversiBoard board)
        {
            board.Print();
        }


    }
}
