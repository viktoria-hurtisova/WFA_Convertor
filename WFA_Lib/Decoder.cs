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
        public MyVector Value { get; }
        public Word Address { get; }

        public MidResult(MyVector c, Word w)
        {
            Value = c;
            Address = w;
        }
    }

    public static class Decoder
    {
        private static ProgressBar progressBar;
        private static int totalNumOfTasks = 0;
        private static int totalNumOfTasksFinished = 0;
        static public void WFAToImage(string inputWFAFile, string imageName, int depth, ProgressBar pB)
        {
            progressBar = pB;

            // initialization of WFA, loading transitions from file
            WFA wfa = new WFA(inputWFAFile);

            Bitmap image = ToImage(wfa, depth);
            image.Save(imageName);

            image.Dispose();
        }

        static private int SumGeometricSequence(int a1, int r, int n)
        {
            return a1 * (int)(1 - Math.Pow(r, n)) / (1 - r);
        }

        private static Bitmap ToImage(WFA wfa, int depth)
        {
            int maxDim = Math.Max(wfa.Resolution.Height, wfa.Resolution.Width);
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

            totalNumOfTasks = 3 * SumGeometricSequence(4, 4, power - length) * 100                    //calculating first half
                                + SumGeometricSequence(4, 4, length) * 100                            //calculating second half
                                + (int)Math.Pow(2, power * 2)                                   //multiplication of mid results
                                + wfa.Resolution.Height * wfa.Resolution.Width;       //building the image

            // Calculate the first and second half
            List<MidResult> firstHalfC1, firstHalfC2, firstHalfC3, secondHalf;
            (firstHalfC1, firstHalfC2, firstHalfC3, secondHalf) = CalculateMidResults(wfa, power, length);

            double[,] resultC1, resultC2, resultC3;
            (resultC1, resultC2, resultC3) = MultiplyMidResults(firstHalfC1, firstHalfC2, firstHalfC3, secondHalf, size);

            if (depth < power)
            {
                int difference = power - depth;
                int ratio = (int)Math.Pow(2, difference);
                wfa.ChangeResolution(ratio);
            }

            return BuildPicture(wfa, resultC1, resultC2, resultC3);
        }

        private static Bitmap BuildPicture(WFA wfaClass, double[,] resultsC1, double[,] resultsC2, double[,] resultsC3)
        {

            Color[,] image = new Color[wfaClass.Resolution.Height, wfaClass.Resolution.Width];

            Parallel.For(0, wfaClass.Resolution.Height, index =>
            {
                double value1, value2, value3;
                Color color;
                for (int j = 0; j < wfaClass.Resolution.Width; j++)
                {
                    value1 = resultsC1[index, j] * 255;
                    value2 = resultsC2[index, j] * 255;
                    value3 = resultsC3[index, j] * 255;


                    value1 = Math.Max(Math.Min(value1, 255), 0);
                    value2 = Math.Max(Math.Min(value2, 255), 0);
                    value3 = Math.Max(Math.Min(value3, 255), 0);


                    color = Color.FromArgb((int)value1, (int)value2, (int)value3);


                    image[index, j] = color;
                }
            });

            progressBar.Report(0.98);   // we are done, just few final things, thats why it is like this

            return ImageManipulator.ArrayToImage(image);
        }

        /// <summary>
        /// It will initiate for each color component (each quadrant) the calculation of the first half of the word
        /// </summary>
        /// <param name="wfa"></param>
        /// <param name="length">Length of the words</param>
        /// <returns></returns>
        private static (List<MidResult>, List<MidResult>, List<MidResult>, List<MidResult>) CalculateMidResults(WFA wfa, int power, int length)
        {
            MyVector initialDistribution = wfa.InitialDistribution * wfa.TransitionMatrices[2];
            var task1 = new Task<List<MidResult>>(() => CalculateMidResults(initialDistribution, wfa.TransitionMatrices, power - length, true));
            task1.Start();

            initialDistribution = wfa.InitialDistribution * wfa.TransitionMatrices[0];
            var task2 = new Task<List<MidResult>>(() => CalculateMidResults(initialDistribution, wfa.TransitionMatrices, power - length, true));
            task2.Start();

            initialDistribution = wfa.InitialDistribution * wfa.TransitionMatrices[1];
            var task3 = new Task<List<MidResult>>(() => CalculateMidResults(initialDistribution, wfa.TransitionMatrices, power - length, true));
            task3.Start();

            var taskSecondHalf = new Task<List<MidResult>>(() => CalculateMidResults(wfa.FinalDistribution, wfa.TransitionMatrices, length, false));
            taskSecondHalf.Start();

            var tasks = new Task[] { task1, task2, task3, taskSecondHalf };
            Task.WaitAll(tasks);

            return (task1.Result, task2.Result, task3.Result, taskSecondHalf.Result);
        }

        /// <summary>
        /// Will calculate for all words w the value IA_w
        /// </summary>
        /// <param name="initialDistribution"></param>
        /// <param name="transitionMatrices"></param>
        /// <param name="length">Lengt of the words</param>
        /// <returns></returns>
        private static List<MidResult> CalculateMidResults(MyVector distribution, List<MyMatrix> transitionMatrices, int length, bool calculatingFirstHalf)
        {
            List<MidResult> firsts = new List<MidResult>();
            List<MidResult> results = new List<MidResult>();
            MyVector v;
            Word w;
            MidResult midRes;

            //Initialization for the first quadrants
            foreach (int i in Enum.GetValues(typeof(Alphabet)))
            {
                v = calculatingFirstHalf ? distribution * transitionMatrices[i] : transitionMatrices[i] * distribution;
                w = new Word((Alphabet)i);
                midRes = new MidResult(v, w);
                firsts.Add(midRes);
            }

            /*/
            var tasks = new Task<List<MidResult>>[firsts.Count];
            for (int i = 0; i < firsts.Count; i++)
            {
                int index = i;
                var task = new Task<List<MidResult>>(() => Calculate(firsts[index], length, transitionMatrices, calculatingFirstHalf));
                task.Start();
                tasks[index] = task;
            }

            Task.WaitAll(tasks);

            foreach (var t in tasks)
            {
                results.AddRange(t.Result);
            }
            /*/

            foreach (var first in firsts)
            {
                var result = Calculate(first, length, transitionMatrices, calculatingFirstHalf);
                results.AddRange(result);
            }
            /**/

            return results;
            
                }

        private static List<MidResult> Calculate(MidResult first, int length, List<MyMatrix> transitionMatrices, bool calculatingFirstHalf)
        {
            List<MidResult> result = new List<MidResult>();
            Stack<MidResult> stack = new Stack<MidResult>();
            MyVector v;
            Word w;
            MidResult midRes;
            int numFinishedTasks = 0;

            stack.Push(first);

            while (stack.TryPop(out midRes))
            {
                if (midRes.Address.Length == length)
                { 
                    result.Add(midRes);
                    numFinishedTasks++;

                    if (numFinishedTasks == SumGeometricSequence(4, 4, length) / 16)
                    {
                        UpdateProgressBar(numFinishedTasks * 100);
                        numFinishedTasks = 0;
                    }
                }

                else
                {
                    foreach (int i in Enum.GetValues(typeof(Alphabet)))
                    {
                        v = calculatingFirstHalf ? (midRes.Value * transitionMatrices[i]) : (transitionMatrices[i] * midRes.Value);
                        w = calculatingFirstHalf ? (midRes.Address + (Alphabet)i) : ((Alphabet)i + midRes.Address);
                        stack.Push(new MidResult(v, w));
                    }
                }
            }

            return result;
        }

        private static (double[,], double[,], double[,]) MultiplyMidResults(List<MidResult> firstHalfC1, List<MidResult> firstHalfC2, List<MidResult> firstHalfC3, List<MidResult> secondHalf, int size)
        {

            var task1 = new Task<double[,]>(() => MultiplyMidResults(firstHalfC1, secondHalf, size));
            task1.Start();

            var task2 = new Task<double[,]>(() => MultiplyMidResults(firstHalfC2, secondHalf, size));
            task2.Start();

            var task3 = new Task<double[,]>(() => MultiplyMidResults(firstHalfC3, secondHalf, size));
            task3.Start();

            var tasks = new Task[] { task1, task2, task3 };
            Task.WaitAll(tasks);

            return (task1.Result, task2.Result, task3.Result);
        }
        /// <summary>
        /// Method for calculating base images
        /// </summary>
        /// <param name="wfa"></param>
        public static void CreateBaseImages(WFA wfa)
        {
            double[] initDist1 = { 1, 0, 0, 0, 0, 0 };      // for constatnt
            double[] initDistX = { 0, 1, 0, 0, 0, 0 };      // for x
            double[] initDistY = { 0, 0, 1, 0, 0, 0 };      // for y
            double[] initDistX2 = { 0, 0, 0, 1, 0, 0 };     // for x^2
            double[] initDistY2 = { 0, 0, 0, 0, 1, 0 };     // for y^2
            double[] initDistXY = { 0, 0, 0, 0, 0, 1 };     // for xy

            List<MyVector> initDist = new List<MyVector>() { initDist1, initDistX, initDistY, initDistX2, initDistY2, initDistXY };

            int maxDim = Math.Max(wfa.Resolution.Height, wfa.Resolution.Width);
            int power = (int)Math.Ceiling(Math.Log2(maxDim));
            int length = (int)Math.Ceiling((decimal)power / 2); // depth of the quadtree for one colour component
            int size = (int)Math.Pow(2, power);

            wfa.FinalDistribution = new double[] { 1, 0.5, 0.5, 0.25, 0.5, 0.5 };
            List<List<MidResult>> firstHalf = new List<List<MidResult>>();

            // Calculate 
            List<MidResult> secondHalf = CalculateMidResults(wfa.FinalDistribution, wfa.TransitionMatrices, length, false);
            List<double[,]> results = new List<double[,]>();

            for (int i = 0; i < initDist.Count; i++)
            {
                wfa.InitialDistribution = initDist[i];
                firstHalf.Add(CalculateFirstHalfForBase(wfa, power - length));
                results.Add(MultiplyMidResults(firstHalf[i], secondHalf, size));
            }

            for (int i = 0; i < initDist.Count; i++)
            {
                State state;
                StateImage image;
                image = new StateImage(results[i], size);
                state = State.CreateProcessedState(wfa.NumberOfStates, image);
                wfa.AddState(state);
            }
        }

        private static double[,] MultiplyMidResults(List<MidResult> firstHalf, List<MidResult> secondHalf, int size)
        {
            double[,] image = new double[size, size];

            Parallel.ForEach(secondHalf, sh =>
            {
                double value;
                Coordinates coor;
                for (int i = 0; i < firstHalf.Count; i++)
                {
                    value = firstHalf[i].Value * sh.Value;

                    coor = (Coordinates)(firstHalf[i].Address + sh.Address);

                    image[coor.X, coor.Y] = value;
                }

                UpdateProgressBar(firstHalf.Count);
            });

            return image;
        }

        private static List<MidResult> CalculateFirstHalfForBase(WFA wfa, int size)
        {
            List<MidResult> result = new List<MidResult>();
            Stack<MidResult> stack = new Stack<MidResult>();
            MyVector v;
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

        private static void UpdateProgressBar(int finishedTasksIncrease)
        {

            if (progressBar != null)
            {
                lock (progressBar)
                {
                    totalNumOfTasksFinished += finishedTasksIncrease;
                    progressBar.Report((double)totalNumOfTasksFinished / totalNumOfTasks);
                }
            }
        }

    }
}
