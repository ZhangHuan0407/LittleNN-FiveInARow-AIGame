using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public static class MemoryBuffer
    {
        private static List<float[]> m_FloatArrayBuffer;
        private static Stack<Notebook> m_NotebookBuffer;

        static MemoryBuffer()
        {
            m_FloatArrayBuffer = new List<float[]>();
            m_NotebookBuffer = new Stack<Notebook>();
        }

        public static float[] RentFloatArray(int length)
        {
            for (int i = m_FloatArrayBuffer.Count - 1; i >= 0; i--)
            {
                if (m_FloatArrayBuffer[i].Length == length)
                {
                    float[] item = m_FloatArrayBuffer[i];
                    Array.Clear(item, 0, item.Length);
                    m_FloatArrayBuffer.RemoveAt(i);
                    return item;
                }
            }
            return new float[length];
        }
        public static void RevertFloatArray(float[] array)
        {
            if (m_FloatArrayBuffer.Count > 15)
            {
                int random = Defined.Random.Next() % m_FloatArrayBuffer.Count;
                m_FloatArrayBuffer.RemoveAt(random);
            }
            m_FloatArrayBuffer.Add(array);
        }

        public static Notebook RentNotebook()
        {
            if (m_NotebookBuffer.Count > 0)
                return m_NotebookBuffer.Pop();
            else
                return new Notebook();
        }
        public static void RevertNotebook(Notebook notebook)
        {
            if (m_NotebookBuffer.Count < Defined.DeepthMax)
                m_NotebookBuffer.Push(notebook);
        }
    }
}