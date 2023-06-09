﻿using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public static class Defined
    {
        public const int Width = 9;
        public const int Height = 9;
        public const int Size = Width * Height;
        // 442 length
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
        public const float AIBelieveRuleAllow = 0.2f;
        public const float AIChooseValue = 0.8f;

        public const float SillyMakeMistake = 0.1f;
        public const float HillMakeMistake = 0.01f;
        public const float HillRandomAttempt = 0.15f;
        public const float HillLowExpectationRatio = 0.1f;

        public const int DeepthMax = 3;

        public static Random Random = new Random();
    }
}