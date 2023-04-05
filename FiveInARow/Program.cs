using System.Text;

namespace FiveInARow
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            //HumanPlayWithSilly();
            //SillyPlayWithHill();
            SillyTrainHill();
            //NikulaTrainHill();
            //HumanPlayWithHill();
        }

        private static void HumanPlayWithNikula()
        {
            GameLogic gameLogic = new GameLogic();
            gameLogic.BlackChessPlayer = new HumanPlayer();
            Hill hill = new Hill();
            hill.ClearMemory();
            gameLogic.WhiteChessPlayer = hill;
            gameLogic.PlayToEnd();
            Console.WriteLine($"winner: {gameLogic.Winner}");
            Console.ReadLine();
        }
        private static void HumanPlayWithSilly()
        {
            GameLogic gameLogic = new GameLogic();
            gameLogic.BlackChessPlayer = new HumanPlayer();
            gameLogic.WhiteChessPlayer = new Silly();
            gameLogic.PlayToEnd();
            Console.WriteLine($"winner: {gameLogic.Winner}");
            Console.ReadLine();
        }
        private static void SillyPlayWithHill()
        {
            GameLogic gameLogic = new GameLogic();
            gameLogic.BlackChessPlayer = new Silly();
            Hill hill = new Hill();
            hill.ClearMemory();
            gameLogic.WhiteChessPlayer = hill;
            gameLogic.OneTurnFinish_Handle += () =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                gameLogic.ConvertToLogFormat(stringBuilder);
                Console.Clear();
                Console.WriteLine(stringBuilder);
                Console.ReadLine();
            };
            gameLogic.PlayToEnd();
            Console.WriteLine($"winner: {gameLogic.Winner}");
            Console.ReadLine();
        }
        private static void SillyTrainHill()
        {
            Console.WriteLine("SillyTrainHill");
            Silly silly = new Silly();
            Hill hill = new Hill();
            hill.ClearMemory();
            for (int i = 0; i < 5 * 1000; i++)
            {
                GameLogic gameLogic = new GameLogic();
                gameLogic.BlackChessPlayer = silly;
                gameLogic.WhiteChessPlayer = hill;
                gameLogic.PlayToEnd();

                if (i % 400 == 399)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    gameLogic.ConvertToLogFormat(stringBuilder);
                    Console.WriteLine(stringBuilder);
                    Console.WriteLine($"winner: {gameLogic.Winner}");
                }
            }
            Console.WriteLine("train finish");
            Console.ReadLine();
        }
        private static void HillTrainHill()
        {
            Console.WriteLine("HillTrainHill");
            Hill blackHill = new Hill();
            blackHill.ClearMemory();
            Hill whiteHill = new Hill();
            whiteHill.ClearMemory();
            for (int i = 0; i < 100 * 1000; i++)
            {
                GameLogic gameLogic = new GameLogic();
                gameLogic.BlackChessPlayer = blackHill;
                gameLogic.WhiteChessPlayer = whiteHill;
                gameLogic.PlayToEnd();

                if (i % 200 == 199 && i % (5 * 1000) == 0)
                {
                    Console.Clear();
                }
                if (i % 200 == 199)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    gameLogic.ConvertToLogFormat(stringBuilder);
                    Console.WriteLine(stringBuilder);
                    Console.WriteLine($"winner: {gameLogic.Winner}");
                }
            }
            Console.WriteLine("train finish");
            Console.ReadLine();
        }
    }
}