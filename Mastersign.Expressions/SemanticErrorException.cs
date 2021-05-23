using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Mastersign.Expressions
{
    /// <summary>
    /// Represents a semantic error in an expression string.
    /// </summary>
    [Serializable]
    public class SemanticErrorException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SemanticErrorException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        public SemanticErrorException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="SemanticErrorException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="innerException">The underlying compiler exception.</param>
        public SemanticErrorException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="SemanticErrorException"/> with
        /// the serialization infos.
        /// </summary>
        /// <param name="info">The serialization info object.</param>
        /// <param name="context">The streaming context of the serialization.</param>
        protected SemanticErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
