using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiveInARow
{
    public class WoodenMan : IController
    {
        public Vector2Int[] Positions;
        public ChessType Chess { get; set; }

        public WoodenMan()
        {
        }

        public void Play(GameLogic gameLogic, out OneStep oneStep)
        {
            int index = gameLogic.StepRecords.Count / 2;
            oneStep = new OneStep(Positions[index]);
        }
        public void GameEnd(GameLogic gameLogic)
        {
        }

        public static IEnumerable<(Vector2Int[] blackPositions, Vector2Int[] whitePositions)> EnumAll5InARowPattern()
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
                    ShuffleWhiteList(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList(6);
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
                    ShuffleWhiteList(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList(6);
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
                    ShuffleWhiteList(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList(6);
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
                    ShuffleWhiteList(whitePositions);
                    Vector2Int[] blackPositions = RandomBlackList(6);
                    yield return (blackPositions, whitePositions);
                }

            Vector2Int[] RandomBlackList(int length)
            {
                int blackIndex = 0;
                Vector2Int[] result = new Vector2Int[length];
                while (blackIndex < result.Length)
                {
                    int randomValue = Defined.Random.Next() % Defined.Size;
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
            void ShuffleWhiteList(Vector2Int[] array)
            {
                int randomValue = Defined.Random.Next() % array.Length;
                if (randomValue == array.Length - 1)
                    return;
                Vector2Int temp = array[randomValue];
                array[randomValue] = array[randomValue + 1];
                array[randomValue + 1] = temp;
            }
        }

        internal static void WoodenManUnitTest()
        {
            Console.WriteLine("WoodenManUnitTest");
            WoodenMan blackWoodenMan = new WoodenMan();
            WoodenMan whiteWoodenMan = new WoodenMan();
            StringBuilder stringBuilder = new StringBuilder();
            using (Stream stream = new FileStream("WoodenManUnitTest.txt", FileMode.Create, FileAccess.Write))
            {
                foreach (var wrapper in WoodenMan.EnumAll5InARowPattern())
                {
                    Vector2Int[] blackPositions = wrapper.Item1;
                    Vector2Int[] whitePositions = wrapper.Item2;

                    GameLogic gameLogic = new GameLogic();
                    gameLogic.BlackChessPlayer = blackWoodenMan;
                    blackWoodenMan.Positions = blackPositions;
                    gameLogic.WhiteChessPlayer = whiteWoodenMan;
                    whiteWoodenMan.Positions = whitePositions;
                    gameLogic.PlayToEnd();
                    stringBuilder.Clear();
                    gameLogic.ConvertToLogFormat(stringBuilder);
                    for (int i = 0; i < whitePositions.Length; i++)
                        stringBuilder.Append(whitePositions[i]).Append("\t");
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            Console.WriteLine("finish");
        }
    }
}