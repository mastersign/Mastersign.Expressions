using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.expressions.language
{
    /// <summary>
    /// This enumeration contains all configuration modes for the grammar.
    /// </summary>
    public enum GrammarCapabilities
    {
        /// <summary>
        /// The basic language with variables, parameters, operators, functions, groups.
        /// </summary>
        Basic,

        /// <summary>
        /// <see cref="Basic"/> with support for reading public properties and fields.
        /// </summary>
        /// <remarks>
        /// This capability adds a certain object oriented flavor to the grammar,
        /// that breaks with the initially pure functional approach.
        /// </remarks>
        MemberRead,
    }
}
