using System;
using System.Collections.Generic;
using System.Text;

namespace FiveInARow
{
    public class GameLogic
    {
        public ChessType[,] Chessboard;
        private IController m_BlackChessPlayer;
        public IController BlackChessPlayer
        {
            get => m_BlackChessPlayer;
            set
            {
                value.ChessType = ChessType.Black;
                m_BlackChessPlayer = value;
            }
        }
        private IController m_WhiteChessPlayer;
        public IController WhiteChessPlayer
        {
            get => m_WhiteChessPlayer;
            set
            {
                value.ChessType = ChessType.White;
                m_WhiteChessPlayer = value;
            }
        }

        public IController CurrentPlayer { get; private set; }

        public bool GameEnd { get; private set; }
        public ChessType Winner { get; private set; }
        public List<Vector2Int> ChessRecords { get; private set; }
        public event Action OneTurnFinish_Handle;

        public GameLogic()
        {
            Chessboard = new ChessType[Defined.Height, Defined.Width];
            GameEnd = false;
            Winner = ChessType.Empty;
            ChessRecords = new List<Vector2Int>();
        }

        public IController ToOpponent(IController currentPlayer)
        {
            if (currentPlayer == BlackChessPlayer)
                return WhiteChessPlayer;
            else if (currentPlayer == WhiteChessPlayer)
                return BlackChessPlayer;
            else
                throw new Exception(nameof(currentPlayer));
        }

        public void PlayToEnd()
        {
            CurrentPlayer = null;
            int playTurnCount = 0;
            while (!GameEnd && playTurnCount++ < Defined.Size)
            {
                if (CurrentPlayer == null)
                    CurrentPlayer = BlackChessPlayer;
                else
                    CurrentPlayer = ToOpponent(CurrentPlayer);
                CurrentPlayer.Play(this, out Vector2Int position);
                if (position.Y < 0 || position.Y > Chessboard.GetLength(0) ||
                    position.X < 0 || position.X > Chessboard.GetLength(1))
                {
                    throw new Exception($"{CurrentPlayer} want to play chess at {position.X}, {position.Y}");
                }
                if (Chessboard[position.Y, position.X] != ChessType.Empty)
                {
                    throw new Exception($"{CurrentPlayer} want to replace chess {Chessboard[position.Y, position.X]} at {position.X}, {position.Y}");
                }
                Chessboard[position.Y, position.X] = CurrentPlayer.ChessType;
                ChessRecords.Add(position);

                if (XYFiveInARow(CurrentPlayer.ChessType, position) || SlopeFiveInARow(CurrentPlayer.ChessType, position))
                {
                    GameEnd = true;
                    Winner = CurrentPlayer.ChessType;
                }
                OneTurnFinish_Handle?.Invoke();
            }
            BlackChessPlayer.GameEnd(this);
            WhiteChessPlayer.GameEnd(this);
        }
        public bool XYFiveInARow(ChessType chessType, Vector2Int position)
        {
            int count = 0;
            ChessType[,] chessboard = Chessboard;
            for (int x = position.X + 1; x < chessboard.GetLength(1); x++)
            {
                if (chessboard[position.Y, x] == chessType)
                    count++;
                else
                    break;
            }
            for (int x = position.X - 1; x >= 0; x--)
            {
                if (chessboard[position.Y, x] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;

            count = 0;
            for (int y = position.Y + 1; y < chessboard.GetLength(0); y++)
            {
                if (chessboard[y, position.X] == chessType)
                    count++;
                else
                    break;
            }
            for (int y = position.Y - 1; y >= 0; y--)
            {
                if (chessboard[y, position.X] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;

            return false;
        }
        public bool SlopeFiveInARow(ChessType chessType, Vector2Int position)
        {
            int count = 0;
            ChessType[,] chessboard = Chessboard;
            int x, y;
            for (x = position.X + 1, y = position.Y + 1; x < chessboard.GetLength(1) && y < chessboard.GetLength(0); x++, y++)
            {
                if (chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            for (x = position.X - 1, y = position.Y - 1; x >= 0 && y >= 0; x--, y--)
            {
                if (chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;

            count = 0;
            for (x = position.X + 1, y = position.Y - 1; x < chessboard.GetLength(1) && y >= 0; x++, y--)
            {
                if (chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            for (x = position.X - 1, y = position.Y + 1; x >= 0 && y < chessboard.GetLength(0); x--, y++)
            {
                if (chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;
            return false;
        }

        public Vector2Int RandomPickEmptyPosition()
        {
            int emptyPositionCount = Defined.Size - ChessRecords.Count;
            if (emptyPositionCount == 0)
                throw new Exception("Zero empty position can't random pick");
            int randomIndex = Defined.Random.Next() % emptyPositionCount;
            ChessType[,] chessboard = Chessboard;
            for (int row = 0; row < chessboard.GetLength(0); row++)
                for (int column = 0; column < chessboard.GetLength(1); column++)
                {
                    if (chessboard[row, column] == ChessType.Empty)
                    {
                        if (randomIndex-- == 0)
                            return new Vector2Int(column, row);
                    }
                }
            throw new Exception("random pick have bug");
        }

        internal float[] ConvertToNNFormat(ChessType aiChessType)
        {
            float[] value = ArrayBuffer.Rent(Defined.NNInputSize);
            int offset = 0;
            for (int row = 0; row < Defined.Height; row++)
            {
                for (int column = 0; column < Defined.Width; column++)
                {
                    ChessType chessType = Chessboard[row, column];
                    if (chessType == ChessType.Empty)
                    {
                        // default value is zero, skip set value to improve performance
                        offset += 2;
                    }
                    else if (chessType == aiChessType)
                    {
                        value[offset++] = Defined.AIChessValue;
                        value[offset++] = Defined.EmptyChessValue;
                    }
                    else
                    {
                        value[offset++] = Defined.EmptyChessValue;
                        value[offset++] = Defined.OpponentChessValue;
                    }
                }
            }
            ChessType opponentChessType = aiChessType.ToOpponentChessType();
            // five in row or five in column
            for (int row = 0; row < Defined.Height - 5 + 1; row++)
                for (int column = 0; column < Defined.Width; column++)
                {
                    int aiChessCount = 0;
                    int opponentChessCount = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        ChessType chessType = Chessboard[row + i, column];
                        if (chessType == aiChessType)
                            aiChessCount++;
                        else if (chessType == opponentChessType)
                            opponentChessCount++;
                    }
                    value[offset++] = opponentChessCount == 0 ? aiChessCount / 4f : 0f;
                    value[offset++] = aiChessCount == 0 ? opponentChessCount / 4f : 0f;
                }
            for (int row = 0; row < Defined.Height; row++)
                for (int column = 0; column < Defined.Width - 5 + 1; column++)
                {
                    int aiChessCount = 0;
                    int opponentChessCount = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        ChessType chessType = Chessboard[row, column + i];
                        if (chessType == aiChessType)
                            aiChessCount++;
                        else if (chessType == opponentChessType)
                            opponentChessCount++;
                    }
                    value[offset++] = opponentChessCount == 0 ? aiChessCount / 4f : 0f;
                    value[offset++] = aiChessCount == 0 ? opponentChessCount / 4f : 0f;
                }
            // five in slope
            for (int row = 0; row < Defined.Height - 5 + 1; row++)
                for (int column = 0; column < Defined.Width - 5 + 1; column++)
                {
                    int aiChessCount = 0;
                    int opponentChessCount = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        ChessType chessType = Chessboard[row + i, column + i];
                        if (chessType == aiChessType)
                            aiChessCount++;
                        else if (chessType == opponentChessType)
                            opponentChessCount++;
                    }
                    value[offset++] = opponentChessCount == 0 ? aiChessCount / 4f : 0f;
                    value[offset++] = aiChessCount == 0 ? opponentChessCount / 4f : 0f;
                }
            for (int row = 0; row < Defined.Height - 5 + 1; row++)
                for (int column = 0; column < Defined.Width - 5 + 1; column++)
                {
                    int aiChessCount = 0;
                    int opponentChessCount = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        ChessType chessType = Chessboard[row + i, column + 4 - i];
                        if (chessType == aiChessType)
                            aiChessCount++;
                        else if (chessType == opponentChessType)
                            opponentChessCount++;
                    }
                    value[offset++] = opponentChessCount == 0 ? aiChessCount / 4f : 0f;
                    value[offset++] = aiChessCount == 0 ? opponentChessCount / 4f : 0f;
                }
            if (offset != Defined.NNInputSize || offset != value.Length)
                throw new ArgumentException("nn format error");
            return value;
        }
        internal void ConvertToLogFormat(StringBuilder stringBuilder)
        {
            stringBuilder.Append("     ");
            for (int column = 0; column < Defined.Width; column++)
                stringBuilder.Append(column).Append("   ");
            stringBuilder.AppendLine();
            for (int row = 0; row < Defined.Height; row++)
            {
                stringBuilder.Append(row).Append("   ");
                for (int column = 0; column < Defined.Width; column++)
                {
                    ChessType type = Chessboard[row, column];
                    if (type == ChessType.Black)
                        stringBuilder.Append(" X  ");
                    else if (type == ChessType.Empty)
                        stringBuilder.Append(" _  ");
                    else if (type == ChessType.White)
                        stringBuilder.Append(" O  ");
                }
                stringBuilder.AppendLine();
            }
        }

        internal void Copy(GameLogic gameLogic)
        {
            for (int row = 0; row < Chessboard.GetLength(0); row++)
                for (int column = 0; column < Chessboard.GetLength(1); column++)
                    Chessboard[row, column] = gameLogic.Chessboard[row, column];
            m_BlackChessPlayer = gameLogic.m_BlackChessPlayer;
            m_WhiteChessPlayer = gameLogic.m_WhiteChessPlayer;
            GameEnd = gameLogic.GameEnd;
            Winner = gameLogic.Winner;
            ChessRecords.Clear();
            ChessRecords.AddRange(gameLogic.ChessRecords);
        }
        internal void Repentance(out Vector2Int lastChessPosition, out ChessType chessType)
        {
            if (ChessRecords.Count == 0)
                throw new Exception("Repentance, ChessRecords.Count = 0");
            lastChessPosition = ChessRecords[ChessRecords.Count - 1];
            ChessRecords.RemoveAt(ChessRecords.Count - 1);
            chessType = Chessboard[lastChessPosition.Y, lastChessPosition.X];
            Chessboard[lastChessPosition.Y, lastChessPosition.X] = ChessType.Empty;
        }
    }
}