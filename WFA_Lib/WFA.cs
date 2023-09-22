using System;
using System.Collections.Generic;
using System.IO;
using WFA_Lib.HelperStructs;
using WFA_Lib.LinearAlgebra;

namespace WFA_Lib
{

    public class WFA
    {
        private ColorRepresentation colorRepresentation;
        public ColorRepresentation Representation { get => colorRepresentation; }

        private int numberOfStates;
        public int NumberOfStates { get => numberOfStates; }

        private List<Transition> transitions;
        public List<Transition> Transitions { get => transitions; }

        private ResolutionStruct resolution;
        public ResolutionStruct Resolution { get => resolution; }

        private List<State> states;
        public List<State> States { get => states; }

        private MyVector finalDistribution;
        public MyVector FinalDistribution { get => finalDistribution; set { finalDistribution = value; } }
        public List<MyMatrix> TransitionMatrices { get; private set; }
        public MyVector InitialDistribution { get; set; }

        public WFA(ResolutionStruct res, ColorRepresentation repre)
        {
            resolution = res;
            colorRepresentation = repre;
            transitions = new List<Transition>();
            states = new List<State>();
            numberOfStates = 0;
        }

        public WFA(string fileName)
        {
            transitions = new List<Transition>();
            states = new List<State>();

            LoadWFA(fileName);
            CreateTransitionMatrices();

            InitialDistribution = new MyVector(new double[NumberOfStates]);
            InitialDistribution.Values[6] = 1;
        }

        private void LoadWFA(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                throw new FileNotFoundException($"There in no {Path.GetFileName(fileLocation)} in {Path.GetDirectoryName(fileLocation)}.");

            else if (Path.GetFileName(fileLocation).Contains(".wfa"))
            {
                try
                {
                    LoadWFAFromWFA(fileLocation);

                }
                catch (Exception)
                {

                    throw new FileLoadException("The input file cannot be loaded");
                }
                AddBaseTransitions();
            }
            else
                throw new ArgumentException("Not supported file format", "fileName");

        }

        private void LoadWFAFromWFA(string fileName)
        {
            int fromState, toState;
            double weight;
            Alphabet label;
            bool headerParsed = false;
            bool finalDistLoaded = false;
            byte[] buffer = new byte[4096 + 17];
            int numOfBytes = 0;
            int startIndex = 0;
            int offset = 0;
            int lastProcessedByteOfFinalDist = 0;
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                while ((numOfBytes = fs.Read(buffer, startIndex, 4096)) > 0)
                {

                    if (!headerParsed)
                    {
                        if (buffer[0] == 0)
                        {
                            colorRepresentation = ColorRepresentation.RGB;
                        }
                        else colorRepresentation = ColorRepresentation.YUV;

                        numberOfStates = BitConverter.ToInt32(buffer[1..5]);
                        resolution = new ResolutionStruct(BitConverter.ToInt32(buffer[5..9]), BitConverter.ToInt32(buffer[9..13]));
                        finalDistribution = new MyVector(new double[numberOfStates]);

                        headerParsed = true;
                        offset = 13;
                    }


                    int lastProcessedByte = 0;
                    if (!finalDistLoaded)
                    {
                        for (int i = offset; i + 8 <= numOfBytes + startIndex; i += 8)
                        {
                            if (lastProcessedByteOfFinalDist == numberOfStates)
                            {
                                finalDistLoaded = true;
                                offset = lastProcessedByte;
                                break;
                            }
                            finalDistribution.Values[lastProcessedByteOfFinalDist] = BitConverter.ToDouble(buffer[i..(i + 8)]);
                            lastProcessedByteOfFinalDist++;
                            lastProcessedByte = i + 8;
                        }

                    }

                    if (finalDistLoaded)
                    {
                        for (int i = offset; i + 17 <= numOfBytes + startIndex; i += 17)
                        {
                            weight = BitConverter.ToDouble(buffer[i..(i + 8)]);
                            fromState = BitConverter.ToInt32(buffer[(i + 8)..(i + 12)]);
                            toState = BitConverter.ToInt32(buffer[(i + 12)..(i + 16)]);
                            label = (Alphabet)buffer[i + 16];
                            transitions.Add(new Transition(fromState, toState, label, weight));
                            lastProcessedByte = i + 17;
                        }
                    }

                    startIndex = numOfBytes + startIndex - lastProcessedByte;
                    for (int i = 0; i < startIndex; i++)
                    {
                        buffer[i] = buffer[lastProcessedByte + i];
                    }

                    offset = 0;
                }

            }
        }
        public void EncodeWFA(string fileLocation)
        {
            using (var fs = File.Create(fileLocation))
            {
                byte[] buffer = EncodeHeader();
                byte[] helper = new byte[8];
                int startIndex = 13;

                CalculateFinalDistribution();

                // We have to save final distribution
                for (int i = 0; i < finalDistribution.Height; i++)
                {
                    if (startIndex >= 4096)
                    {
                        PrintBuffer(fs, buffer[0..4096]);

                        for (int j = 0; j < startIndex - 4096; j++)
                        {
                            buffer[j] = buffer[4096 + j];
                        }
                        startIndex -= 4096;
                    }
                    BitConverter.TryWriteBytes(helper, finalDistribution.Values[i]);
                    for (int j = 0; j < helper.Length; j++)
                    {
                        buffer[startIndex + j] = helper[j];
                    }
                    startIndex += 8;
                }

                foreach (var t in transitions)
                {
                    if (startIndex >= 4096)
                    {
                        PrintBuffer(fs, buffer[0..4096]);

                        for (int i = 0; i < startIndex - 4096; i++)
                        {
                            buffer[i] = buffer[4096 + i];
                        }
                        startIndex -= 4096;
                    }

                    BitConverter.TryWriteBytes(helper, t.Weight);
                    for (int i = 0; i < 8; i++)
                    {
                        buffer[startIndex + i] = helper[i];
                    }
                    startIndex += 8;

                    BitConverter.TryWriteBytes(helper, t.FromStateId);
                    for (int i = 0; i < 4; i++)
                    {
                        buffer[startIndex + i] = helper[i];
                    }
                    startIndex += 4;

                    BitConverter.TryWriteBytes(helper, t.ToStateId);
                    for (int i = 0; i < 4; i++)
                    {
                        buffer[startIndex + i] = helper[i];
                    }
                    startIndex += 4;

                    buffer[startIndex] = (byte)t.Label;
                    startIndex++;
                }

                if (startIndex > 0)
                {
                    PrintBuffer(fs, buffer, startIndex);
                }
                fs.Flush();
            }
        }

        private byte[] EncodeHeader()
        {
            byte[] buffer = new byte[4096 + 17];
            byte[] helper = new byte[8];

            buffer[0] = (byte)Representation;
            int startIndex = 1;

            BitConverter.TryWriteBytes(helper, NumberOfStates);

            for (int i = 0; i < 4; i++)
            {
                buffer[startIndex + i] = helper[i];
            }
            startIndex += 4;

            BitConverter.TryWriteBytes(helper, Resolution.Width);

            for (int i = 0; i < 4; i++)
            {
                buffer[startIndex + i] = helper[i];
            }
            startIndex += 4;

            BitConverter.TryWriteBytes(helper, Resolution.Height);

            for (int i = 0; i < 4; i++)
            {
                buffer[startIndex + i] = helper[i];
            }
            startIndex += 4;

            return buffer;

        }

        private static void PrintBuffer(FileStream fs, byte[] buffer, int bufferLength = 4096)
        {
            fs.Write(buffer, 0, bufferLength);
        }

        private void CalculateFinalDistribution()
        {
            finalDistribution = new MyVector(new double[numberOfStates]);

            for (int i = 0; i < numberOfStates; i++)
            {
                finalDistribution.Values[i] = states[i].GetAverageIntensity();
            }
        }

        public void ChangeResolution(int ratio)
        {
            int width = resolution.Width / ratio;
            int height = resolution.Height / ratio;

            resolution = new ResolutionStruct(width, height);

        }

        /// <summary>
        /// Create transtion matrices from the list of transitions for decoding
        /// </summary>
        public void CreateTransitionMatrices()
        {
            MyMatrix m0 = new double[NumberOfStates, NumberOfStates];
            MyMatrix m1 = new double[NumberOfStates, NumberOfStates];
            MyMatrix m2 = new double[NumberOfStates, NumberOfStates];
            MyMatrix m3 = new double[NumberOfStates, NumberOfStates];
            foreach (var t in Transitions)
            {
                switch (t.Label)
                {
                    case Alphabet._0:
                        m0.Values[t.FromStateId, t.ToStateId] = t.Weight;
                        break;
                    case Alphabet._1:
                        m1.Values[t.FromStateId, t.ToStateId] = t.Weight;
                        break;
                    case Alphabet._2:
                        m2.Values[t.FromStateId, t.ToStateId] = t.Weight;
                        break;
                    case Alphabet._3:
                        m3.Values[t.FromStateId, t.ToStateId] = t.Weight;
                        break;
                    default:
                        break;
                }
            }
            TransitionMatrices = new List<MyMatrix> { m0, m1, m2, m3 };

        }

        public void SetStateAsProcessed(int ID)
        {
            int index = 0;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].ID == ID)
                {
                    index = i;
                    break;
                }
            }

            State state = states[index];
            states[index] = State.CreateProcessedState(state.ID, state.Image);
        }

        /// <summary>
        /// Romove states which have id bigger or equal to n
        /// </summary>
        /// <param name="n"></param>
        public void RemoveStates(int n)
        {
            List<State> statesToRemove = new List<State>();

            foreach (var state in states)
            {
                if (state.ID >= n)
                {
                    statesToRemove.Add(state);
                }
            }

            foreach (var state in statesToRemove)
            {
                states.Remove(state);
            }

            numberOfStates = states.Count;
        }

        /// <summary>
        /// Remove transitions which are going to or from state with id bigger or equal to n
        /// </summary>
        /// <param name="n"></param>
        public void RemoveTransitions(int n)
        {
            List<Transition> tranToRemove = new List<Transition>();
            foreach (var transition in Transitions)
            {
                if (transition.FromStateId >= n || transition.ToStateId >= n)
                {
                    tranToRemove.Add(transition);
                }
            }

            foreach (var transition in tranToRemove)
            {

                Transitions.Remove(transition);
            }
        }

        public void AddTransitions(List<Transition> transitions)
        {
            foreach (var t in transitions)
            {
                Transitions.Add(t);
            }
        }

        public void AddTransition(Transition transition)
        {
            transitions.Add(transition);
        }

        public void AddState(State state)
        {
            states.Add(state);
            numberOfStates = states.Count;
        }

        private void AddBaseTransitions()
        {
            Transitions.Add(new Transition(0, 0, Alphabet._0, 1));
            Transitions.Add(new Transition(1, 1, Alphabet._0, 0.5));
            Transitions.Add(new Transition(2, 2, Alphabet._0, 0.5));
            Transitions.Add(new Transition(3, 3, Alphabet._0, 0.25));
            Transitions.Add(new Transition(4, 4, Alphabet._0, 0.25));
            Transitions.Add(new Transition(5, 5, Alphabet._0, 0.25));
            Transitions.Add(new Transition(0, 0, Alphabet._1, 1));
            Transitions.Add(new Transition(1, 0, Alphabet._1, 0.5));
            Transitions.Add(new Transition(1, 1, Alphabet._1, 0.5));
            Transitions.Add(new Transition(2, 2, Alphabet._1, 0.5));
            Transitions.Add(new Transition(3, 2, Alphabet._1, 0.25));
            Transitions.Add(new Transition(3, 3, Alphabet._1, 0.25));
            Transitions.Add(new Transition(4, 0, Alphabet._1, 0.25));
            Transitions.Add(new Transition(4, 1, Alphabet._1, 0.5));
            Transitions.Add(new Transition(4, 4, Alphabet._1, 0.25));
            Transitions.Add(new Transition(5, 5, Alphabet._1, 0.25));
            Transitions.Add(new Transition(0, 0, Alphabet._2, 1));
            Transitions.Add(new Transition(1, 1, Alphabet._2, 0.5));
            Transitions.Add(new Transition(2, 0, Alphabet._2, 0.5));
            Transitions.Add(new Transition(2, 2, Alphabet._2, 0.5));
            Transitions.Add(new Transition(3, 1, Alphabet._2, 0.25));
            Transitions.Add(new Transition(3, 3, Alphabet._2, 0.25));
            Transitions.Add(new Transition(4, 4, Alphabet._2, 0.25));
            Transitions.Add(new Transition(5, 0, Alphabet._2, 0.25));
            Transitions.Add(new Transition(5, 2, Alphabet._2, 0.5));
            Transitions.Add(new Transition(5, 5, Alphabet._2, 0.25));
            Transitions.Add(new Transition(0, 0, Alphabet._3, 1));
            Transitions.Add(new Transition(1, 0, Alphabet._3, 0.5));
            Transitions.Add(new Transition(1, 1, Alphabet._3, 0.5));
            Transitions.Add(new Transition(2, 0, Alphabet._3, 0.5));
            Transitions.Add(new Transition(2, 2, Alphabet._3, 0.5));
            Transitions.Add(new Transition(3, 0, Alphabet._3, 0.25));
            Transitions.Add(new Transition(3, 1, Alphabet._3, 0.25));
            Transitions.Add(new Transition(3, 2, Alphabet._3, 0.25));
            Transitions.Add(new Transition(3, 3, Alphabet._3, 0.25));
            Transitions.Add(new Transition(4, 0, Alphabet._3, 0.25));
            Transitions.Add(new Transition(4, 1, Alphabet._3, 0.5));
            Transitions.Add(new Transition(4, 4, Alphabet._3, 0.25));
            Transitions.Add(new Transition(5, 0, Alphabet._3, 0.25));
            Transitions.Add(new Transition(5, 2, Alphabet._3, 0.5));
            Transitions.Add(new Transition(5, 5, Alphabet._3, 0.25));
        }

        public void AddBaseStates()
        {
            double[,] m0 = {
                { 1, 0, 0, 0, 0, 0 },
                { 0, 0.5, 0, 0, 0, 0 },
                { 0, 0, 0.5, 0, 0, 0 },
                { 0, 0, 0, 0.25, 0, 0 },
                { 0, 0, 0, 0, 0.25, 0 },
                { 0, 0, 0, 0, 0, 0.25 }};

            double[,] m1 = {
                { 1, 0, 0, 0, 0, 0 },
                { 0.5, 0.5, 0, 0, 0, 0 },
                { 0, 0, 0.5, 0, 0, 0 },
                { 0, 0, 0.25, 0.25, 0, 0 },
                { 0.25, 0.5, 0, 0, 0.25, 0 },
                { 0, 0, 0, 0, 0, 0.25 }};

            double[,] m2 = {
                { 1, 0, 0, 0, 0, 0 },
                { 0, 0.5, 0, 0, 0, 0 },
                { 0.5, 0, 0.5, 0, 0, 0 },
                { 0, 0.25, 0, 0.25, 0, 0 },
                { 0, 0, 0, 0, 0.25, 0 },
                { 0.25, 0, 0.5, 0, 0, 0.25}};

            double[,] m3 = {
                { 1, 0, 0, 0, 0, 0 },
                { 0.5, 0.5, 0, 0, 0, 0 },
                { 0.5, 0, 0.5, 0, 0, 0 },
                { 0.25, 0.25, 0.25, 0.25, 0, 0 },
                { 0.25, 0.5, 0, 0, 0.25, 0 },
                { 0.25, 0, 0.5, 0, 0, 0.25 }};

            TransitionMatrices = new List<MyMatrix>() { m0, m1, m2, m3 };

            Decoder.CreateBaseImages(this);
        }

    }
}
