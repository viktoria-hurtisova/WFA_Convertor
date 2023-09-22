using System;
using System.Collections.Generic;
using System.Text;

namespace WFA_Lib.HelperStructs
{
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
