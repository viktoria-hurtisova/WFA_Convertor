using System;

namespace WFA_Lib.HelperStructs
{
    public struct Coordinates
    {
        public int X { get; }
        public int Y { get; }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static explicit operator Coordinates(Word word)
        {
            int x = 0;
            int y = 0;

            int power = word.Values.Count;
            int size = (int)Math.Pow(2, power - 1);

            foreach (var letter in word.Values)
            {
                switch (letter)
                {
                    case Alphabet._0:
                        //x += size;
                        y += size;
                        size /= 2;
                        break;
                    case Alphabet._1:
                        x += size;
                        y += size;
                        size /= 2;
                        break;
                    case Alphabet._2:
                        //x += size;
                        //y += size;
                        size /= 2;
                        break;
                    case Alphabet._3:
                        x += size;
                        //y += size;
                        size /= 2;
                        break;
                    default:
                        break;
                }
            }

            return new Coordinates(x, y);
        }
    }
}
