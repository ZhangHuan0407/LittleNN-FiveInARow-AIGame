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
            IController? currentPlayer = null;
            int playTurnCount = 0;
            while (!GameEnd && playTurnCount++ < Defined.Size)
            {
                if (currentPlayer == null)
                    currentPlayer = BlackChessPlayer;
                else
                    currentPlayer = ToOpponent(currentPlayer);
                currentPlayer.Play(this, out Vector2Int position);
                if (position.Y < 0 || position.Y > Chessboard.GetLength(0) ||
                    position.X < 0 || position.X > Chessboard.GetLength(1))
                {
                    throw new Exception($"{currentPlayer} want to play chess at {position.X}, {position.Y}");
                }
                if (Chessboard[position.Y, position.X] != ChessType.Empty)
                {
                    throw new Exception($"{currentPlayer} want to replace chess {Chessboard[position.Y, position.X]} at {position.X}, {position.Y}");
                }
                Chessboard[position.Y, position.X] = currentPlayer.ChessType;
                ChessRecords.Add(position);

                if (XYFiveInARow(position) || SlopeFiveInARow(position))
                {
                    GameEnd = true;
                    Winner = currentPlayer.ChessType;
                }
                OneTurnFinish_Handle?.Invoke();
            }
            BlackChessPlayer.GameEnd(this);
            WhiteChessPlayer.GameEnd(this);
        }
        private bool XYFiveInARow(Vector2Int position)
        {
            int count = 0;
            ChessType chessType = Chessboard[position.Y, position.X];
            for (int x = position.X + 1; x < Chessboard.GetLength(1); x++)
            {
                if (Chessboard[position.Y, x] == chessType)
                    count++;
                else
                    break;
            }
            for (int x = position.X - 1; x >= 0; x--)
            {
                if (Chessboard[position.Y, x] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;

            count = 0;
            for (int y = position.Y + 1; y < Chessboard.GetLength(0); y++)
            {
                if (Chessboard[y, position.X] == chessType)
                    count++;
                else
                    break;
            }
            for (int y = position.Y - 1; y >= 0; y--)
            {
                if (Chessboard[y, position.X] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;

            return false;
        }
        private bool SlopeFiveInARow(Vector2Int position)
        {
            int count = 0;
            ChessType chessType = Chessboard[position.Y, position.X];
            int x, y;
            for (x = position.X + 1, y = position.Y + 1; x < Chessboard.GetLength(1) && y < Chessboard.GetLength(0); x++, y++)
            {
                if (Chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            for (x = position.X - 1, y = position.Y - 1; x > 0 && y > 0; x--, y--)
            {
                if (Chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;

            count = 0;
            for (x = position.X + 1, y = position.Y - 1; x < Chessboard.GetLength(1) && y > 0; x++, y--)
            {
                if (Chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            for (x = position.X - 1, y = position.Y + 1; x > 0 && y < Chessboard.GetLength(0); x--, y++)
            {
                if (Chessboard[y, x] == chessType)
                    count++;
                else
                    break;
            }
            count++;
            if (count >= 5)
                return true;
            return false;
        }

        internal float[] ConvertToNNFormat(ChessType aiChessType)
        {
            float[] value = new float[Defined.Size];
            for (int row = 0; row < Defined.Height; row++)
            {
                for (int column = 0; column < Defined.Width; column++)
                {
                    float t;
                    ChessType type = Chessboard[row, column];
                    if (type == ChessType.Empty)
                        t = Defined.EmptyChessValue;
                    else if (type == aiChessType)
                        t = Defined.AIChessValue;
                    else
                        t = Defined.OpponentChessValue;
                    value[row * Defined.Width + column] = t;
                }
            }
            return value;
        }
        internal void ConvertToLogFormat(StringBuilder stringBuilder)
        {
            stringBuilder.Append("  ");
            for (int column = 0; column < Defined.Width; column++)
                stringBuilder.Append(column).Append(" ");
            stringBuilder.AppendLine();
            for (int row = 0; row < Defined.Height; row++)
            {
                stringBuilder.Append(row).Append(" ");
                for (int column = 0; column < Defined.Width; column++)
                {
                    ChessType type = Chessboard[row, column];
                    if (type == ChessType.Black)
                        stringBuilder.Append("X ");
                    else if (type == ChessType.Empty)
                        stringBuilder.Append("_ ");
                    else if (type == ChessType.White)
                        stringBuilder.Append("O ");
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
                throw new Exception();
            lastChessPosition = ChessRecords[ChessRecords.Count - 1];
            ChessRecords.RemoveAt(ChessRecords.Count - 1);
            chessType = Chessboard[lastChessPosition.Y, lastChessPosition.X];
            Chessboard[lastChessPosition.Y, lastChessPosition.X] = ChessType.Empty;
        }
    }
}