using System;

namespace FiveInARow
{
    public struct HillSection : IComparable<HillSection>
    {
        public string FilePath;
        public int Age;

        public int CompareTo(HillSection other) => Age.CompareTo(other.Age);
    }
}