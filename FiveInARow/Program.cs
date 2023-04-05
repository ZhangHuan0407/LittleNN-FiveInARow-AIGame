using System.Text;

namespace FiveInARow
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            HumanPlayWithSilly();
        }

        private static void HumanPlayWithSilly()
        {
            GameLogic gameLogic = new GameLogic();
            gameLogic.BlackChessPlayer = new HumanPlayer();
            gameLogic.WhiteChessPlayer = new Silly();
            gameLogic.PlayToEnd();
            Console.ReadLine();
        }
    }
}