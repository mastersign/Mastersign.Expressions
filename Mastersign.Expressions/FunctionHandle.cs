using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace de.mastersign.expressions
{
    /// <summary>
    /// A handle to describe a function for registration in the <see cref="EvaluationContext"/>.
    /// </summary>
    public class FunctionHandle
    {
        /// <summary>
        /// The reflection info of the function.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Creates a new function handle by providing a delegate.
        /// </summary>
        /// <param name="handle">A delegate.</param>
        public FunctionHandle(Delegate handle)
            : this(handle.Method)
        {
        }

        /// <summary>
        /// Creates a new function handle by providing the reflection info of it.
        /// </summary>
        /// <remarks>
        /// Only static methods can directly be used as a function, 
        /// because in the moment of the calling, there is no object as context 
        /// for a dynamic method call. If you need to call a dynamic method,
        /// wrapp the call into a closure of a lamda expression and use the other constructor.
        /// </remarks>
        /// <param name="method">The reflection info of a static function.</param>
        public FunctionHandle(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException("method");
            if (!method.IsStatic)
            {
                throw new ArgumentException("The given method is not static. Only static methods are supported as a function.");
            }
            if (method.ContainsGenericParameters)
            {
                throw new ArgumentException("The given method contains generic parameters. Generic methods are not supported.");
            }
            Method = method;
        }

        /// <summary>
        /// Gets the return type of the function.
        /// </summary>
        public Type ReturnType { get { return Method.ReturnType; } }

        /// <summary>
        /// Calls the function by a reflection invokation.
        /// </summary>
        /// <param name="parameters">The arguments for the call.</param>
        /// <returns>The result of the call.</returns>
        public object Call(object[] parameters)
        {
            return Method.Invoke(null, BindingFlags.Default, Type.DefaultBinder,
                parameters, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The implicit cast from an <see cref="MethodInfo"/> object into a function handle.
        /// </summary>
        /// <param name="method">The reflection info of a function.</param>
        /// <returns>An instance of <see cref="FunctionHandle"/>.</returns>
        public static implicit operator FunctionHandle(MethodInfo method)
        {
            return new FunctionHandle(method);
        }

        /// <summary>
        /// The implicit cast from a <see cref="Delegate"/> into a function handle.
        /// </summary>
        /// <param name="function">The delegate.</param>
        /// <returns>An instance of <see cref="FunctionHandle"/>.</returns>
        public static implicit operator FunctionHandle(Delegate function)
        {
            return new FunctionHandle(function);
        }
    }
}
