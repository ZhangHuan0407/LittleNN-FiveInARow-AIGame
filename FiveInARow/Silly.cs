using LittleNN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiveInARow
{
    public class Silly : IController
    {
        public ChessType ChessType { get; set; }
        public readonly bool StopTrain;
        private int m_TrainTimes;
        public float LastLoss;
        public NeuralNetwork NeuralNetwork;
        public readonly GameLogic Notebook;

        public Silly(bool stopTrain)
        {
            StopTrain = stopTrain;
            Notebook = new GameLogic();
        }

        public void TryRecall()
        {
            string path = Path.Combine(Defined.ModelDirectory, "Silly.bin");
            if (File.Exists(path))
            {
                NeuralNetwork = NeuralNetwork.LoadFrom(path);
                Console.WriteLine("load nn model at: " + path);
            }
            else
            {
                List<Sequential> sequential = Sequential.CreateNew();
                sequential.Add(Sequential.Neural("input layer", Defined.NNInputSize));
                sequential.Add(Sequential.Activation("linear link", ActivationsFunctionType.LeakyReLU));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("output layer", Defined.Size));
                NeuralNetwork = new NeuralNetwork(sequential, 0.02f, 0.75f);
                Console.WriteLine("create new nn model");
            }
        }
        public void SaveMemory()
        {
            NeuralNetwork.SaveTo(Path.Combine(Defined.ModelDirectory, "Silly.bin"));
            m_TrainTimes = 0;
        }

        public void LogEvaluation(GameLogic gameLogic, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"Evaluation {ChessType}");
            stringBuilder.Append("     ");
            for (int column = 0; column < Defined.Width; column++)
                stringBuilder.Append(column).Append("   ");
            stringBuilder.AppendLine();
            float[] chessboard = gameLogic.ConvertToNNFormat(ChessType);
            float[] evaluation = NeuralNetwork.Forward(chessboard);
            for (int i = 0; i < evaluation.Length; i++)
            {
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                int value = (int)MathF.Floor(evaluation[i] * 100f);
                if (column == 0)
                    stringBuilder.Append(row).Append("  ");
                stringBuilder.Append(value.ToString().PadLeft(3)).Append(" ");
                if (column == Defined.Width - 1)
                    stringBuilder.AppendLine();
            }
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            float[] chessboard = gameLogic.ConvertToNNFormat(ChessType);
            position = new Vector2Int(-1, -1); // just for compile...
            float[] hillEvaluation = NeuralNetwork.Forward(chessboard);
            float maxEvaluation = 0f;
            for (int i = 0; i < hillEvaluation.Length; i++)
            {
                if (hillEvaluation[i] < maxEvaluation)
                    continue;
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                if (gameLogic.Chessboard[row, column] != ChessType.Empty)
                    continue;
                maxEvaluation = hillEvaluation[i];
                position.X = column;
                position.Y = row;
            }

            if (position.X == -1)
            {
                position = gameLogic.RandomPickEmptyPosition();
            }
        }
        public void LearnLastStep()
        {
            Notebook.Repentance(out Vector2Int chessPosition, out ChessType chessType);
            float[] chessboard = Notebook.ConvertToNNFormat(chessType);
            float[] hillEvaluation = NeuralNetwork.Forward(chessboard);
            float[] hillEvaluationCopy = new float[hillEvaluation.Length];
            Array.Copy(hillEvaluation, hillEvaluationCopy, hillEvaluation.Length);
            for (int i = 0; i < hillEvaluation.Length; i++)
            {
                // check this position is allowed by game rule
                if (hillEvaluation[i] > Defined.AIBelieveSelf)
                {
                    int row = i / Defined.Width;
                    int column = i % Defined.Width;
                    if (Notebook.Chessboard[row, column] != ChessType.Empty)
                        hillEvaluation[i] = Defined.AIAbortValue;
                }
            }
            hillEvaluation[chessPosition.Y * Defined.Width + chessPosition.X] = Defined.AIChooseValue;
            LastLoss = LossFuntion.MSELoss(hillEvaluationCopy, hillEvaluation);
            NeuralNetwork.OptimizerBackward(hillEvaluation);
            NeuralNetwork.OptimizerStep();
            if (m_TrainTimes++ > 1000)
                SaveMemory();
        }
        public void GameEnd(GameLogic gameLogic)
        {
            if (StopTrain)
                return;
        }
    }
}