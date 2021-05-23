using System;
using System.Globalization;
using System.Reflection;

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
        /// The target instance for the method or <c>null</c>.
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// Creates a new function handle by providing a delegate.
        /// </summary>
        /// <param name="handle">A delegate.</param>
        public FunctionHandle(Delegate handle)
            : this(handle.Method, handle.Target)
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
        /// <param name="target">The target for the method call, in case the method is not static.</param>
        public FunctionHandle(MethodInfo method, object target = null)
        {
            if (method == null) throw new ArgumentNullException("method");
            if (method.ContainsGenericParameters)
            {
                throw new ArgumentException("The given method contains generic parameters. Generic methods are not supported.", nameof(method));
            }
            if (!method.IsStatic && target == null)
            {
                throw new ArgumentNullException(nameof(target), "The method is not static. Therefore, a target instance is required.");
            }
            Method = method;
            Target = target;
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
            return Method.Invoke(Target, BindingFlags.Default, Type.DefaultBinder,
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
