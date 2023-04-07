using System;

namespace FiveInARow
{
    [Serializable]
    public enum ChessType : byte
    {
        Empty = 0,
        Black = 1,
        White = 2,
    }

    public static class ChessTypeExtension
    {
        public static ChessType ToOpponentChessType(this ChessType chessType)
        {
            if (chessType == ChessType.Empty)
                throw new Exception("ToOpponentChess(ChessType.Empty)");
            if (chessType == ChessType.Black)
                return ChessType.White;
            else if (chessType == ChessType.White)
                return ChessType.Black;
            throw new NotImplementedException("ToOpponentChess");
        }
    }
}