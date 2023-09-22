using WFA_Lib.LinearAlgebra;

namespace WFA_Lib.HelperStructs
{
    public struct StateImage
    {
        public int Size { get; private set; }
        public double[,] Values { get; private set; }

        public StateImage(double[,] image, int resolution)
        {
            Size = resolution;
            Values = image;
        }

        public StateImage GetQuadrant(Alphabet letter)
        {
            int x = 0;
            int y = 0;
            int size = Size / 2;
            switch (letter)
            {
                case Alphabet._0:
                    y += size;
                    break;
                case Alphabet._1:
                    x += size;
                    y += size;
                    break;
                case Alphabet._2:
                    break;
                case Alphabet._3:
                    x += size;
                    break;
                default:
                    break;
            }

            double[,] image = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    image[i, j] = Values[x + i, y + j];
                }
            }
            return new StateImage(image, size);
        }

        public MyVector ToVector()
        {
            var vector = new MyVector(new double[Size * Size]);

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    vector.Values[i * Size + j] = Values[i, j];
                }
            }
            return vector;
        }
    }
}
