using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiveInARow
{
    internal static class Program
    {
        public static bool UseLogFile;
        public static bool Infinite;
        
        [STAThread]
        private static void Main(string[] args)
        {
            ReadArguments(args);
            //SillyTest();
            SillyTestTrainHill();
            //SillyPlayWithHill();
        }

        private static void ReadArguments(string[] args)
        {
            if (Environment.UserInteractive)
            {
                Infinite = false;
                // guess you are running in project
                Defined.ModelDirectory = "../../../Model";
            }
            else
            {
                Infinite = true;
                Defined.ModelDirectory = "./";
            }
            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i];
                if (argument.ToLowerInvariant().StartsWith("-model="))
                {
                    Defined.ModelDirectory = argument.Substring("-model=".Length);
                }
                if (argument.ToLowerInvariant().StartsWith("-infinite="))
                {
                    Infinite = bool.Parse(argument.Substring("-infinite=".Length));
                }
            }
            Console.WriteLine($"-model={Defined.ModelDirectory}");
            Console.WriteLine($"-infinite={Infinite}");
        }
        private static void SillyTest()
        {
            Silly blackSilly = new Silly();
            Silly whiteSilly = new Silly();
            StringBuilder stringBuilder = new StringBuilder();
            using (Stream stream = new FileStream("SillyTest.txt", FileMode.Create, FileAccess.Write))
            {
                foreach (var wrapper in Silly.EnumAll5InARowPattern())
                {
                    Vector2Int[] blackPositions = wrapper.Item1;
                    Vector2Int[] whitePositions = wrapper.Item2;

                    GameLogic gameLogic = new GameLogic();
                    gameLogic.BlackChessPlayer = blackSilly;
                    blackSilly.Positions = blackPositions;
                    gameLogic.WhiteChessPlayer = whiteSilly;
                    whiteSilly.Positions = whitePositions;
                    gameLogic.PlayToEnd();
                    stringBuilder.Clear();
                    gameLogic.ConvertToLogFormat(stringBuilder);
                    byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            Console.WriteLine("finish");
        }
        private static void SillyTestTrainHill()
        {
            Console.WriteLine("SillyTestTrainHill");
            Silly silly = new Silly();
            Hill hill = new Hill(false);
            hill.TryRecallOrClearMemory();

            Silly blackSilly = new Silly();
            Silly whiteSilly = new Silly();
            int max = Infinite ? int.MaxValue : 200;
            for (int i = 0; i < max; i++)
            {
                float lossTotal = 0f;
                int trainCount = 0;
                foreach (var wrapper in Silly.EnumAll5InARowPattern())
                {
                    Vector2Int[] blackPositions = wrapper.Item1;
                    Vector2Int[] whitePositions = wrapper.Item2;

                    GameLogic gameLogic = new GameLogic();
                    gameLogic.BlackChessPlayer = blackSilly;
                    blackSilly.Positions = blackPositions;
                    gameLogic.WhiteChessPlayer = whiteSilly;
                    whiteSilly.Positions = whitePositions;
                    gameLogic.OneTurnFinish_Handle += () =>
                    {
                        // Learn white chess only and ignore white chess first step
                        if (gameLogic.ChessRecords.Count % 2 == 0 &&
                            gameLogic.ChessRecords.Count > 2)
                        {
                            hill.Notebook.Copy(gameLogic);
                            hill.LearnLastStep();
                            lossTotal += hill.LastLoss;
                            trainCount++;
                        }
                    };
                    gameLogic.PlayToEnd();
                }
                if (i % 50 == 49 || Environment.UserInteractive)
                {
                    Console.WriteLine($"{i}, {DateTime.Now}, average loss: {lossTotal / trainCount}");
                    hill.SaveMemory();
                }
            }
            hill.SaveMemory();

            //StringBuilder stringBuilder = new StringBuilder();
            //for (int i = 0; i < lossAverageRecords.Count; i++)
            //    stringBuilder.Append(i + 1).Append(',');
            //stringBuilder.Length -= 1;
            //stringBuilder.AppendLine();
            //for (int i = 0; i < lossAverageRecords.Count; i++)
            //    stringBuilder.Append(lossAverageRecords[i]).Append(',');
            //stringBuilder.Length -= 1;
            //File.WriteAllText("HillLoss.csv", stringBuilder.ToString());
            Console.WriteLine("train finish");
        }
        private static void SillyPlayWithHill()
        {
            //GameLogic gameLogic = new GameLogic();
            //gameLogic.BlackChessPlayer = new Silly();
            //Hill hill = new Hill();
            //hill.ClearMemory();
            //gameLogic.WhiteChessPlayer = hill;
            //gameLogic.OneTurnFinish_Handle += () =>
            //{
            //    StringBuilder stringBuilder = new StringBuilder();
            //    gameLogic.ConvertToLogFormat(stringBuilder);
            //    Console.Clear();
            //    Console.WriteLine(stringBuilder);
            //    Console.ReadLine();
            //};
            //gameLogic.PlayToEnd();
            //Console.WriteLine($"winner: {gameLogic.Winner}");
            //Console.ReadLine();
        }
    }
}