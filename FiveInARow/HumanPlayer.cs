using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FiveInARow
{
    public class HumanPlayer : IController
    {
        public ChessType PlayerChessType { get; set; }
        private StringBuilder m_LogBuilder;

        public HumanPlayer()
        {
            m_LogBuilder = new StringBuilder();
        }

        public void Play(GameLogic gameLogic, out OneStep oneStep)
        {
            m_LogBuilder.Clear();
            gameLogic.ConvertToLogFormat(m_LogBuilder);
            Console.WriteLine(m_LogBuilder.ToString());
            Console.WriteLine();
            Console.WriteLine($"Chess type: {PlayerChessType}");
            Console.WriteLine($"Input position: x,y");
            string input = Console.ReadLine().Trim().Replace(" ", string.Empty);
            if (Regex.Match(input, "(?<X>[0-9]+),(?<Y>[0-9]+)") is Match match &&
                match.Success)
            {
                int x = int.Parse(match.Groups["X"].Value);
                int y = int.Parse(match.Groups["Y"].Value);
                oneStep = new OneStep(new Vector2Int(x, y));
                if (x < 0 || x >= Defined.Width ||
                    y < 0 || y >= Defined.Height)
                {
                    Play(gameLogic, out oneStep);
                }
            }
            else
                Play(gameLogic, out oneStep);
        }
        public void GameEnd(GameLogic gameLogic)
        {
            m_LogBuilder.Clear();
            gameLogic.ConvertToLogFormat(m_LogBuilder);
            Console.WriteLine(m_LogBuilder.ToString());
        }
    }
}