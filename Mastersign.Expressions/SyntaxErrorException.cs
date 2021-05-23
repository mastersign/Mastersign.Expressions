using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Mastersign.Expressions
{
    /// <summary>
    /// Represents a syntactic error in an expression string.
    /// </summary>
    [Serializable]
    public class SyntaxErrorException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SyntaxErrorException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="innerException">The underlying exception describing the parser error.</param>
        public SyntaxErrorException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="SyntaxErrorException"/> with
        /// the serialization infos.
        /// </summary>
        /// <param name="info">The serialization info object.</param>
        /// <param name="context">The streaming context of the serialization.</param>
        protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
