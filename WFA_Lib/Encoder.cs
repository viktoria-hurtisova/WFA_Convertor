using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WFA_Lib
{
    public class Encoder
    {
        private static ProgressBar progressBar;
        private static int totalNumOfSubsquaresProcessed;
        private static int totalNumOfSubsquares;

        public static void ImageToWFA(string inputImage, string wfaName, ProgressBar pB)
        {

            progressBar = pB;

            WFA wfa;
            Color[,] imageRGB;
            double[,] image;
            int originalImageWidth, originalImageHeight;

            try
            {
                using (var imageBM = new Bitmap(inputImage))
                {
                    // preparing the image
                    originalImageWidth = imageBM.Width;
                    originalImageHeight = imageBM.Height;
                    imageRGB = ImageManipulator.ToSquare(imageBM);
                    image = ImageManipulator.ConcatenateColourComponents(imageRGB);
                    MapTo01(image);
                }
            }
            catch (Exception)
            {

                throw new Exception("The input image cannot be loaded.");
            }

            // WFA initialization
            wfa = new WFA(new ResolutionStruct(originalImageWidth, originalImageHeight), ColorRepresentation.RGB);
            wfa.AddBaseStates();

            // add the input image as a first state
            StateImage stateImage = new StateImage(image, image.GetLength(0));
            State state = new State(wfa.NumberOfStates, stateImage);
            wfa.AddState(state);

            var length = (int)Math.Log2(image.GetLength(0));
            totalNumOfSubsquares = (int)(1 - Math.Pow(4, length)) / (1 - 4);    //sum of geometric sequence
            totalNumOfSubsquaresProcessed = 0;

            MakeWFA(wfa, state, double.PositiveInfinity, length);



            wfa.EncodeWFA(wfaName);
        }

        private static double MakeWFA(WFA wfa, State state, double max, int depth)
        {
            if (max <= 0 || depth == 0) // we cannot acieve better result and we cannot devide one pixel into quadrants
            {
                totalNumOfSubsquaresProcessed += (int)(1 - Math.Pow(4, depth)) / (1 - 4);
                return double.PositiveInfinity;
            }

            double cost = 0;
            double cost1, cost2;
            int currentNumOfStates;
            StateImage quadrant;
            State newState;
            List<Transition> transitions;

            foreach (int i in Enum.GetValues(typeof(Alphabet)))
            {
                quadrant = state.Image.GetQuadrant((Alphabet)i);    // we get the appropriet quadrant

                transitions = FindLinearCombinations(wfa, state, quadrant, (Alphabet)i, out cost1);    // linear combination calculations


                currentNumOfStates = wfa.NumberOfStates;

                // new state option
                newState = new State(wfa.NumberOfStates, quadrant);
                wfa.AddState(newState);
                wfa.AddTransition(new Transition(state.ID, newState.ID, (Alphabet)i, 1));

                cost2 = MakeWFA(wfa, newState, Math.Min(max - cost - 1, cost1 - 1), depth - 1);   // resursive call of ToWFA

                // comparing two variants
                if (cost1 < cost2)
                {
                    cost += cost1;
                    wfa.RemoveStates(currentNumOfStates);
                    wfa.RemoveTransitions(currentNumOfStates);
                    wfa.AddTransitions(transitions);
                }
                else
                {
                    cost += cost2;
                    wfa.SetStateAsProcessed(newState.ID);
                }

                progressBar.Report(++totalNumOfSubsquaresProcessed / (double)totalNumOfSubsquares);

                if (cost > max || (cost1 == double.PositiveInfinity && cost2 == double.PositiveInfinity))
                {
                    totalNumOfSubsquaresProcessed += (4 - i) * (int)(1 - Math.Pow(4, depth - 1)) / (1 - 4);
                    return double.PositiveInfinity;
                }
            }

            if (cost < max)
                return cost;
            else
                return double.PositiveInfinity;
        }

        private static List<Transition> FindLinearCombinations(WFA wfa, State parentState, StateImage quadrant, Alphabet label, out double cost)
        {
            List<Vector> imageVectors = new List<Vector>();
            List<int> statesUsed = new List<int>();
            StateImage stateImg;
            List<Transition> transitions = new List<Transition>();

            Vector b = quadrant.ToVector();
            Vector imageVector;
            double value;
            int indexer = 0;

            foreach (var state in wfa.States)
            {
                if (state.Processed && state.HighestResolution >= quadrant.Size)
                {
                    stateImg = state.GetImageWithSize(quadrant.Size);
                    statesUsed.Add(state.ID);
                    imageVector = stateImg.ToVector();

                    value = Ratio(b, imageVector);      //we want to check, if there exists real number such that value*imageVector = b

                    if (indexer < b.Height)
                    {
                        if (!double.IsNaN(value) && value != 0)
                        {
                            if (SquareError(imageVector * value, b) == 0)
                            {
                                transitions.Add(new Transition(parentState.ID, state.ID, label, value));
                                cost = 1;
                                return transitions;
                            }

                        }
                        indexer++;
                    }
                    imageVectors.Add(imageVector);
                }
            }

            //If there is no vector such that it is the same except for a multiple of a real number


            Matrix A = ConcatenateVectors(imageVectors);
            double[] x;
            int info;
            alglib.densesolverlsreport report;
            alglib.rmatrixsolvels((double[,])A, A.Height, A.Width, (double[])b, 0.0, out info, out report, out x);

            Vector newImage = A * (Vector)x;


            for (int i = 0; i < x.Length; i++)      // we will add transitions with non-zero weights
            {
                if (Math.Round(x[i], 15) != 0)
                {
                    transitions.Add(new Transition(parentState.ID, statesUsed[i], label, x[i]));
                }
            }

            if (SquareError(newImage, b) < 0.001)       // we will consider it a good linear combination if the square error of the new image and the original <1
            {
                cost = transitions.Count;
            }
            else
            {
                cost = double.PositiveInfinity;
            }

            return transitions;
        }

        private static double Ratio(Vector v1, Vector v2)
        {
            double sum1 = 0;
            double sum2 = 0;
            int nonZeroValues = 0;

            for (int i = 0; i < v1.Height; i++)
            {
                if (v1.Values[i] != 0 && v2.Values[i] != 0)
                    nonZeroValues++;
                else if (v1.Values[i] != 0 || v2.Values[i] != 0)    // one of them is zero and one is non-zero
                {
                    return double.NaN;
                }
                sum1 += v1.Values[i];
                sum2 = v2.Values[i];
            }

            if (sum2 == 0 || nonZeroValues == 0)
                return double.NaN;
            else
                return (sum1 / sum2) / nonZeroValues;
        }

        private static Matrix ConcatenateVectors(List<Vector> vectors)
        {
            int width = (vectors.Count > vectors[0].Height - 1) ? vectors[0].Height - 1 : vectors.Count;
            //int width = vectors.Count;
            Matrix matrix = new Matrix(new double[vectors[0].Height, width]);

            Parallel.For(0, matrix.Height, (index) =>
            {
                for (int j = 0; j < width; j++)
                {
                    matrix.Values[index, j] = vectors[j].Values[index];
                }
            });
            return matrix;
        }

        private static void MapTo01(double[,] image)
        {
            Parallel.For(0, image.GetLongLength(0), (index) =>
            {
                for (int j = 0; j < image.GetLongLength(1); j++)
                {
                    image[index, j] = image[index, j] / 255;
                }
            });
        }

        /// <summary>
        /// Calculate square of two vectors
        /// </summary>
        private static double SquareError(Vector vector1, Vector vector2)
        {
            if (vector1.Height != vector2.Height)
                throw new ArgumentException("Vectors must have same height");

            double error = 0;

            Parallel.For(0, vector1.Height, (index) =>
            {
                error += Math.Pow(vector1.Values[index] - vector2.Values[index], 2);
            });
            return error;
        }
    }
}
