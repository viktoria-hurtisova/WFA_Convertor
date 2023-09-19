using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Globalization;
using Extreme.Mathematics.LinearAlgebra;
using Extreme.Mathematics;

namespace WFA_Lib
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

        /*
        public static explicit operator double[,](MyMatrix matrix)
        {
            double[,] values = new double[matrix.Height, matrix.Width];
            Array.Copy(matrix.Values, values, matrix.Height * matrix.Width);
            return values;
        }
        */

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