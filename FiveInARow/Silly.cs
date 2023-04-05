using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public class Silly : IController
    {
        public ChessType ChessType { get; set; }
        private List<Vector2Int> m_PositionList;
        private Random m_Random;

        public Silly()
        {
            m_PositionList = new List<Vector2Int>();
            m_Random = new Random();
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            if (gameLogic.ChessRecords.Count < 2)
            {
                int randomColumn = m_Random.Next() % Defined.Width;
                int randomRow = m_Random.Next() % Defined.Height;
                if (gameLogic.Chessboard[randomRow, randomColumn] == ChessType.Empty)
                {
                    position = new Vector2Int(randomColumn, randomRow);
                    return;
                }
            }

            m_PositionList.Clear();
            bool hasNeighborOnly = false;
            for (int y = 0; y < gameLogic.Chessboard.GetLength(0); y++)
            {
                for (int x = 0; x < gameLogic.Chessboard.GetLength(1); x++)
                {
                    if (gameLogic.Chessboard[y, x] == ChessType.Empty)
                    {
                        bool hasNeightbor = IsSameChess(x + 1, y) ||
                                            IsSameChess(x, y + 1) ||
                                            IsSameChess(x - 1, y) ||
                                            IsSameChess(x, y - 1);
                        if (!hasNeighborOnly && hasNeightbor)
                        {
                            hasNeighborOnly = hasNeightbor;
                            m_PositionList.Clear();
                        }
                        if (hasNeighborOnly && !hasNeightbor)
                            continue;
                        m_PositionList.Add(new Vector2Int(x, y));
                    }
                }
            }
            bool IsSameChess(int checkX, int checkY)
            {
                if (checkX >= 0 && checkY >= 0 && checkX < Defined.Width && checkY < Defined.Height)
                {
                    if (gameLogic.Chessboard[checkY, checkX] == ChessType)
                        return true;
                }
                return false;
            }
            position = m_PositionList[m_Random.Next() % m_PositionList.Count];
        }
        public void GameEnd(GameLogic gameLogic)
        {
        }
    }
}