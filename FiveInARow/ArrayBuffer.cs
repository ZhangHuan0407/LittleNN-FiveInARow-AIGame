using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public static class ArrayBuffer
    {
        private static List<float[]> m_List;

        static ArrayBuffer()
        {
            m_List = new List<float[]>();
        }

        public static float[] Rent(int length)
        {
            for (int i = m_List.Count - 1; i >= 0; i--)
            {
                if (m_List[i].Length == length)
                {
                    float[] item = m_List[i];
                    Array.Clear(item, 0, item.Length);
                    m_List.RemoveAt(i);
                    return item;
                }
            }
            return new float[length];
        }
        public static void Revert(float[] array)
        {
            if (m_List.Count > 9)
            {
                int random = Defined.Random.Next() % m_List.Count;
                m_List.RemoveAt(random);
            }
            m_List.Add(array);
        }
    }
}