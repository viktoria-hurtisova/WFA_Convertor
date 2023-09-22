using WFA_Lib.LinearAlgebra;

namespace WFA_Lib.HelperStructs
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
}
