using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WFA_Lib.HelperStructs
{
    public struct Word
    {
        public List<Alphabet> Values { get; }
        public int Length { get => Values.Count; }

        public Word(List<Alphabet> word)
        {
            Values = new List<Alphabet>();

            Values.AddRange(word);
        }

        public Word(Alphabet letter)
        {
            Values = new List<Alphabet>() { letter };
        }

        public Word Concatenate(Alphabet letter)
        {
            List<Alphabet> newValues = new List<Alphabet>();
            newValues.Add(letter);
            newValues.AddRange(Values);

            return new Word(newValues);
        }

        public Word AddFirst(Alphabet letter)
        {
            List<Alphabet> newValues = new List<Alphabet>();
            newValues.Add(letter);
            newValues.AddRange(Values);

            return new Word(newValues);
        }

        public static Word operator +(Word word1, Word word2)
        {
            List<Alphabet> newValues = new List<Alphabet>();

            newValues.AddRange(word1.Values);
            newValues.AddRange(word2.Values);

            return new Word(newValues);
        }

        public static Word operator +(Word word, Alphabet letter)
        {
            List<Alphabet> newValues = new List<Alphabet>();
            newValues.AddRange(word.Values);
            newValues.Add(letter);

            return new Word(newValues);
        }

        public static Word operator +(Alphabet letter, Word word)
        {
            List<Alphabet> newValues = new List<Alphabet>();
            newValues.Add(letter);
            newValues.AddRange(word.Values);

            return new Word(newValues);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var letter in Values)
            {
                switch (letter)
                {
                    case Alphabet._0:
                        sb.Append("0");
                        break;
                    case Alphabet._1:
                        sb.Append("1");
                        break;
                    case Alphabet._2:
                        sb.Append("2");
                        break;
                    case Alphabet._3:
                        sb.Append("3");
                        break;
                    default:
                        break;
                }
            }
            return sb.ToString();
        }
    }


}