using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    enum Player
    {
        None = 0,
        White = 1,
        Black = 2,
        Tie = 3
    }

    struct BoardPosition
    {
        public int x;
        public int y;

        public BoardPosition(int _x, int _y) 
        {
            x = _x;
            y = _y;
        }

        public bool IsValid()
        {
            return x >= 0;
        }

        public void Print()
        {
            Console.Write("[" + x + ", " + y + "]");
        }
    }

    class ReversiBoard
    {
        public Player turn;
        public Player winner;
        public Player[,] board;

        public ReversiBoard Clone()
        {
            ReversiBoard cloneBoard = new ReversiBoard();
            cloneBoard.turn = turn;
            cloneBoard.winner = winner;
            cloneBoard.board = board.Clone() as Player[,];

            return cloneBoard;
        }

        public void Print()
        {
            Console.WriteLine("Turn: " + turn.ToString());
            Console.WriteLine("Winner: " + winner.ToString());

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
                    Console.Write(PlayerToChar(board[x, y]) + " ");
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

        public static ReversiBoard GenerateInitialBoard()
        {
            ReversiBoard initialBoard = new ReversiBoard();
            initialBoard.turn = Player.Black;
            initialBoard.winner = Player.None;
            initialBoard.board = new Player[8, 8];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    initialBoard.board[x, y] = Player.None;
                }
            }

            initialBoard.board[3, 3] = Player.White;
            initialBoard.board[4, 4] = Player.White;
            initialBoard.board[3, 4] = Player.Black;
            initialBoard.board[4, 3] = Player.Black;

            return initialBoard;
        }
    }
}
