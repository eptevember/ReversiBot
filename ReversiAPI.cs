using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Collections.Specialized;

using Newtonsoft.Json;

namespace Reversi
{
    class CreateBoardResponse
    {
        public string BoardID;
    }

    class JoinBoardResponse
    {
        public string Player;
        public string PlayerKey;
    }

    class GetBoardResponse
    {
        public string Turn;
        public string Winner;
        public List<List<string>> Board;
    }

    class ReversiAPI
    {
        public string boardID;
        string playerKey;
        Player player = Player.None;

        Player StringToPlayer(string player)
        {
            if (player == "white") return Player.White;
            else if (player == "black") return Player.Black;
            else if (player == "tie") return Player.Tie;
            else return Player.None;
        }

        string SendPost(NameValueCollection data)
        {
            string url = "https://www.eptevember.com/reversiapi.php";
            //string url = "http://localhost/reversiapi.php";

            try
            {
                using (var wb = new WebClient())
                {
                    var response = wb.UploadValues(url, "POST", data);
                    return Encoding.UTF8.GetString(response);
                }
            } catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                //throw;
                return "";
            }
        }

        public void CreateBoard()
        {
            var data = new NameValueCollection();
            data["Operation"] = "CreateBoard";
            string response = SendPost(data);

            CreateBoardResponse createBoardResponse = JsonConvert.DeserializeObject<CreateBoardResponse>(response);
            boardID = createBoardResponse.BoardID;
        }

        public void CreateBoardWithID(string id)
        {
            var data = new NameValueCollection();
            data["Operation"] = "CreateBoardWithID";
            data["BoardID"] = id;
            string response = SendPost(data);

            CreateBoardResponse createBoardResponse = JsonConvert.DeserializeObject<CreateBoardResponse>(response);
            boardID = createBoardResponse.BoardID;
        }

        public void JoinBoard(string playerName)
        {
            var data = new NameValueCollection();
            data["Operation"] = "JoinBoard";
            data["BoardID"] = boardID;
            data["PlayerName"] = playerName;
            string response = SendPost(data);

            Console.WriteLine(response);

            JoinBoardResponse joinBoardResponse = JsonConvert.DeserializeObject<JoinBoardResponse>(response);
            playerKey = joinBoardResponse.PlayerKey;
            player = StringToPlayer(joinBoardResponse.Player);
        }
        public void CreateAndJoinBoard()
        {
            CreateBoard();
            JoinBoard("");
        }

        public ReversiBoard GetBoard()
        {
            var data = new NameValueCollection();
            data["Operation"] = "GetBoard";
            data["BoardID"] = boardID;
            string response = SendPost(data);

            GetBoardResponse getBoardResponse = JsonConvert.DeserializeObject<GetBoardResponse>(response);
            ReversiBoard reversiBoard = new ReversiBoard();
            reversiBoard.turn = StringToPlayer(getBoardResponse.Turn);
            reversiBoard.winner = StringToPlayer(getBoardResponse.Winner);
            reversiBoard.board = new Player[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    reversiBoard.board[x, y] = StringToPlayer(getBoardResponse.Board[x][y]);
                }
            }

            return reversiBoard;
        }

        string BoardPositionToString(BoardPosition position)
        {
            char[] result = new char[2];
            result[0] = (char)('A' + position.x);
            result[1] = (char)('1' + position.y);

            return new string(result);
        }

        public void SetPiece(ReversiBoard board, ReversiBot reversiBot)
        {
            BoardPosition position = reversiBot.CalculateNextMove(board);
            string positionStr = BoardPositionToString(position);

            var data = new NameValueCollection();
            data["Operation"] = "SetPiece";
            data["BoardID"] = boardID;
            data["PlayerKey"] = playerKey;
            data["Position"] = positionStr;
            string response = SendPost(data);
            Console.WriteLine(response);
        }

        // Return true if the game ended
        public bool FetchBoardAndMakeMove(ReversiBot reversiBot)
        {
            ReversiBoard board = GetBoard();
            if (board.winner != Player.None)
                return true;

            if (board.turn == player)
            {
                SetPiece(board, reversiBot);
            }

            return false;
        }
    }
}
