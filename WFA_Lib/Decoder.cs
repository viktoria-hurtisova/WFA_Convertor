using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace WFA_Lib
{
    struct MidResult
    {
        public Vector Value { get; }
        public Word Address { get; }

        public MidResult(Vector c, Word w)
        {
            Value = c;
            Address = w;
        }
    }

    struct WFAStruct
    {
        public List<Matrix> TransitionMatrices { get; private set; }
        public Vector InitialDistribution { get; private set; }
        public Vector FinalDistribution { get; private set; }

        public WFAStruct(List<Matrix> matrices, Vector initDist, Vector finalDist)
        {
            TransitionMatrices = matrices;
            InitialDistribution = initDist;
            FinalDistribution = finalDist;
        }
    }
    public static class Decoder
    {
        private static ProgressBar progressBar;
        private static int totalNumOfTasks = 0;
        private static int totalNumOfTasksEnded = 0;
        static public void WFAToImage(string inputWFAFile, string imageName, int depth, ProgressBar pB)
        {
            progressBar = pB;

            // loading the wfa from file
            WFA wfaClass = new WFA(inputWFAFile);


            // initialization for decoding
            List<Matrix> transitionMatrices;
            try
            {
                transitionMatrices = wfaClass.CreateTransitionMatrices();

            }
            catch (Exception)
            {

                throw new FileLoadException("The input file cannot be loaded.");
            }
            var initialDist = new Vector(new double[wfaClass.NumberOfStates]);
            initialDist.Values[6] = 1;

            WFAStruct wfa = new WFAStruct(transitionMatrices, initialDist, wfaClass.FinalDistribution);

            Bitmap image = ToImage(wfaClass, wfa, depth);


            image.Save(imageName);

            image.Dispose();

        }

        static private int SumGeometricSequence(int a1, int r, int n)
        {
            return a1 * (int)(1 - Math.Pow(r, n)) / (1 - r);
        }

        private static Bitmap ToImage(WFA wfaClass, WFAStruct wfa, int depth)
        {
            int maxDim = Math.Max(wfaClass.Resolution.Height, wfaClass.Resolution.Width);
            int power = (int)Math.Ceiling(Math.Log2(maxDim));
            int size = (int)Math.Pow(2, power);

            int length;
            if (depth == 0 || depth >= power)
            {
                depth = power;
                length = (int)Math.Ceiling((decimal)power / 2);
            }
            else
            {
                length = (int)Math.Ceiling((decimal)depth / 2); // the depth of the quadree that will be calculated in the second half,
                                                                // (power - length) will be calculated in the first half
            }

            totalNumOfTasks = 3 * SumGeometricSequence(4, 4, power - length)                    //calculating first half
                                + SumGeometricSequence(4, 4, length)                            //calculating second half
                                + (int)Math.Pow(2, power * 2)                                   //multiplication of mid results
                                + wfaClass.Resolution.Height * wfaClass.Resolution.Width;       //building the image

            // Calculate 
            List<MidResult> firstHalfC1, firstHalfC2, firstHalfC3;
            (firstHalfC1, firstHalfC2, firstHalfC3) = CalculateFirstFalf(wfa, power - length);
            List<MidResult> secondHalf = CalculateSecondHalf(wfa, length);

            var resultC1 = MultiplyMidResults(firstHalfC1, secondHalf, (int)Math.Pow(2, power));
            var resultC2 = MultiplyMidResults(firstHalfC2, secondHalf, (int)Math.Pow(2, power));
            var resultC3 = MultiplyMidResults(firstHalfC3, secondHalf, (int)Math.Pow(2, power));

            if (depth < power)
            {
                int difference = power - depth;
                int ratio = (int)Math.Pow(2, difference);
                wfaClass.ChangeResolution(ratio);
            }

            return BuildPicture(wfaClass, resultC1, resultC2, resultC3);  
        }

        private static Bitmap BuildPicture(WFA wfaClass, double[,] resultsC1, double[,] resultsC2, double[,] resultsC3)
        {
            double value1, value2, value3;
            Color color;

            Color[,] image = new Color[wfaClass.Resolution.Height, wfaClass.Resolution.Width];

            for (int i = 0; i < wfaClass.Resolution.Height; i++)
            {
                for (int j = 0; j < wfaClass.Resolution.Width; j++)
                {
                    value1 = resultsC1[i, j] * 255;
                    value2 = resultsC2[i, j] * 255;
                    value3 = resultsC3[i, j] * 255;


                    value1 = Math.Max(Math.Min(value1, 255), 0);
                    value2 = Math.Max(Math.Min(value2, 255), 0);
                    value3 = Math.Max(Math.Min(value3, 255), 0);


                    color = Color.FromArgb((int)value1, (int)value2, (int)value3);


                    image[i, j] = color;
                }
            }

            progressBar.Report(0.98);   // we are done, just few final things, thats why it is like this

            return ImageManipulator.ArrayToImage(image);
        }

        /// <summary>
        /// It will initiate for each color component the calculation of the first half of the word
        /// </summary>
        /// <param name="wfa"></param>
        /// <param name="length">Length of the words</param>
        /// <returns></returns>
        private static (List<MidResult>, List<MidResult>, List<MidResult>) CalculateFirstFalf(WFAStruct wfa, int length)
        {

            Vector initialDistribution = wfa.InitialDistribution * wfa.TransitionMatrices[2];
            var resultsC1 = CalculateFirstHalf(initialDistribution, wfa.TransitionMatrices, length);

            initialDistribution = wfa.InitialDistribution * wfa.TransitionMatrices[0];
            var resultC2 = CalculateFirstHalf(initialDistribution, wfa.TransitionMatrices, length);

            initialDistribution = wfa.InitialDistribution * wfa.TransitionMatrices[1];
            var resultC3 = CalculateFirstHalf(initialDistribution, wfa.TransitionMatrices, length);

            // Calculate the first half for all colour components
            return (resultsC1, resultC2, resultC3);
        }

        /// <summary>
        /// Will calculate for all words w the value IA_w
        /// </summary>
        /// <param name="initialDistribution"></param>
        /// <param name="transitionMatrices"></param>
        /// <param name="length">Lengt of the words</param>
        /// <returns></returns>
        private static List<MidResult> CalculateFirstHalf(Vector initialDistribution, List<Matrix> transitionMatrices, int length)
        {
            List<MidResult> result = new List<MidResult>();
            Stack<MidResult> stack = new Stack<MidResult>();
            Vector v;
            Word w;
            MidResult midRes;

            //Initialization for the first quadrants
            foreach (int i in Enum.GetValues(typeof(Alphabet)))
            {
                v = initialDistribution * transitionMatrices[i];
                w = new Word((Alphabet)i);
                midRes = new MidResult(v, w);
                stack.Push(midRes);
            }

            while (stack.TryPop(out midRes))
            {
                if (midRes.Address.Length == length)
                {
                    result.Add(midRes);
                }
                else
                {
                    foreach (int i in Enum.GetValues(typeof(Alphabet)))
                    {
                        v = midRes.Value * transitionMatrices[i];
                        w = midRes.Address + (Alphabet)i;
                        stack.Push(new MidResult(v, w));
                    }
                }
            }

            if (totalNumOfTasks > 0) // that means that we are decoding
            {
                lock (progressBar)
                {
                    totalNumOfTasksEnded += SumGeometricSequence(4, 4, length);
                    progressBar.Report(totalNumOfTasksEnded / totalNumOfTasks);
                }
            }
            return result;
        }

        /// <summary>
        /// Calculate for all words w A_wF
        /// </summary>
        /// <param name="wfa">The automaton</param>
        /// <param name="length">lengt of the words</param>
        /// <returns>List of all words of length size with corresponding vector</returns>
        static private List<MidResult> CalculateSecondHalf(WFAStruct wfa, int length)
        {
            Stack<MidResult> stack = new Stack<MidResult>();
            Vector v;
            Word w;
            MidResult midRes;

            //Initialization
            foreach (int i in Enum.GetValues(typeof(Alphabet)))
            {
                v = wfa.TransitionMatrices[i] * wfa.FinalDistribution;
                w = new Word((Alphabet)i);
                midRes = new MidResult(v, w);
                stack.Push(midRes);
            }

            List<MidResult> result = new List<MidResult>();

            while (stack.TryPop(out midRes))
            {
                if (midRes.Address.Length == length)
                {
                    result.Add(midRes);
                }
                else
                {
                    foreach (int i in Enum.GetValues(typeof(Alphabet)))
                    {
                        v = wfa.TransitionMatrices[i] * midRes.Value;
                        w = (Alphabet)i + midRes.Address;
                        stack.Push(new MidResult(v, w));
                    }
                }
            }
            if (totalNumOfTasks > 0) // that means that we are decoding
            {
                lock (progressBar)
                {
                    totalNumOfTasksEnded += SumGeometricSequence(4, 4, length);
                    progressBar.Report(totalNumOfTasksEnded / totalNumOfTasks);
                }
            }

            return result;
        }

        /// <summary>
        /// Method for calculating base images
        /// </summary>
        /// <param name="wfaClass"></param>
        /// <param name="matrices">Transition Matrices for base images</param>
        public static void CreateBaseImages(WFA wfaClass, List<Matrix> matrices)
        {
            double[] initDist1 = { 1, 0, 0, 0, 0, 0 };      // for constatnt
            double[] initDistX = { 0, 1, 0, 0, 0, 0 };      // for x
            double[] initDistY = { 0, 0, 1, 0, 0, 0 };      // for y
            double[] initDistX2 = { 0, 0, 0, 1, 0, 0 };     // for x^2
            double[] initDistY2 = { 0, 0, 0, 0, 1, 0 };     // for y^2
            double[] initDistXY = { 0, 0, 0, 0, 0, 1 };     // for xy

            List<Vector> initDist = new List<Vector>() { initDist1, initDistX, initDistY, initDistX2, initDistY2, initDistXY };

            int maxDim = Math.Max(wfaClass.Resolution.Height, wfaClass.Resolution.Width);
            int power = (int)Math.Ceiling(Math.Log2(maxDim));
            int length = (int)Math.Ceiling((decimal)power / 2); // depth of the quadtree for one colour component
            int size = (int)Math.Pow(2, power);

            double[] finalDist = { 1, 0.5, 0.5, 0.25, 0.5, 0.5 };

            List<List<MidResult>> firstHalf = new List<List<MidResult>>();

            // Calculate 
            WFAStruct wfa = new WFAStruct(matrices, initDist1, finalDist);
            List<MidResult> secondHalf = CalculateSecondHalf(wfa, length);
            List<double[,]> results = new List<double[,]>();
            for (int i = 0; i < initDist.Count; i++)
            {
                wfa = new WFAStruct(matrices, initDist[i], finalDist);
                firstHalf.Add(CalculateFirstHalfForBase(wfa, power - length));
                results.Add(MultiplyMidResults(firstHalf[i], secondHalf, size));
            }

            StateImage image;
            State state;
            for (int i = 0; i < initDist.Count; i++)
            {
                image = new StateImage(results[i], size);
                state = State.CreateProcessedState(wfaClass.NumberOfStates, image);
                wfaClass.AddState(state);
            }
        }

        private static double[,] MultiplyMidResults(List<MidResult> firstHalf, List<MidResult> secondHalf, int size)
        {
            double value;
            Coordinates coor;
            double[,] image = new double[size, size];
            foreach (var sh in secondHalf)
            {
                for (int i = 0; i < firstHalf.Count; i++)
                {
                    value = firstHalf[i].Value * sh.Value;

                    coor = (Coordinates)(firstHalf[i].Address + sh.Address);

                    image[coor.X, coor.Y] = value;
                }
            }

            totalNumOfTasksEnded += size;
            lock (progressBar)
            {
                progressBar.Report(totalNumOfTasksEnded / totalNumOfTasks);
            }

            return image;
        }

        private static List<MidResult> CalculateFirstHalfForBase(WFAStruct wfa, int size)
        {
            Stack<MidResult> stack = new Stack<MidResult>();
            Vector v;
            Word w;
            MidResult midRes;

            //Initialization
            foreach (int i in Enum.GetValues(typeof(Alphabet)))
            {
                v = wfa.InitialDistribution * wfa.TransitionMatrices[i];
                w = new Word((Alphabet)i);
                midRes = new MidResult(v, w);
                stack.Push(midRes);
            }

            List<MidResult> result = new List<MidResult>();

            while (stack.TryPop(out midRes))
            {
                if (midRes.Address.Length == size)
                {
                    result.Add(midRes);
                }
                else
                {
                    foreach (int i in Enum.GetValues(typeof(Alphabet)))
                    {
                        v = midRes.Value * wfa.TransitionMatrices[i];
                        w = (Alphabet)i + midRes.Address;
                        stack.Push(new MidResult(v, w));
                    }
                }
            }

            return result;
        }
    }
}
