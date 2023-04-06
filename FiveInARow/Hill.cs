using LittleNN;
using System;
using System.Collections.Generic;
using System.IO;

namespace FiveInARow
{
    public class Hill : IController
    {
        public ChessType ChessType { get; set; }
        public readonly bool StopTrain;
        private int m_TrainTimes;
        public float LastLoss;
        private NeuralNetwork m_NeuralNetwork;
        //private List<Vector2Int> m_PositionList;
        //private Random m_Random;
        public readonly GameLogic Notebook;

        public Hill(bool stopTrain)
        {
            //m_PositionList = new List<Vector2Int>();
            StopTrain = stopTrain;
            //m_Random = new Random();
            Notebook = new GameLogic();
        }

        public void ClearMemory()
        {
            List<Sequential> sequential = Sequential.CreateNew();
            sequential.Add(Sequential.Neural("input layer", Defined.NNInputSize));
            sequential.Add(Sequential.Activation("linear link", ActivationsFunctionType.LeakyReLU));
            sequential.Add(Sequential.Neural("hidden layer", Defined.NNInputSize * 2));
            sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
            sequential.Add(Sequential.Neural("hidden layer", 200));
            sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
            sequential.Add(Sequential.Neural("output layer", Defined.Size));
            m_NeuralNetwork = new NeuralNetwork(sequential, 0.02f, 0.75f);
        }
        public void TryRecallOrClearMemory()
        {
            string path = Path.Combine(Defined.ModelDirectory, "hill.bin");
            if (File.Exists(path))
            {
                m_NeuralNetwork = NeuralNetwork.LoadFrom(path);
                Console.WriteLine("load nn model at: " + path);
            }
            else
            {
                ClearMemory();
                Console.WriteLine("create new nn model");
            }
        }
        public void SaveMemory()
        {
            m_NeuralNetwork.SaveTo(Path.Combine(Defined.ModelDirectory, "hill.bin"));
            m_TrainTimes = 0;
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            throw new NotImplementedException();
            //position = new Vector2Int(-1, -1); // just for compile...
            //float[] chessboard = gameLogic.ConvertToNNFormat(ChessType);
            //float[] hillEvaluation = m_NeuralNetwork.Forward(chessboard);
            //float maxEvaluation = 0f;
            //for (int i = 0; i < hillEvaluation.Length; i++)
            //{
            //    if (hillEvaluation[i] < maxEvaluation)
            //        continue;
            //    int row = i / Defined.Width;
            //    int column = i % Defined.Width;
            //    if (gameLogic.Chessboard[row, column] != ChessType.Empty)
            //        continue;
            //    maxEvaluation = hillEvaluation[i];
            //    position.X = column;
            //    position.Y = row;
            //}

            //if (maxEvaluation >= BelieveSelf)
            //{
            //    m_PositionList.Clear();
            //    for (int i = 0; i < chessboard.Length; i++)
            //    {
            //        int row = i / Defined.Width;
            //        int column = i % Defined.Width;
            //        if (gameLogic.Chessboard[row, column] == ChessType.Empty)
            //            m_PositionList.Add(new Vector2Int(column, row));
            //    }
            //    position = m_PositionList[m_Random.Next() % m_PositionList.Count];
            //}
        }
        public void LearnLastStep()
        {
            Notebook.Repentance(out Vector2Int chessPosition, out ChessType chessType);
            float[] chessboard = Notebook.ConvertToNNFormat(chessType);
            float[] hillEvaluation = m_NeuralNetwork.Forward(chessboard);
            float[] hillEvaluationCopy = new float[hillEvaluation.Length];
            Array.Copy(hillEvaluation, hillEvaluationCopy, hillEvaluation.Length);
            for (int i = 0; i < hillEvaluation.Length; i++)
            {
                // check this position is allowed by game rule
                if (hillEvaluation[i] > Defined.AIBelieveRuleAllow)
                {
                    int row = i / Defined.Width;
                    int column = i % Defined.Width;
                    if (Notebook.Chessboard[row, column] != ChessType.Empty)
                        hillEvaluation[i] = Defined.AIAbortValue;
                }
            }
            hillEvaluation[chessPosition.Y * Defined.Width + chessPosition.X] = Defined.AIChooseValue;
            LastLoss = LossFuntion.MSELoss(hillEvaluationCopy, hillEvaluation);
            m_NeuralNetwork.OptimizerBackward(hillEvaluation);
            m_NeuralNetwork.OptimizerStep();
        }
        public void GameEnd(GameLogic gameLogic)
        {
            if (StopTrain)
                return;
            if (m_TrainTimes++ > 2000)
                SaveMemory();
        }
    }
}