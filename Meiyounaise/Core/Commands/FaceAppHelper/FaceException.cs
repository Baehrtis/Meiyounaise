using System;

namespace Meiyounaise.Core.Commands.FaceAppHelper
{
    public class FaceException : Exception
    {
        public ExceptionType Type { get; set; }

        public FaceException(ExceptionType type, string message) : base(message)
        {
            Type = type;
        }

        public override string ToString()
        {
            return $"{Type}: {Message}";
        }
    }
}
