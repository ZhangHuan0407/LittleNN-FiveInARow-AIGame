using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public static class Defined
    {
        public const int Width = 9;
        public const int Height = 9;
        public const int Size = Width * Height;
        public const int NNInputSize = /*ai chess ? 1 : 0*/Size + /*opponent chess ? 1 : 0*/Size +
                                       /*ai chess in 5 row or 5 column*/(Width - 5 + 1) * Height + Width * (Height - 5 + 1) +
                                       /*ai chess in 5 slope*/(Width - 5 + 1) * (Height - 5 + 1) * 2 +
                                       /*opponent chess in 5 row or 5 column*/(Width - 5 + 1) * Height + Width * (Height - 5 + 1) +
                                       /*opponent chess in 5 slope*/(Width - 5 + 1) * (Height - 5 + 1) * 2;

        public const float AIChessValue = 1f;
        public const float OpponentChessValue = 1f;
        public const float EmptyChessValue = 0f;

        public static string ModelDirectory = string.Empty;

        public const float PickValue = 1f;
        public const float AIAbortValue = 0f;
        public const float AIBelieveSelf = 0.2f;
        public const float AIChooseValue = 0.8f;

        public static Random Random = new Random();
    }
}