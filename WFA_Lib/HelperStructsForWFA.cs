using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace WFA_Lib
{

    public enum Alphabet
    {
        _0 = 0,
        _1 = 1,
        _2 = 2,
        _3 = 3
    }

    public enum ColorRepresentation
    {
        RGB = 0,
        YUV = 1
    }

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
            var values = Values;
            Parallel.For(0, size, (index) =>
            {
                for (int j = 0; j < size; j++)
                {
                    image[index, j] = values[x + index, y + j];
                }
            });

            return new StateImage(image, size);
            }

        public MyVector ToVector()
        {
            var vector = new MyVector(new double[Size * Size]);

            var values = Values;
            int size = Size;
            Parallel.For(0, size, (index) =>
            {
                for (int j = 0; j < size; j++)
                {
                    vector.Values[index * size + j] = values[index, j];
                }
            });

            return vector;
        }
    }

    public struct State
    {
        public int ID { get; private set; }

        public StateImage Image { get => Images[0]; }
        List<StateImage> Images { get; }
        public int HighestResolution { get; private set; }

        private bool processed;
        public bool Processed { get => processed; }

        public State(int id, StateImage image)
        {
            ID = id;
            Images = new List<StateImage>();
            Images.Add(image);
            HighestResolution = image.Size;
            processed = false;
        }

        public StateImage GetImageWithSize(int newSize)
        {
            foreach (var image in Images)
            {
                if (image.Size == newSize)
                    return image;
            }
            double[,] img = ImageManipulator.Shrink(Images[0].Values, newSize, newSize);
            StateImage newImage = new StateImage(img, newSize);
            Images.Add(newImage);
            return newImage;
        }

        public static State CreateProcessedState(int id, StateImage image)
        {
            State newState = new State(id, image);
            newState.processed = true;

            return newState;
        }

        public double GetAverageIntensity()
        {
            foreach (var img in Images)
            {
                if (img.Size == 1)
                {
                    return img.Values[0, 0];
                }
            }

            var smallestImg = Images[Images.Count - 1];
            double average = 0;
            for (int i = 0; i < smallestImg.Size; i++)
            {
                for (int j = 0; j < smallestImg.Size; j++)
                {
                    average += smallestImg.Values[i, j];
                }
            }
            return average / (smallestImg.Size * smallestImg.Size);
        }
    }

    public struct Transition
    {
        public int FromStateId { get; private set; }
        public Alphabet Label { get; private set; }
        public int ToStateId { get; private set; }
        public double Weight { get; private set; }

        public Transition(int fromStateId, int toStateId, Alphabet label, double weight)
        {
            FromStateId = fromStateId;
            ToStateId = toStateId;
            Label = label;
            Weight = weight;
        }

        public override string ToString()
        {
            return $"{FromStateId}, {ToStateId}, {Label}, {Weight.ToString(CultureInfo.CreateSpecificCulture("en-US"))}";
        }
    }

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

    public struct ResolutionStruct
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ResolutionStruct(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}