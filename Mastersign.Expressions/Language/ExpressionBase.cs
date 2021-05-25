using System.Collections;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace Mastersign.Expressions.Language
{
    /// <summary>
    /// The base class of all nodes in the abstract syntax tree of Mastersign.Expressions.
    /// </summary>
    public abstract class ExpressionElement
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The source string of the node.
        /// </summary>
        public abstract string Source { get; }

        /// <summary>
        /// Checks the semantic correctness of the node including potential child nodes.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="errMessages">A buffer for error messages.</param>
        /// <returns><c>true</c> if the node is correct, or <c>false</c> if at least one semantic error occured.</returns>
        public abstract bool CheckSemantic(EvaluationContext context, StringBuilder errMessages);

        /// <summary>
        /// Returns the <see cref="Type"/> the node is representing in the expression.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <returns>The <see cref="Type"/> of the node.</returns>
        public abstract Type GetValueType(EvaluationContext context);

        /// <summary>
        /// Returns the interpreted value. This value is computed without compilation.
        /// If this node has children, the <see cref="GetValue(EvaluationContext,object[])"/> on every child node.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="parameters">The parameter values for the evaluation.</param>
        /// <returns>The computed value.</returns>
        public abstract object GetValue(EvaluationContext context, object[] parameters);

        /// <summary>
        /// Returns the interpreted value. 
        /// For more information see <see cref="GetValue(EvaluationContext, object[])"/>.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <returns>The computed value.</returns>
        public object GetValue(EvaluationContext context)
        {
            return GetValue(context, new object[0]);
        }

        /// <summary>
        /// Returns the LINQ-Expression representing this node.
        /// This is essentially a transformation from the Mastersign.Expressions-AST into a LINQ-AST.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <returns>An <see cref="Expression"/> object as root of a LINQ-AST.</returns>
        public abstract Expression GetExpression(EvaluationContext context);

        /// <summary>
        /// Applies a number of <see cref="IRightPart"/>s to this <see cref="ExpressionElement"/>
        /// and returns the resulting element.
        /// </summary>
        /// <param name="rightParts">A list of right parts.</param>
        /// <returns>The resulting <see cref="ExpressionElement"/>.</returns>
        internal ExpressionElement TransformWithRightParts(IEnumerable<IRightPart> rightParts)
        {
            return rightParts.Aggregate(this, (current, part) => part.BuildExpressionElement(current));
        }
    }

    /// <summary>
    /// A node in the abstract syntax tree of Mastersign.Expressions, which has child nodes.
    /// </summary>
    public abstract class ExpressionNode : ExpressionElement, IEnumerable<ExpressionElement>
    {
        /// <summary>
        /// Returns an <see cref="IEnumerator"/> object to iterate over all child nodes.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{ExpressionElement}"/> object to iterate over all child nodes.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public abstract IEnumerator<ExpressionElement> GetEnumerator();
    }

}