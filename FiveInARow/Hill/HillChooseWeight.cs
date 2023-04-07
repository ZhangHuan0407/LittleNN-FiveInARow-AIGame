using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public struct HillChooseWeight
    {
        public int Weight;
        public Vector2Int Position;

        public HillChooseWeight(int weight, Vector2Int position)
        {
            Weight = weight;
            Position = position;
        }
    }
    public static class HillChooseWeightExtension
    {
        public static HillChooseWeight PickOne(this List<HillChooseWeight> list)
        {
            int totalWeight = 0;
            for (int i = 0; i < list.Count; i++)
            {
                int weight = list[i].Weight;
                if (weight < 0)
                    throw new ArgumentException("HillChooseWeight.Weight less than 0");
                totalWeight += weight;
            }
            int value = Defined.Random.Next() % totalWeight;
            totalWeight = 0;
            for (int i = 0; i < list.Count; i++)
            {
                totalWeight += list[i].Weight;
                if (totalWeight >= value)
                    return list[i];
            }
            throw new Exception("HillChooseWeightExtension.PickOne bug");
        }
    }
}