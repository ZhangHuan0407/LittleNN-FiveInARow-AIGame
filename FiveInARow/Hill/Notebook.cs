using System.Collections.Generic;

namespace FiveInARow
{
    public class Notebook : GameLogic
    {
        public int Deepth { get; set; }

        public Notebook() : base()
        {
        }

        public List<Vector2Int> TryGetBestStep(ChessType chessType)
        {
            List<Vector2Int> bestStep = new List<Vector2Int>();
            for (int row = 0; row < Defined.Height; row++)
                for (int column = 0; column < Defined.Width; column++)
                {
                    Vector2Int position = new Vector2Int(column, row);
                    if (XYFiveInARow(chessType, position) || SlopeFiveInARow(chessType, position))
                        bestStep.Add(position);
                }
            return bestStep;
        }
    }
}