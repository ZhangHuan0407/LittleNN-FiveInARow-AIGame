using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FiveInARow
{
    public class HumanPlayer : IController
    {
        public ChessType ChessType { get; set; }
        private StringBuilder m_LogBuilder;

        public HumanPlayer()
        {
            m_LogBuilder = new StringBuilder();
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            m_LogBuilder.Clear();
            gameLogic.ConvertToLogFormat(m_LogBuilder);
            Console.WriteLine(m_LogBuilder.ToString());
            Console.WriteLine();
            Console.WriteLine($"Chess type: {ChessType}");
            Console.WriteLine($"Input position: x,y");
            string input = Console.ReadLine().Trim().Replace(" ", string.Empty);
            if (Regex.Match(input, "(?<X>[0-9]),(?<Y>[0-9])") is Match match &&
                match.Success)
            {
                int x = int.Parse(match.Groups["X"].Value);
                int y = int.Parse(match.Groups["Y"].Value);
                position.X = x;
                position.Y = y;
            }
            else
                Play(gameLogic, out position);
        }
        public void GameEnd(GameLogic gameLogic)
        {
            m_LogBuilder.Clear();
            gameLogic.ConvertToLogFormat(m_LogBuilder);
            Console.WriteLine(m_LogBuilder.ToString());
        }
    }
}