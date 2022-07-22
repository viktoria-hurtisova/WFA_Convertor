using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Globalization;

namespace WFA_Lib
{
    public struct Matrix
    {
        public double[,] Values { get; }
        public int Height { get => Values.GetLength(0); }   // number of rows
        public int Width { get => Values.GetLength(1); }    // number of columns

        public Matrix(double[,] values)
        {
            Values = new double[values.GetLength(0), values.GetLength(1)];
            Array.Copy(values, Values, values.GetLength(0) * values.GetLength(1));
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.Width != matrix2.Height)
                throw new ArgumentException("Matrices are not of appropriet size");

            double[,] result = new double[matrix1.Height, matrix2.Width];
            alglib.rmatrixgemm(matrix1.Height, matrix2.Width, matrix1.Width, 1, (double[,])matrix1, 0, 0, 0, (double[,])matrix2, 0, 0, 0, 1, ref result, 0, 0);

            return result;
        }

        public static explicit operator double[,](Matrix matrix)
        {
            double[,] values = new double[matrix.Height, matrix.Width];
            Array.Copy(matrix.Values, values, matrix.Height * matrix.Width);
            return values;
        }

        public static implicit operator Matrix(double[,] array)
        {
            return new Matrix(array);
        }

        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.Height != matrix2.Height || matrix1.Width != matrix2.Width)
                throw new ArgumentException("Matrices are not of appropriet size");

            double[,] result = new double[matrix1.Height, matrix1.Width];

            for (int i = 0; i < matrix1.Height; i++)
            {
                for (int j = 0; j < matrix2.Width; j++)
                {
                    result[i, j] = matrix1.Values[i, j] + matrix2.Values[i, j];
                }
            }

            return new Matrix(result);
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
        public Matrix Transpose()
        {
            double[,] values = new double[Width, Height];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    values[j, i] = Values[i, j];
                }
            }

            return values;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < Height; i++)
            {
                sb.Append("{");
                for (int j = 0; j < Width; j++)
                {
                    sb.Append(Values[i, j].ToString(CultureInfo.CreateSpecificCulture("en-US")));
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2);
                sb.Append("},");
                sb.Append(Environment.NewLine);
            }
            sb.Remove(sb.Length - 3, 3);
            sb.Append("}");
            return sb.ToString();
        }

    }

    public struct Vector
    {
        public double[] Values { get; }
        public int Height { get => Values.GetLength(0); } // number of rows

        public Vector(double[] values)
        {
            int height = values.GetLength(0);
            Values = new double[height];
            Array.Copy(values, Values, height);
        }

        public Vector(Vector vector)
        {
            int height = vector.Height;
            Values = new double[height];
            Array.Copy(vector.Values, Values, height);
        }

        public static Vector operator *(Matrix matrix, Vector vector)
        {
            if (matrix.Width != vector.Height)
                throw new ArgumentException("Matrix and vector are not in compatibile dimensions");

            double[] result = new double[matrix.Height];
            alglib.rmatrixgemv(matrix.Height, matrix.Width, 1, (double[,])matrix, 0, 0, 0, (double[])vector, 0, 1, ref result, 0);

            return result;
        }

        public static Vector operator *(Vector vector, Matrix matrix)
        {
            if (matrix.Height != vector.Height)
                throw new ArgumentException("Matrix and vector are not in compatibile dimensions");

            double[] result = new double[matrix.Width];
            alglib.rmatrixgemv(matrix.Height, matrix.Width, 1, (double[,])matrix, 0, 0, 1, (double[])vector, 0, 1, ref result, 0);

            return result;
        }

        public static Vector operator *(double value, Vector vector)
        {
            double[] newVector = new double[vector.Height];
            for (int i = 0; i < vector.Height; i++)
            {
                newVector[i] = value * vector.Values[i];
            }

            return newVector;
        }

        public static Vector operator *(Vector vector, double value)
        {
            return value * vector;
        }

        public static double operator *(Vector v1, Vector v2)
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

        public static explicit operator double[](Vector vector)
        {
            double[] values = new double[vector.Height];
            Array.Copy(vector.Values, values, vector.Height);
            return values;
        }

        public static implicit operator Vector(double[] values)
        {
            return new Vector(values);
        }

        public bool Equals(Vector vector)
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
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (var num in Values)
            {
                sb.Append(num.ToString(CultureInfo.CreateSpecificCulture("en-US")));
                sb.Append(". ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");
            return sb.ToString();
        }
    }
}