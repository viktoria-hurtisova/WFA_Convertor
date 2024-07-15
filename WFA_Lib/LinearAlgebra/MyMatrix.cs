using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;

namespace WFA_Lib.LinearAlgebra
{
    public struct MyMatrix
    {
        private readonly DenseMatrix<double> values;
        public DenseMatrix<double> Values { get => values; }
        public int Height { get => values.RowCount; }   // number of rows
        public int Width { get => values.ColumnCount; }    // number of columns

        public MyMatrix(double[,] values)
        {
            this.values = Matrix.Create(values);
        }

        public MyMatrix(double[] values, int height, int width)
        {
            this.values = Matrix.Create(height, width, values, MatrixElementOrder.RowMajor);
        }

        public MyMatrix(DenseMatrix<double> m)
        {
            this.values = m.Clone().ToDenseMatrix();
        }

        public MyMatrix(Matrix<double> m)
        {
            this.values = m.Clone().ToDenseMatrix();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        public static MyMatrix operator *(MyMatrix matrix1, MyMatrix matrix2)
        {
            var result = DenseMatrix<double>.Multiply(matrix1.values, matrix2.values);
            return new MyMatrix(result);
        }

        public static explicit operator double[](MyMatrix matrix)
        {
            return matrix.values.ToArray();
        }

        public static implicit operator MyMatrix(double[,] array)
        {
            return new MyMatrix(array);
        }

        public static MyMatrix operator +(MyMatrix matrix1, MyMatrix matrix2)
        {

            DenseMatrix<double> result = matrix1.values.Clone().ToDenseMatrix();
            result = DenseMatrix<double>.Add(matrix1.values, 1, matrix2.values, result);

            return new MyMatrix(result);
        }

        public double[] ToOneDimensionalArray()
        {
            double[] array = new double[Width * Height];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    array[i * Width + j] = Values[i, j];
                }
            }

            return array;
        }
        public MyMatrix Transpose()
        {
            MyMatrix result = new MyMatrix(values.Transpose());

            return result;
        }

        public override string ToString()
        {
            return values.ToString();
        }

    }

}
