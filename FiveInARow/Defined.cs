using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public static class Defined
    {
        public const int Width = 9;
        public const int Height = 9;
        public const int Size = Width * Height;

        public const float AIChessValue = 1f;
        public const float HumanChessValue = 0f;
        public const float EmptyChessValue = 0.5f;

        public static string ModelDirectory = new DirectoryInfo("../../../Model").FullName.Replace("\\", "/");
    }
}