using LittleNN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FiveInARow
{
    public class Silly : IController
    {
        public ChessType PlayerChessType { get; set; }
        private readonly GameLogic m_Notebook;
        private List<Vector2Int> m_PositionList;

        private float[] m_SillyEvaluationCopy;
        public float LastLoss { get; private set; }
        private int m_TrainTimes;
        public NeuralNetwork NeuralNetwork;

        public Silly()
        {
            PlayerChessType = ChessType.Empty;
            m_Notebook = new GameLogic();
            m_PositionList = new List<Vector2Int>();

            m_SillyEvaluationCopy = new float[Defined.Size];
            LastLoss = 0f;
            m_TrainTimes = 0;
            NeuralNetwork = null;
        }

        public bool TryRecall()
        {
            string path = Path.Combine(Defined.ModelDirectory, "Silly.bin");
            if (File.Exists(path))
            {
                NeuralNetwork = NeuralNetwork.LoadFrom(path);
                Console.WriteLine("load Silly nn model at: " + path);
                return true;
            }
            else
            {
                List<Sequential> sequential = Sequential.CreateNew();
                sequential.Add(Sequential.Neural("input layer", Defined.NNInputSize));
                sequential.Add(Sequential.Activation("linear link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("output layer", Defined.Size));
                NeuralNetwork = new NeuralNetwork(sequential, 0.02f, 0.75f);
                Console.WriteLine("create new Silly nn model");
                return false;
            }
        }
        public void SaveMemory()
        {
            NeuralNetwork.SaveTo(Path.Combine(Defined.ModelDirectory, "Silly.bin"));
            m_TrainTimes = 0;
        }

        internal void LogEvaluation(GameLogic gameLogic, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"Evaluation {PlayerChessType}");
            stringBuilder.Append("     ");
            for (int column = 0; column < Defined.Width; column++)
                stringBuilder.Append(column).Append("   ");
            stringBuilder.AppendLine();
            float[] chessboard = gameLogic.ConvertToNNFormat(PlayerChessType);
            float[] evaluation = NeuralNetwork.Forward(chessboard);
            MemoryBuffer.RevertFloatArray(chessboard);
            for (int i = 0; i < evaluation.Length; i++)
            {
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                if (column == 0)
                    stringBuilder.Append(row).Append("  ");
                float pValue = evaluation[i];
                if (pValue < Defined.AIChooseValue)
                {
                    string content = ((int)MathF.Floor(pValue * 10f)).ToString();
                    stringBuilder.Append(("." + content).PadLeft(3));
                }
                else
                {
                    string content = ((int)MathF.Floor(pValue * 100f)).ToString();
                    stringBuilder.Append(content.PadLeft(3));
                }
                stringBuilder.Append(" ");
                if (column == Defined.Width - 1)
                    stringBuilder.AppendLine();
            }
        }

        public void Play(GameLogic gameLogic, out OneStep oneStep)
        {
            if (Defined.Random.NextDouble() < Defined.SillyMakeMistake)
            {
                oneStep = new SillyOneStep(gameLogic.RandomPickEmptyPosition(), true, true);
                return;
            }
            throw new NotImplementedException();
            float[] chessboard = gameLogic.ConvertToNNFormat(PlayerChessType);
            float[] sillyEvaluation = NeuralNetwork.Forward(chessboard);
            MemoryBuffer.RevertFloatArray(chessboard);
            m_PositionList.Clear();
            for (int i = 0; i < sillyEvaluation.Length; i++)
            {
                if (sillyEvaluation[i] < Defined.AIChooseValue)
                    continue;
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                if (gameLogic.Chessboard[row, column] != ChessType.Empty)
                    continue;
                m_PositionList.Add(new Vector2Int(column, row));
            }

            if (m_PositionList.Count > 0)
                oneStep = new SillyOneStep(m_PositionList[Defined.Random.Next() % m_PositionList.Count], false, false);
            else
                oneStep = new SillyOneStep(gameLogic.RandomPickEmptyPosition(), false, true);
        }

        public void GameEnd(GameLogic gameLogic)
        {
            if (gameLogic.Winner == PlayerChessType)
                return;
            m_Notebook.Copy(gameLogic);
            m_Notebook.Repentance(out _, out _);
            m_Notebook.Repentance(out OneStep lastStep, out _);
            TrainOneStep(m_Notebook);
        }

        public void TrainOneStep(GameLogic gameLogic)
        {
            float[] chessboard = gameLogic.ConvertToNNFormat(gameLogic.CurrentPlayer.PlayerChessType);
            float[] sillyEvaluation = NeuralNetwork.Forward(chessboard);
            MemoryBuffer.RevertFloatArray(chessboard);
            float[] target = new float[Defined.Size];
            for (int row = 0; row < Defined.Height; row++)
            {
                for (int column = 0; column < Defined.Width; column++)
                {
                    ChessType chess = gameLogic.Chessboard[row, column];
                    if (chess != ChessType.Empty)
                        continue;
                    for (int xOffset = 1; xOffset < 5 && InBox(column + xOffset, row); xOffset++)
                    {
                        var neighborChess = gameLogic.Chessboard[row, column + xOffset];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[row * Defined.Width + column + xOffset] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int xOffset = -4; xOffset < 0 && InBox(column + xOffset, row); xOffset++)
                    {
                        var neighborChess = gameLogic.Chessboard[row, column + xOffset];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[row * Defined.Width + column + xOffset] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int yOffset = 1; yOffset < 5 && InBox(column, row + yOffset); yOffset++)
                    {
                        var neighborChess = gameLogic.Chessboard[row + yOffset, column];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[(row + yOffset) * Defined.Width + column] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int yOffset = -4; yOffset < 0 && InBox(column, row + yOffset); yOffset++)
                    {
                        var neighborChess = gameLogic.Chessboard[row + yOffset, column];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[(row + yOffset) * Defined.Width + column] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int xOffset = 1, yOffset = 1; xOffset < 5 && InBox(column + xOffset, row + yOffset); xOffset++, yOffset++)
                    {
                        var neighborChess = gameLogic.Chessboard[row + yOffset, column + xOffset];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[(row + yOffset) * Defined.Width + column + xOffset] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int xOffset = -4, yOffset = -4; xOffset < 0 && InBox(column + xOffset, row + yOffset); xOffset++, yOffset++)
                    {
                        var neighborChess = gameLogic.Chessboard[row, column + xOffset];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[(row + yOffset) * Defined.Width + column + xOffset] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int xOffset = 1, yOffset = 4; xOffset < 5 && InBox(column + xOffset, row + yOffset); xOffset++, yOffset--)
                    {
                        var neighborChess = gameLogic.Chessboard[row + yOffset, column + xOffset];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[(row + yOffset) * Defined.Width + column + xOffset] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    for (int xOffset = -4, yOffset = -1; xOffset < 0 && InBox(column + xOffset, row + yOffset); xOffset++, yOffset--)
                    {
                        var neighborChess = gameLogic.Chessboard[row, column + xOffset];
                        if (neighborChess == ChessType.Empty)
                        {
                            target[(row + yOffset) * Defined.Width + column + xOffset] += 0.2f;
                            break;
                        }
                        else if (neighborChess != chess)
                            break;
                    }
                    static bool InBox(int cooX, int cooY)
                    {
                        return cooX >= 0 && cooX < Defined.Width && cooY >= 0 && cooY < Defined.Height;
                    }
                }
            }
            for (int row = 0; row < Defined.Height; row++)
            {
                for (int column = 0; column < Defined.Width; column++)
                {
                    ChessType chess = gameLogic.Chessboard[row, column];
                    int index = row * Defined.Width + column;
                    float value = target[index];
                    if (chess == ChessType.Empty)
                    {
                        value += Defined.AIBelieveRuleAllow;
                        target[index] = Math.Min(value, 1f);
                    }
                }
            }
            LastLoss = LossFuntion.MSELoss(sillyEvaluation, target);
            NeuralNetwork.OptimizerBackward(target);
            NeuralNetwork.OptimizerStep();
            if (m_TrainTimes++ > 30000)
                SaveMemory();
        }
    }
}