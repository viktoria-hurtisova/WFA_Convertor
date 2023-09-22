using System.Collections.Generic;

namespace WFA_Lib.HelperStructs
{
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
            Images = new List<StateImage> { image };
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
            State newState = new State(id, image) { processed = true };

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
}
