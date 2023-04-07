using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiveInARow
{
    internal static class Program
    {
        public static bool Infinite;
        
        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine(Defined.NNInputSize);
            ReadArguments(args);
            WoodenManTrainSilly();
            //HumanPlayWithSilly();
            //SillyPlayWithSilly();
        }

        private static void ReadArguments(string[] args)
        {
            if (Environment.UserInteractive)
            {
                Infinite = false;
                // guess you are running in visual studio
                Defined.ModelDirectory = "../../../Model";
            }
            else
            {
                Infinite = true;
                Defined.ModelDirectory = "./";
            }
            if (!Directory.Exists(Defined.ModelDirectory))
                throw new DirectoryNotFoundException(Defined.ModelDirectory);
            
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
        private static void WoodenManTrainSilly()
        {
            Console.WriteLine("WoodenManTrainSilly");
            Silly silly = new Silly();
            silly.TryRecall();

            WoodenMan blackWoodenMan = new WoodenMan();
            WoodenMan whiteWoodenMan = new WoodenMan();
            int max = Infinite ? int.MaxValue : 300;
            for (int i = 0; i < max; i++)
            {
                float lossTotal = 0f;
                int trainCount = 0;
                foreach ((Vector2Int[] blackPositions, Vector2Int[] whitePositions) wrapper in WoodenMan.EnumAll5InARowPattern())
                {
                    GameLogic gameLogic = new GameLogic();
                    gameLogic.BlackChessPlayer = blackWoodenMan;
                    blackWoodenMan.Positions = wrapper.blackPositions;
                    gameLogic.WhiteChessPlayer = whiteWoodenMan;
                    whiteWoodenMan.Positions = wrapper.whitePositions;
                    gameLogic.OneTurnFinish_Handle += () =>
                    {
                        if (gameLogic.Winner == ChessType.Empty)
                        {
                            silly.TrainOneStep(gameLogic);
                            lossTotal += silly.LastLoss;
                            trainCount++;
                        }
                    };
                    gameLogic.PlayToEnd();
                }
                if (i % 50 == 49)
                {
                    Console.WriteLine($"{i}, {DateTime.Now}, average loss: {lossTotal / trainCount}");
                }
            }
            silly.SaveMemory();
            Console.WriteLine("train finish");
        }
        private static void SillyPlayWithSilly()
        {
            Console.WriteLine("SillyPlayWithSilly");
            Silly whiteSilly = new Silly();
            whiteSilly.TryRecall();
            Silly blackSilly = new Silly();
            blackSilly.NeuralNetwork = whiteSilly.NeuralNetwork;

            GameLogic gameLogic = new GameLogic()
            {
                BlackChessPlayer = blackSilly,
                WhiteChessPlayer = whiteSilly,
            };
            gameLogic.OneTurnFinish_Handle += () =>
            {
                OneStep lastStep = gameLogic.StepRecords[gameLogic.StepRecords.Count - 1];
                if (lastStep is SillyOneStep sillyOneStep)
                    Console.WriteLine(sillyOneStep.ToString());
                StringBuilder stringBuilder = new StringBuilder();
                gameLogic.ConvertToLogFormat(stringBuilder);
                Console.WriteLine(stringBuilder.ToString());
                Console.WriteLine();
                if (gameLogic.ToOpponent(gameLogic.CurrentPlayer) is Silly silly)
                {
                    Console.WriteLine(silly.GetType().Name);
                    stringBuilder.Clear();
                    silly.LogEvaluation(gameLogic, stringBuilder);
                    Console.WriteLine(stringBuilder.ToString());
                }
                Console.ReadLine();
            };
            gameLogic.PlayToEnd();
        }
        private static void HumanPlayWithSilly()
        {
            Silly whiteSilly = new Silly();
            whiteSilly.TryRecall();
            GameLogic gameLogic = new GameLogic()
            {
                BlackChessPlayer = new HumanPlayer(),
                WhiteChessPlayer = whiteSilly,
            };
            gameLogic.OneTurnFinish_Handle += () =>
            {
                OneStep lastStep = gameLogic.StepRecords[gameLogic.StepRecords.Count - 1];
                if (lastStep is SillyOneStep sillyOneStep)
                    Console.WriteLine(sillyOneStep.ToString());
                if (gameLogic.ToOpponent(gameLogic.CurrentPlayer) is Silly silly)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    silly.LogEvaluation(gameLogic, stringBuilder);
                    Console.WriteLine(gameLogic.CurrentPlayer.GetType().Name);
                    Console.WriteLine(stringBuilder.ToString());

                    //stringBuilder.Clear();
                    //for (int i = 0; i < Defined.Height; i++)
                    //{
                    //    stringBuilder.Append(" ");
                    //    for (int j = 0; j < Defined.Width; j++)
                    //    {
                    //        bool t = silly.NearlyAnyChess(gameLogic.Chessboard, j, i, ChessType.White);
                    //        stringBuilder.Append(t).Append("  ");
                    //    }
                    //    stringBuilder.AppendLine();
                    //}
                    //Console.WriteLine(stringBuilder);
                }
            };
            gameLogic.PlayToEnd();
        }
    }
}