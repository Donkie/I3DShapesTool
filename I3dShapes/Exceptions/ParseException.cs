using System;

namespace I3dShapes.Exceptions
{
    public abstract class ParseException : Exception
    {
        protected ParseException(string message)
            : base(message)
        {
        }
    }
}
