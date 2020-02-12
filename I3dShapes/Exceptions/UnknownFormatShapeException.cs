namespace I3dShapes.Exceptions
{
    public class UnknownFormatShapeException : ParseException
    {
        public UnknownFormatShapeException()
            : base("Unknown Format Shape.")
        {
        }

        public int RawType { get; set; }

        internal void SetRawType(int rawType)
        {
            RawType = rawType;
        }
    }
}
