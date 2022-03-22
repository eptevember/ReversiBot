using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reversi
{
    class Program
    {
        static void Main(string[] args)
        {
            DoRealRun(args);
            // TestBot();
            // TestAPIWithBothPlayers();

            //int wait = Convert.ToInt32(Console.ReadLine());
        }

        static void DoRealRun(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Expected arguments: BoardName CreateBoard PlayerName Depth UseBetterBot");
                Console.WriteLine("Example: board1 1 player1 5 1");
                return;
            }

            string boardName = args[0];
            bool createBoard = (args[1] == "1" ? true : false);
            string playerName = args[2];
            int depth = Int32.Parse(args[3]);
            bool useBetterBot = (args[4] == "1" ? true : false);


            //ReversiBot reversiBot = new ReversiBotLockedPosition(5, 12);

            ReversiBot reversiBot = null;
            if (useBetterBot)
            {
                reversiBot = new ReversiBotLockedPosition(depth, 12);
            }
            else
            {
                reversiBot = new ReversiBotTrivial(depth);
            }


            reversiBot.timeLimitSecs = 2 * 60;

            ReversiAPI reversiAPI = new ReversiAPI();
            if (createBoard)
            {
                if (boardName == "")
                {
                    reversiAPI.CreateBoard();
                    Console.WriteLine(reversiAPI.boardID);
                }
                else
                {
                    reversiAPI.CreateBoardWithID(boardName);
                }
            }
            else
            {
                reversiAPI.boardID = boardName;
            }

            reversiAPI.JoinBoard(playerName);

            bool done = false;
            while (!done)
            {
                done = reversiAPI.FetchBoardAndMakeMove(reversiBot);
                Thread.Sleep(2000);
            }
        }

        static void TestAPIWithBothPlayers() {
            ReversiBot reversiBot1 = new ReversiBotLockedPosition(5, 12);
            reversiBot1.timeLimitSecs = 60;
            ReversiBot reversiBot2 = new ReversiBotLockedPosition(5, 12);
            reversiBot2.timeLimitSecs = 60;

            ReversiAPI reversiAPI = new ReversiAPI();
            reversiAPI.CreateBoardWithID("e11");
            Console.WriteLine(reversiAPI.boardID);

            reversiAPI.JoinBoard("BetterBot1");

            ReversiAPI reversiAPI2 = new ReversiAPI();
            reversiAPI2.boardID = reversiAPI.boardID;
            reversiAPI2.JoinBoard("BetterBot2");

            bool done = false;
            while (!done)
            {
                done = reversiAPI.FetchBoardAndMakeMove(reversiBot1);
                //Thread.Sleep(1000);
                done = reversiAPI2.FetchBoardAndMakeMove(reversiBot2);
                //Thread.Sleep(1000);
            }
        }

        static void TestBot()
        {
            Player userTurn = Player.None;

            ReversiBot reversiBotPlayer = new ReversiBotTrivial(0);

            //ReversiBot reversiBotWhite = new ReversiBotTrivial(3);
            ReversiBot reversiBotWhite = new ReversiBotLockedPosition(6, 12);
            ReversiBot reversiBotBlack = new ReversiBotLockedPosition(6, 12);
            // ReversiBot reversiBotBlack = new ReversiBotLockedPosition(0, 3.0);

            int repetitions = 60;
            int[] wins = new int[] { 0, 0, 0, 0 };
            for (int i = 0; i < repetitions; i++)
            {
                ReversiBoard board = ReversiBoard.GenerateInitialBoard();
                //board.Print();
                while (board.winner == Player.None)
                {
                    if (board.turn == userTurn)
                    {

                        BoardPosition nextMove = GetMovementFromConsole();
                        if (reversiBotPlayer.IsPossibleMove(board, board.turn, nextMove))
                        {
                            Console.WriteLine(nextMove.x + ", " + nextMove.y);
                            board = reversiBotPlayer.GetBoardAfterMove(board, nextMove);
                        }
                        else
                        {
                            Console.WriteLine("Movement not valid");
                        }

                    }
                    else
                    {
                        if (board.turn == Player.White)
                        {
                            board = reversiBotWhite.GetBoardAfterCalculatedMove(board);
                            reversiBotWhite.Print(board);
                        }
                        else
                        {
                            board = reversiBotBlack.GetBoardAfterCalculatedMove(board);
                            reversiBotBlack.Print(board);
                        }
                    }
                }

                reversiBotPlayer.Print(board);
                wins[(int)board.winner]++;
            }

            Console.WriteLine("Black: " + wins[(int)Player.Black]);
            Console.WriteLine("White: " + wins[(int)Player.White]);
            Console.WriteLine("Tie: " + wins[(int)Player.Tie]);
            Console.WriteLine("Done");
        }

        static BoardPosition GetMovementFromConsole()
        {
            BoardPosition result = new BoardPosition(0, 0);
            while (true)
            {
                Console.WriteLine("Enter coordinate for move (A1 - H8)");
                String input = Console.ReadLine();
                if (input.Length == 2)
                {
                    if (input[0] >= 'a' && input[0] <= 'h')
                        result.x = input[0] - 'a';
                    else if (input[0] >= 'A' && input[0] <= 'H')
                        result.x = input[0] - 'A';
                    else
                        continue;

                    if (input[1] >= '1' && input[1] <= '8')
                        result.y = input[1] - '1';
                    else
                        continue;

                    return result;
                }
            }
        }
    }
}
