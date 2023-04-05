using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public class Silly : IController
    {
        public ChessType ChessType { get; set; }
        private HashSet<Vector2Int> m_EmptyPosition;

        public Silly()
        {
            m_EmptyPosition = new HashSet<Vector2Int>();
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            m_EmptyPosition.Clear();
            for (int y = 0; y < gameLogic.Chessboard.GetLength(0); y++)
            {
                for (int x = 0; x < gameLogic.Chessboard.GetLength(1); x++)
                {
                    if (gameLogic.Chessboard[y, x] == ChessType.Empty)
                    {
                        m_EmptyPosition.Add(new Vector2Int(x, y));
                        if (IsSameChess(x + 1, y) ||
                            IsSameChess(x, y + 1) ||
                            IsSameChess(x - 1, y) ||
                            IsSameChess(x, y - 1))
                        {
                            position = new Vector2Int(x, y);
                            return;
                        }
                    }
                }
            }
            bool IsSameChess(int checkX, int checkY)
            {
                if (checkX >= 0 && checkY >= 0 && checkX >= Defined.Width && checkY >= Defined.Height)
                {
                    if (gameLogic.Chessboard[checkX, checkY] == ChessType)
                        return true;
                }
                return false;
            }
            position = m_EmptyPosition.First();
        }
        public void GameEnd(GameLogic gameLogic)
        {
        }
    }
}