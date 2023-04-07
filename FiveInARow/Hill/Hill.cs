using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LittleNN;

namespace FiveInARow
{
    public class Hill : IController
    {
        public ChessType PlayerChessType { get; set; }
        private List<HillChooseWeight> m_HighExpectationList;
        private List<HillChooseWeight> m_LowExpectationList;

        public float LastLoss { get; private set; }
        private int m_TrainTimes;
        public NeuralNetwork NeuralNetwork;
        public int Age { get; }
        private readonly bool m_LearningAbility;

        public Hill(NeuralNetwork neuralNetwork, int age, bool learningAbility)
        {
            PlayerChessType = ChessType.Empty;
            m_HighExpectationList = new List<HillChooseWeight>(Defined.Size);
            m_LowExpectationList = new List<HillChooseWeight>(Defined.Size);

            LastLoss = 0f;
            m_TrainTimes = 0;
            NeuralNetwork = neuralNetwork;
            Age = age;
            m_LearningAbility = learningAbility;
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

        public void Play(GameLogic gameLogic, out OneStep oneStep) =>
            Play(gameLogic, PlayerChessType, Defined.HillMakeMistake, out oneStep);
        public void Play(GameLogic gameLogic, ChessType chessType, float mistake, out OneStep oneStep)
        {
            if (Defined.Random.NextDouble() < mistake)
            {
                oneStep = new HillOneStep(gameLogic.RandomPickEmptyPosition());
                return;
            }
            float[] chessboard = gameLogic.ConvertToNNFormat(chessType);
            float[] sillyEvaluation = NeuralNetwork.Forward(chessboard);
            MemoryBuffer.RevertFloatArray(chessboard);
            m_HighExpectationList.Clear();
            m_LowExpectationList.Clear();
            for (int i = 0; i < sillyEvaluation.Length; i++)
            {
                if (sillyEvaluation[i] < Defined.AIBelieveRuleAllow)
                    continue;
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                if (gameLogic.Chessboard[row, column] != ChessType.Empty)
                    continue;
                float pValue = sillyEvaluation[i];
                if (pValue > Defined.AIChooseValue)
                {
                    int weight = (int)((pValue - Defined.AIChooseValue) / (1f - Defined.AIChooseValue) * 100f);
                    if (weight < 1)
                        weight = 1;
                    m_HighExpectationList.Add(new HillChooseWeight(weight, new Vector2Int(column, row)));
                }
                else
                {
                    int weight = (int)((pValue - Defined.AIBelieveRuleAllow) / (Defined.AIChooseValue - Defined.AIBelieveRuleAllow) * 100f);
                    if (weight < 1)
                        weight = 1;
                    m_LowExpectationList.Add(new HillChooseWeight(weight, new Vector2Int(column, row)));
                }
            }

            List<HillChooseWeight>? selectList = null;
            if (m_HighExpectationList.Count > 0)
                selectList = m_HighExpectationList;
            if (m_LowExpectationList.Count > 0)
            {
                if (selectList is null)
                    selectList = m_LowExpectationList;
                else if (Defined.Random.NextDouble() < 0.1d)
                    selectList = m_LowExpectationList;
            }
            if (selectList != null)
            {
                HillChooseWeight hillChoose = selectList.PickOne();
                oneStep = new HillOneStep(hillChoose.Position);
            }
            else
                oneStep = new HillOneStep(gameLogic.RandomPickEmptyPosition());
        }
        public void GameEnd(GameLogic gameLogic)
        {
            if (!m_LearningAbility)
                return;

            Notebook notebook = MemoryBuffer.RentNotebook();
            notebook.Copy(gameLogic);
            notebook.Deepth = 1;

            if (gameLogic.Winner == PlayerChessType)
                OptimizeLastStep(notebook);
            else
            {
                notebook.Repentance(out _, out _);
                OptimizeLastStep(notebook);
            }
        }
        private bool OptimizeLastStep(Notebook notebook)
        {
            if (notebook.Deepth >= Defined.DeepthMax)
                return false;
            notebook.Repentance(out OneStep lastStep, out ChessType chessType);
            float[] chessboard = notebook.ConvertToNNFormat(chessType);
            float[] evaluation = NeuralNetwork.Forward(chessboard);
            List<Vector2Int> list = notebook.TryGetBestStep(PlayerChessType);
            HashSet<Vector2Int> set = new HashSet<Vector2Int>(list);
            if (list.Count > 0)
            {
                for (int i = 0; i < evaluation.Length; i++)
                {
                    int row = i / Defined.Width;
                    int column = i % Defined.Width;
                    float value = evaluation[i];
                    if (set.Contains(new Vector2Int(column, row)))
                    {
                        if (value < Defined.AIChooseValue)
                            value = (value + Defined.PickValue) / 2f;
                    }
                    else if(notebook.Chessboard[row, column] != ChessType.Empty)
                    {
                        if (value > Defined.AIBelieveRuleAllow)
                            value = (value + Defined.AIAbortValue) / 2f;
                    }
                    else
                    {
                        if (value > Defined.AIChooseValue)
                            value = (value + Defined.AIBelieveRuleAllow) / 2f;
                    }
                }
                return true;
            }
            Play(notebook, notebook.CurrentPlayer.PlayerChessType, Defined.HillRandomAttempt, out OneStep oneStep);
            if (oneStep.Position == lastStep.Position)
                return false;
            throw new NotImplementedException();
        }
    }
}