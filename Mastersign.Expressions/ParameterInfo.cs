using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace de.mastersign.expressions
{
    /// <summary>
    /// This class represents a parameter in the evaluation context.
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type Type { get; private set; }

        internal ParameterExpression Expression { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Parameter"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public ParameterInfo(string name, Type type)
        {
            Name = name;
            Type = type;
            Expression = System.Linq.Expressions.Expression.Parameter(type, name);
        }
    }
}