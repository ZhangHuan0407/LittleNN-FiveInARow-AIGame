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
            //WoodenManTrainSilly();
            //SillyPlayWithSilly();

            Silly whiteSilly = new Silly(true);
            whiteSilly.TryRecall();
            GameLogic gameLogic = new GameLogic()
            {
                BlackChessPlayer = new HumanPlayer(),
                WhiteChessPlayer = whiteSilly,
            };
            gameLogic.OneTurnFinish_Handle += () =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (gameLogic.ToOpponent(gameLogic.CurrentPlayer) is Silly silly)
                    silly.LogEvaluation(gameLogic, stringBuilder);
                Console.WriteLine(stringBuilder.ToString());
            };
            gameLogic.PlayToEnd();
            Console.ReadLine();
            Console.ReadLine();
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
            Console.WriteLine("SillyTestTrainHill");
            Silly hill = new Silly(false);
            hill.TryRecall();

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
                if (i % 50 == 49)
                {
                    Console.WriteLine($"{i}, {DateTime.Now}, average loss: {lossTotal / trainCount}");
                    hill.SaveMemory();
                }
            }
            hill.SaveMemory();
            Console.WriteLine("train finish");
        }
        private static void SillyPlayWithSilly()
        {
            Silly whiteSilly = new Silly(true);
            whiteSilly.TryRecall();
            Silly blackSilly = new Silly(true);
            blackSilly.NeuralNetwork = whiteSilly.NeuralNetwork;

            GameLogic gameLogic = new GameLogic()
            {
                BlackChessPlayer = blackSilly,
                WhiteChessPlayer = whiteSilly,
            };
            gameLogic.OneTurnFinish_Handle += () =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                gameLogic.ConvertToLogFormat(stringBuilder);
                Console.WriteLine(stringBuilder.ToString());
                Console.WriteLine();
                stringBuilder.Clear();
                if (gameLogic.ToOpponent(gameLogic.CurrentPlayer) is Silly silly)
                    silly.LogEvaluation(gameLogic, stringBuilder);
                Console.WriteLine(stringBuilder.ToString());
                Console.ReadLine();
            };
            gameLogic.PlayToEnd();
        }

    }
}