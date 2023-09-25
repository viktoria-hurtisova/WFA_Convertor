using System.Globalization;

namespace WFA_Lib.HelperStructs
{
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
}
