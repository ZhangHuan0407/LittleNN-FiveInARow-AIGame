using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public class Silly : IController
    {
        public Vector2Int[] Positions;

        public Silly()
        {
        }

        public ChessType ChessType { get; set; }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            int index = gameLogic.ChessRecords.Count / 2;
            position = Positions[index];
        }
        public void GameEnd(GameLogic gameLogic)
        {
        }

        private static Random random = new Random();
        public static IEnumerable<(Vector2Int[], Vector2Int[])> EnumAll5InARowPattern()
        {
            bool[] buffer = new bool[Defined.Size];

            //O _ _ _ _
            //O _ _ _ _
            //O _ _ _ _
            //O _ _ _ _
            //O _ _ _ _
            //_ _ _ _ _
            for (int row = 0; row < Defined.Height - 5 + 1; row++)
                for (int column = 0; column < Defined.Width; column++)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    Vector2Int[] whitePositions = new Vector2Int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        whitePositions[i] = new Vector2Int(column, row + i);
                        buffer[(row + i) * Defined.Width + column] = true;
                    }
                    Shuffle(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList();
                    yield return (blackPositions, whitePositions);
                }

            //O O O O O _
            //_ _ _ _ _ _
            //_ _ _ _ _ _
            //_ _ _ _ _ _
            //_ _ _ _ _ _
            for (int row = 0; row < Defined.Height; row++)
                for (int column = 0; column < Defined.Width - 5 + 1; column++)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    Vector2Int[] whitePositions = new Vector2Int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        whitePositions[i] = new Vector2Int(column + i, row);
                        buffer[row * Defined.Width + column + i] = true;
                    }
                    Shuffle(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList();
                    yield return (blackPositions, whitePositions);
                }

            //O _ _ _ _
            //_ O _ _ _
            //_ _ O _ _
            //_ _ _ O _
            //_ _ _ _ O
            for (int row = 0; row < Defined.Height - 5 + 1; row++)
                for (int column = 0; column < Defined.Width - 5 + 1; column++)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    Vector2Int[] whitePositions = new Vector2Int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        whitePositions[i] = new Vector2Int(column + i, row + i);
                        buffer[(row + i) * Defined.Width + column + i] = true;
                    }
                    Shuffle(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList();
                    yield return (blackPositions, whitePositions);
                }

            //_ _ _ _ O
            //_ _ _ O _
            //_ _ O _ _
            //_ O _ _ _
            //O _ _ _ _
            for (int row = 0; row < Defined.Height - 5 + 1; row++)
                for (int column = 0; column < Defined.Width - 5 + 1; column++)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    Vector2Int[] whitePositions = new Vector2Int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        whitePositions[i] = new Vector2Int(column + 4 - i, row + i);
                        buffer[(row + i) * Defined.Width + column + 4 - i] = true;
                    }
                    Shuffle(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList();
                    yield return (blackPositions, whitePositions);
                }

            Vector2Int[] RandomBlackList()
            {
                int blackIndex = 0;
                Vector2Int[] result = new Vector2Int[6];
                while (blackIndex < result.Length)
                {
                    int randomValue = random.Next() % Defined.Size;
                    int randomRow = randomValue / Defined.Width;
                    int randomColumn = randomValue % Defined.Width;
                    if (!buffer[randomRow * Defined.Width + randomColumn])
                    {
                        buffer[randomRow * Defined.Width + randomColumn] = true;
                        result[blackIndex++] = new Vector2Int(randomColumn, randomRow);
                    }
                }
                return result;
            }
            void Shuffle(Vector2Int[] array)
            {
                for (int i = 0; i < array.Length - 1; i++)
                {
                    int swapTarget = random.Next() % array.Length;
                    Vector2Int temp = array[swapTarget];
                    array[swapTarget] = array[i];
                    array[i] = temp;
                }
            }
        }
    }
}