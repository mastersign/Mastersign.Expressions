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
        { }

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

        //public bool MatchesDirect(Type[] parameters)
        //{
        //    var parameterInfos = Method.GetParameters();
        //    if (parameterInfos.Length != parameters.Length) return false;
        //    return parameterInfos
        //        .Zip(parameters, (pi, t) => pi.ParameterType.IsAssignableFrom(t))
        //        .All(v => v);
        //}

        //public bool MatchesWithConversion(Type[] parameters)
        //{
        //    var parameterInfos = Method.GetParameters();
        //    if (parameterInfos.Length != parameters.Length) return false;
        //    return parameterInfos
        //        .Zip(parameters, (pi, t) => IsAssignableWithConversion(pi.ParameterType, t))
        //        .All(v => v);
        //}

        //private static bool IsAssignableWithConversion(Type target, Type source)
        //{
        //    return HasImplicitCast(target, source, target) 
        //        || HasImplicitCast(source, source, target) 
        //        || NumericHelper.IsNumeric(target) 
        //            && NumericHelper.IsNumeric(source) 
        //            && NumericHelper.IsUpgradable(source, target);
        //}

        //private static bool HasImplicitCast(Type def, Type src, Type trg)
        //{
        //    return GetImplicitCastOperators(def, src, trg).Any();
        //}

        //private static IEnumerable<MethodInfo> GetImplicitCastOperators(Type def, Type src, Type trg)
        //{
        //    return from op in def.GetMethods(BindingFlags.Public | BindingFlags.Static)
        //           where op.IsSpecialName && op.Name.Equals("op_Implicit")
        //           let parameter = op.GetParameters().Select(pi => pi.ParameterType).ToArray()
        //           where parameter.Length == 1
        //           where op.ReturnType == trg && parameter[0].IsAssignableFrom(src)
        //                 || op.ReturnType == src && parameter[0].IsAssignableFrom(trg)
        //           select op;
        //}

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

        //private static Delegate CreateDelegate(MethodInfo method)
        //{
        //    var returnType = method.ReturnType;

        //    var parameters = method.GetParameters();
        //    var parameterTypes = parameters
        //        .Select(pi => pi.ParameterType)
        //        .ToArray();

        //    if (returnType == typeof(void))
        //    {
        //        throw new ArgumentException("The given method has the return type void and can not serve as a function.");
        //    }

        //    var typeParameters = parameterTypes.Concat(new[] {returnType}).ToArray();
        //    Type delegateType;
        //    switch (typeParameters.Length)
        //    {
        //        case 0:
        //            delegateType = typeof(Func<>).MakeGenericType(typeParameters);
        //            break;
        //        case 1:
        //            delegateType = typeof(Func<,>).MakeGenericType(typeParameters);
        //            break;
        //        case 2:
        //            delegateType = typeof(Func<,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 3:
        //            delegateType = typeof(Func<,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 4:
        //            delegateType = typeof(Func<,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 5:
        //            delegateType = typeof(Func<,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 6:
        //            delegateType = typeof(Func<,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 7:
        //            delegateType = typeof(Func<,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 8:
        //            delegateType = typeof(Func<,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 9:
        //            delegateType = typeof(Func<,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 10:
        //            delegateType = typeof(Func<,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 11:
        //            delegateType = typeof(Func<,,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 12:
        //            delegateType = typeof(Func<,,,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 13:
        //            delegateType = typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 14:
        //            delegateType = typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 15:
        //            delegateType = typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        case 16:
        //            delegateType = typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(typeParameters);
        //            break;
        //        default:
        //            return null;
        //    }
        //    return Delegate.CreateDelegate(delegateType, method);
        //}

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
