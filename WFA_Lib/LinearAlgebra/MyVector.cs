using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using System;

namespace WFA_Lib.LinearAlgebra
{
    public struct MyVector
    {
        public Vector<double> Values { get => _values; }

        private readonly Vector<double> _values;
        public int Height { get => _values.Length; }

        public MyVector(double[] values)
        {
            this._values = Vector.Create(values);
        }

        public MyVector(Vector<double> values)
        {
            _values = values;
        }

        private MyVector(DenseMatrix<double> values)
        {
            _values = Vector.Create(values.ToArray());
        }

        public static MyVector operator *(MyMatrix matrix, MyVector vector)
        {
            if (matrix.Width != vector.Height)
                throw new ArgumentException("Matrix and vector do not have compatibile dimensions.");

            double[] vectorValues = vector.Values.ToArray();
            MyMatrix vectorAsMatrix = new MyMatrix(Matrix.Create(vector.Height, 1, vectorValues, MatrixElementOrder.RowMajor));

            var result = DenseMatrix<double>.Multiply(matrix.Values, vectorAsMatrix.Values);
            return new MyVector(result);
        }

        public static MyVector operator *(MyVector vector, MyMatrix matrix)
        {
            if (matrix.Height != vector.Height)
                throw new ArgumentException("Matrix and vector are not in compatibile dimensions");


            DenseMatrix<double> transpose = matrix.Values.Transpose().ToDenseMatrix();
            return new MyMatrix(transpose) * vector;

        }

        public static MyVector operator *(double value, MyVector vector)
        {
            var result = Vector.Multiply<double>(value, vector.Values);
            return new MyVector(result);
        }

        public static MyVector operator *(MyVector vector, double value)
        {
            return value * vector;
        }

        public static double operator *(MyVector v1, MyVector v2)
        {
            double result = 0;
            if (v1.Height != v2.Height)
                throw new ArgumentException("Vectors are not in compatibile dimensions");

            for (int i = 0; i < v1.Height; i++)
            {
                result += v1.Values[i] * v2.Values[i];
            }
            return result;

        }


        public static explicit operator double[](MyVector vector)
        {
            return vector.Values.ToArray(); ;
        }

        public static implicit operator MyVector(double[] values)
        {
            return new MyVector(values);
        }

        public bool Equals(MyVector vector)
        {
            for (int i = 0; i < Height; i++)
            {
                if (Values[i] != vector.Values[i])
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            return _values.ToString();
        }
    }
}
