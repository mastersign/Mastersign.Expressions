using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Sprache;
using de.mastersign.expressions.language;

namespace de.mastersign.expressions
{
    /// <summary>
    /// The evaluation context provides functions, constants and other contextual information
    /// for the evaluation of expressions.
    /// This class is the facade to the Mastersign.Expressions API.
    /// </summary>
    public class EvaluationContext
    {
        #region Static

        /// <summary>
        /// An evaluation context with all default packages loaded.
        /// </summary>
        public static EvaluationContext Default;

        static EvaluationContext()
        {
            Default = new EvaluationContext();
            Default.LoadAllPackages();
        }

        #endregion

        private readonly EvaluationContext parent;

        /// <summary>
        /// Initializes a new instance of <see cref="EvaluationContext"/> without a parent context.
        /// </summary>
        public EvaluationContext()
        {
            this.parent = null;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EvaluationContext"/>
        /// with the context <paramref name="parent"/> as backup for looking up
        /// variables and functions.
        /// </summary>
        /// <param name="parent">The parent context.</param>
        public EvaluationContext(EvaluationContext parent)
        {
            this.parent = parent;
        }

        #region Variables

        private readonly Dictionary<string, Tuple<object, bool>> variables = new Dictionary<string, Tuple<object, bool>>();

        /// <summary>
        /// Sets the value for a variable or constant.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="asConst">A value indicating if the variable is compiled by value or by reference.
        /// If this parameter is set to <c>true</c>, the varibale value is integrated into the
        /// expression as a constant. If set to <c>false</c>, the variable is compiled as a
        /// call to <see cref="ReadVariable"/>.
        /// </param>
        public void SetVariable(string name, object value, bool asConst = false)
        {
            if (Grammar.Keywords.Contains(name)) throw new ArgumentException("The given variable name is a language keyword.");
            variables[name] = Tuple.Create(value, asConst);
        }

        /// <summary>
        /// Removes a variable or constant from the context.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        public void RemoveVariable(string name)
        {
            variables.Remove(name);
        }

        /// <summary>
        /// Checks if a variable or a constant exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns><c>true</c>, if the variable or constant exists; otherwise <c>false</c>.</returns>
        public bool VariableExists(string name)
        {
            return variables.ContainsKey(name)
                || (parent != null && parent.VariableExists(name));
        }

        /// <summary>
        /// Returns the value of a variable or a constant.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The value.</returns>
        public object ReadVariable(string name)
        {
            if (!VariableExists(name))
            {
                throw new ArgumentException("Variable does not exists.", "name");
            }
            Tuple<object, bool> variable;
            return variables.TryGetValue(name, out variable)
                ? variable.Item1
                : parent.ReadVariable(name);
        }

        /// <summary>
        /// Returns a value, indicating of the varibale is a constant value.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns><c>true</c> if the variable will be compiled as constant and 
        /// <c>false</c>, if the variable is compiled as call to <see cref="ReadVariable"/>.</returns>
        public bool IsVariableConstant(string name)
        {
            if (!VariableExists(name))
            {
                throw new ArgumentException("Variable does not exists.", "name");
            }
            Tuple<object, bool> variable;
            return variables.TryGetValue(name, out variable)
                ? variable.Item2
                : parent.IsVariableConstant(name);
        }

        #endregion

        #region Functions

        private readonly Dictionary<string, FunctionGroup> functionGroups
            = new Dictionary<string, FunctionGroup>();

        /// <summary>
        /// Adds a group of functions to the context.
        /// </summary>
        /// <param name="identifier">The function name.</param>
        /// <param name="functionGroup">The group of functions.</param>
        public void AddFunctionGroup(string identifier, FunctionGroup functionGroup)
        {
            functionGroups.Add(identifier, functionGroup);
        }

        /// <summary>
        /// Adds a single function to the context. If there are functions allready registered under the given 
        /// function name, the new function will be added to the function group.
        /// given name
        /// </summary>
        /// <param name="identifier">The function name.</param>
        /// <param name="function">The function.</param>
        public void AddFunction(string identifier, FunctionHandle function)
        {
            FunctionGroup group;
            if (!functionGroups.TryGetValue(identifier, out group))
            {
                group = new FunctionGroup();
                functionGroups.Add(identifier, group);
            }
            group.Add(function);
        }

        /// <summary>
        /// Removes a complete function group from the context.
        /// </summary>
        /// <param name="identifier">The function name.</param>
        public void RemoveFunctionGroup(string identifier)
        {
            functionGroups.Remove(identifier);
        }

        /// <summary>
        /// Checks, if a function group with the given function name exists.
        /// </summary>
        /// <param name="identifier">The function name.</param>
        /// <returns><c>true</c>, if a function group is registered with the given name; otherwise <c>false</c>.</returns>
        public bool FunctionGroupExists(string identifier)
        {
            return functionGroups.ContainsKey(identifier)
                || (parent != null && parent.FunctionGroupExists(identifier));
        }

        /// <summary>
        /// Returns a function group for a given function name.
        /// </summary>
        /// <param name="identifier">The function name.</param>
        /// <returns>The function group or <c>null</c>, if no function group with the given name exists.</returns>
        public FunctionGroup GetFunctionGroup(string identifier)
        {
            FunctionGroup group;
            return functionGroups.TryGetValue(identifier, out group)
                ? group
                : (parent != null ? parent.GetFunctionGroup(identifier) : null);
        }

        #endregion

        #region Parameters

        private ParameterInfo[] parameterList = new ParameterInfo[0];

        /// <summary>
        /// Sets the parameter list for the evaluation.
        /// </summary>
        /// <param name="parameters">An enumerable with the parameters.</param>
        public void SetParameters(IEnumerable<ParameterInfo> parameters)
        {
            parameterList = parameters != null ? parameters.ToArray() : new ParameterInfo[0];
        }

        /// <summary>
        /// Sets the parameter list for the evaluation.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public void SetParameters(params ParameterInfo[] parameters)
        {
            parameterList = parameters ?? new ParameterInfo[0];
        }

        /// <summary>
        /// Checks if a parameter labeled <paramref name="name"/> exists in this evaluation context.
        /// </summary>
        /// <remarks>Parameters are not derived from a parent context.</remarks>
        /// <param name="name">The name of the parameter.</param>
        /// <returns><c>true</c> if this context contains a parameter with the given name; 
        /// otherwise <c>false</c>.</returns>
        public bool ParameterExists(string name)
        {
            return parameterList.Any(p => p.Name.Equals(name));
        }

        /// <summary>
        /// Returns the <see cref="ParameterInfo"/> called <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The parameter description.</returns>
        /// <exception cref="InvalidOperationException">Is thrown, 
        /// if no parameter labeled <paramref name="name"/> exists.</exception>
        public ParameterInfo GetParameter(string name)
        {
            return parameterList.First(p => p.Name.Equals(name));
        }

        /// <summary>
        /// Returns the position of the parameter called <paramref name="name"/> 
        /// in the parameter list.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The position of the parameter.</returns>
        /// <exception cref="InvalidOperationException">Is thrown, 
        /// if no parameter labeled <paramref name="name"/> exists.</exception>
        public int GetParameterPosition(string name)
        {
            for (var i = 0; i < parameterList.Length; i++)
            {
                if (parameterList[i].Name.Equals(name)) return i;
            }
            throw new InvalidOperationException("The given parameter name does not exist.");
        }

        /// <summary>
        /// Creates a sequence of parameter expressions.
        /// </summary>
        private IEnumerable<ParameterExpression> ParameterExpressions
        {
            get { return from p in parameterList select p.Expression; }
        }

        #endregion

        #region Default Context

        /// <summary>
        /// Load all included packages into the context.
        /// </summary>
        public void LoadAllPackages()
        {
            LoadMathPackage();
            LoadConversionPackage();
            LoadStringPackage();
            LoadRegexPackage();
        }

        /// <summary>
        /// Load the package with functions for boolean logic.
        /// </summary>
        public void LoadLogicPackage()
        {
            AddFunction("not", new FunctionHandle((Func<bool, bool>)(v => !v)));
        }

        /// <summary>
        /// The static random class for the 'rand' function in the math package.
        /// </summary>
        private static readonly Random rand = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Load the package with functions and constants for simple math.
        /// </summary>
        public void LoadMathPackage()
        {
            SetVariable("pi", Math.PI, true);
            SetVariable("e", Math.E, true);

            AddFunction("mod", (Func<Int32, Int32, Int32>)((a, b) => a % b));
            AddFunction("mod", (Func<UInt32, UInt32, UInt32>)((a, b) => a % b));
            AddFunction("mod", (Func<Int64, Int64, Int64>)((a, b) => a % b));
            AddFunction("mod", (Func<UInt64, UInt64, UInt64>)((a, b) => a % b));
            AddFunction("mod", (Func<Single, Single, Single>)((a, b) => a % b));
            AddFunction("mod", (Func<Double, Double, Double>)((a, b) => a % b));
            AddFunction("mod", (Func<Decimal, Decimal, Decimal>)((a, b) => a % b));

            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(SByte) }));
            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(Int16) }));
            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(Int32) }));
            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(Int64) }));
            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(Single) }));
            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(Double) }));
            AddFunction("abs", typeof(Math).GetMethod("Abs", new[] { typeof(Decimal) }));

            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(SByte) }));
            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(Int16) }));
            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(Int32) }));
            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(Int64) }));
            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(Single) }));
            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(Double) }));
            AddFunction("sign", typeof(Math).GetMethod("Sign", new[] { typeof(Decimal) }));

            AddFunction("floor", typeof(Math).GetMethod("Floor", new[] { typeof(Double) }));
            AddFunction("floor", typeof(Math).GetMethod("Floor", new[] { typeof(Decimal) }));
            AddFunction("round", typeof(Math).GetMethod("Round", new[] { typeof(Double) }));
            AddFunction("round", typeof(Math).GetMethod("Round", new[] { typeof(Decimal) }));
            AddFunction("round", typeof(Math).GetMethod("Round", new[] { typeof(Double), typeof(int) }));
            AddFunction("round", typeof(Math).GetMethod("Round", new[] { typeof(Decimal), typeof(int) }));
            AddFunction("ceil", typeof(Math).GetMethod("Ceiling", new[] { typeof(Double) }));
            AddFunction("ceil", typeof(Math).GetMethod("Ceiling", new[] { typeof(Decimal) }));
            AddFunction("trunc", typeof(Math).GetMethod("Truncate", new[] { typeof(Double) }));
            AddFunction("trunc", typeof(Math).GetMethod("Truncate", new[] { typeof(Decimal) }));

            AddFunction("sin", typeof(Math).GetMethod("Sin", new[] { typeof(double) }));
            AddFunction("cos", typeof(Math).GetMethod("Cos", new[] { typeof(double) }));
            AddFunction("tan", typeof(Math).GetMethod("Tan", new[] { typeof(double) }));
            AddFunction("asin", typeof(Math).GetMethod("Asin", new[] { typeof(double) }));
            AddFunction("acos", typeof(Math).GetMethod("Acos", new[] { typeof(double) }));
            AddFunction("atan", typeof(Math).GetMethod("Atan", new[] { typeof(double) }));
            AddFunction("atan2", typeof(Math).GetMethod("Atan2", new[] { typeof(double), typeof(double) }));
            AddFunction("sinh", typeof(Math).GetMethod("Sinh", new[] { typeof(double) }));
            AddFunction("cosh", typeof(Math).GetMethod("Cosh", new[] { typeof(double) }));
            AddFunction("tanh", typeof(Math).GetMethod("Tanh", new[] { typeof(double) }));

            AddFunction("exp", typeof(Math).GetMethod("Exp", new[] { typeof(double) }));
            AddFunction("log", typeof(Math).GetMethod("Log", new[] { typeof(double) }));
            AddFunction("log", typeof(Math).GetMethod("Log", new[] { typeof(double), typeof(double) }));
            AddFunction("log10", typeof(Math).GetMethod("Log10", new[] { typeof(double) }));
            AddFunction("sqrt", typeof(Math).GetMethod("Sqrt", new[] { typeof(double) }));

            AddFunction("rand", (Func<double>)(() => rand.NextDouble()));

            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(SByte), typeof(SByte) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Byte), typeof(Byte) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Int16), typeof(Int16) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(UInt16), typeof(UInt16) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Int32), typeof(Int32) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(UInt32), typeof(UInt32) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Int64), typeof(Int64) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(UInt64), typeof(UInt64) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Single), typeof(Single) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Double), typeof(Double) }));
            AddFunction("min", typeof(Math).GetMethod("Min", new[] { typeof(Decimal), typeof(Decimal) }));

            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(SByte), typeof(SByte) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Byte), typeof(Byte) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Int16), typeof(Int16) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(UInt16), typeof(UInt16) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Int32), typeof(Int32) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(UInt32), typeof(UInt32) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Int64), typeof(Int64) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(UInt64), typeof(UInt64) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Single), typeof(Single) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Double), typeof(Double) }));
            AddFunction("max", typeof(Math).GetMethod("Max", new[] { typeof(Decimal), typeof(Decimal) }));
        }

        /// <summary>
        /// Load a package for conversion between primitive types.
        /// </summary>
        public void LoadConversionPackage()
        {
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(SByte) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Byte) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Int16) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(UInt16) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Int32) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(UInt32) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Int64) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(UInt64) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Single) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Double) }));
            AddFunction("c_byte", typeof(Convert).GetMethod("ToByte", new[] { typeof(Decimal) }));
            AddFunction("c_byte", new FunctionHandle((Func<string, Byte>)(str => Byte.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(SByte) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Byte) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Int16) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(UInt16) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Int32) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(UInt32) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Int64) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(UInt64) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Single) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Double) }));
            AddFunction("c_sbyte", typeof(Convert).GetMethod("ToSByte", new[] { typeof(Decimal) }));
            AddFunction("c_sbyte", new FunctionHandle((Func<string, SByte>)(str => SByte.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(SByte) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Byte) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Int16) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(UInt16) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Int32) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(UInt32) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Int64) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(UInt64) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Single) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Double) }));
            AddFunction("c_int16", typeof(Convert).GetMethod("ToInt16", new[] { typeof(Decimal) }));
            AddFunction("c_int16", new FunctionHandle((Func<string, Int16>)(str => Int16.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(SByte) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Byte) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Int16) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(UInt16) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Int32) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(UInt32) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Int64) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(UInt64) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Single) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Double) }));
            AddFunction("c_uint16", typeof(Convert).GetMethod("ToUInt16", new[] { typeof(Decimal) }));
            AddFunction("c_uint16", new FunctionHandle((Func<string, UInt16>)(str => UInt16.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(SByte) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Byte) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Int16) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(UInt16) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Int32) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(UInt32) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Int64) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(UInt64) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Single) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Double) }));
            AddFunction("c_int32", typeof(Convert).GetMethod("ToInt32", new[] { typeof(Decimal) }));
            AddFunction("c_int32", new FunctionHandle((Func<string, Int32>)(str => Int32.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(SByte) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Byte) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Int16) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(UInt16) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Int32) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(UInt32) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Int64) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(UInt64) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Single) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Double) }));
            AddFunction("c_uint32", typeof(Convert).GetMethod("ToUInt32", new[] { typeof(Decimal) }));
            AddFunction("c_uint32", new FunctionHandle((Func<string, UInt32>)(str => UInt32.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(SByte) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Byte) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Int16) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(UInt16) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Int32) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(UInt32) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Int64) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(UInt64) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Single) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Double) }));
            AddFunction("c_int64", typeof(Convert).GetMethod("ToInt64", new[] { typeof(Decimal) }));
            AddFunction("c_int64", new FunctionHandle((Func<string, Int64>)(str => Int64.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(SByte) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Byte) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Int16) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(UInt16) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Int32) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(UInt32) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Int64) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(UInt64) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Single) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Double) }));
            AddFunction("c_uint64", typeof(Convert).GetMethod("ToUInt64", new[] { typeof(Decimal) }));
            AddFunction("c_uint64", new FunctionHandle((Func<string, UInt64>)(str => UInt64.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(SByte) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Byte) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Int16) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(UInt16) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Int32) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(UInt32) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Int64) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(UInt64) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Single) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Double) }));
            AddFunction("c_single", typeof(Convert).GetMethod("ToSingle", new[] { typeof(Decimal) }));
            AddFunction("c_single", new FunctionHandle((Func<string, Single>)(str => Single.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(SByte) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Byte) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Int16) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(UInt16) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Int32) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(UInt32) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Int64) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(UInt64) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Single) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Double) }));
            AddFunction("c_double", typeof(Convert).GetMethod("ToDouble", new[] { typeof(Decimal) }));
            AddFunction("c_double", new FunctionHandle((Func<string, Double>)(str => Double.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(SByte) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Byte) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Int16) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(UInt16) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Int32) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(UInt32) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Int64) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(UInt64) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Single) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Double) }));
            AddFunction("c_decimal", typeof(Convert).GetMethod("ToDecimal", new[] { typeof(Decimal) }));
            AddFunction("c_decimal", new FunctionHandle((Func<string, Decimal>)(str => Decimal.Parse(str, CultureInfo.InvariantCulture))));

            AddFunction("c_str", new FunctionHandle((Func<SByte, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Byte, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Int16, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<UInt16, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Int32, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<UInt32, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Int64, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<UInt64, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Single, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Double, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<Decimal, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<bool, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<DateTime, string>)(v => v.ToString(CultureInfo.InvariantCulture))));
            AddFunction("c_str", new FunctionHandle((Func<string, string>)(v => v)));
            AddFunction("c_str", new FunctionHandle((Func<object, string>)(v => v.ToString())));
        }

        /// <summary>
        /// Load the package with functions for string manipulation.
        /// </summary>
        public void LoadStringPackage()
        {
            AddFunction("to_lower", new FunctionHandle((Func<string, string>)(s => s.ToLowerInvariant())));
            AddFunction("to_upper", new FunctionHandle((Func<string, string>)(s => s.ToUpperInvariant())));
            AddFunction("trim", new FunctionHandle((Func<string, string>)(s => s.Trim())));
            AddFunction("trim_start", new FunctionHandle((Func<string, string>)(s => s.TrimStart())));
            AddFunction("trim_end", new FunctionHandle((Func<string, string>)(s => s.TrimEnd())));
            AddFunction("substr", new FunctionHandle((Func<string, int, string>)((str, start) => str.Substring(start))));
            AddFunction("substr", new FunctionHandle((Func<string, int, int, string>)((str, start, count) => str.Substring(start, count))));
            AddFunction("remove", new FunctionHandle((Func<string, int, string>)((str, start) => str.Remove(start))));
            AddFunction("remove", new FunctionHandle((Func<string, int, int, string>)((str, start, count) => str.Remove(start, count))));
            AddFunction("replace", new FunctionHandle((Func<string, string, string, string>)((str, oldVal, newVal) => str.Replace(oldVal, newVal))));

            AddFunction("find", new FunctionHandle((Func<string, string, int>)((haystack, needle) => haystack.IndexOf(needle, StringComparison.InvariantCulture))));
            AddFunction("find", new FunctionHandle((Func<string, string, int, int>)((haystack, needle, start) => haystack.IndexOf(needle, start, StringComparison.InvariantCulture))));
            AddFunction("find", new FunctionHandle((Func<string, string, int, int, int>)((haystack, needle, start, count) => haystack.IndexOf(needle, start, count, StringComparison.InvariantCulture))));
            AddFunction("find_i", new FunctionHandle((Func<string, string, int>)((haystack, needle) => haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase))));
            AddFunction("find_i", new FunctionHandle((Func<string, string, int, int>)((haystack, needle, start) => haystack.IndexOf(needle, start, StringComparison.InvariantCultureIgnoreCase))));
            AddFunction("find_i", new FunctionHandle((Func<string, string, int, int, int>)((haystack, needle, start, count) => haystack.IndexOf(needle, start, count, StringComparison.InvariantCultureIgnoreCase))));
            AddFunction("find_last", new FunctionHandle((Func<string, string, int>)((haystack, needle) => haystack.LastIndexOf(needle, StringComparison.InvariantCulture))));
            AddFunction("find_last", new FunctionHandle((Func<string, string, int, int>)((haystack, needle, start) => haystack.LastIndexOf(needle, start, StringComparison.InvariantCulture))));
            AddFunction("find_last", new FunctionHandle((Func<string, string, int, int, int>)((haystack, needle, start, count) => haystack.LastIndexOf(needle, start, count, StringComparison.InvariantCulture))));
            AddFunction("find_last_i", new FunctionHandle((Func<string, string, int>)((haystack, needle) => haystack.LastIndexOf(needle, StringComparison.InvariantCultureIgnoreCase))));
            AddFunction("find_last_i", new FunctionHandle((Func<string, string, int, int>)((haystack, needle, start) => haystack.LastIndexOf(needle, start, StringComparison.InvariantCultureIgnoreCase))));
            AddFunction("find_last_i", new FunctionHandle((Func<string, string, int, int, int>)((haystack, needle, start, count) => haystack.LastIndexOf(needle, start, count, StringComparison.InvariantCultureIgnoreCase))));

            AddFunction("contains", new FunctionHandle((Func<string, string, bool>)((haystack, needle) => haystack.Contains(needle))));
            AddFunction("starts_with", new FunctionHandle((Func<string, string, bool>)((str, start) => str.StartsWith(start))));
            AddFunction("ends_with", new FunctionHandle((Func<string, string, bool>)((str, end) => str.EndsWith(end))));

            AddFunction("min", new FunctionHandle((Func<string, string, string>)((a, b) => string.Compare(a, b, false, CultureInfo.InvariantCulture) > 0 ? b : a)));
            AddFunction("max", new FunctionHandle((Func<string, string, string>)((a, b) => string.Compare(a, b, false, CultureInfo.InvariantCulture) < 0 ? b : a)));
            AddFunction("min_i", new FunctionHandle((Func<string, string, string>)((a, b) => string.Compare(a, b, true, CultureInfo.InvariantCulture) > 0 ? b.ToLowerInvariant() : a.ToLowerInvariant())));
            AddFunction("max_i", new FunctionHandle((Func<string, string, string>)((a, b) => string.Compare(a, b, true, CultureInfo.InvariantCulture) < 0 ? b.ToLowerInvariant() : a.ToLowerInvariant())));
        }

        /// <summary>
        /// Load the package for regular expressions.
        /// </summary>
        public void LoadRegexPackage()
        {
            AddFunction("regex", typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) }));
            AddFunction("regex_match", new FunctionHandle((Func<string, string, string>)((str, regex) =>
                {
                    var m = Regex.Match(str, regex);
                    return m.Success ? m.Value : null;
                })));
            AddFunction("regex_replace", typeof(Regex).GetMethod("Replace", new[] { typeof(string), typeof(string), typeof(string) }));
        }

        #endregion

        #region Parsing, Compiling, Evaluating

        private readonly Grammar grammar = new Grammar();

        internal Grammar Grammar { get { return grammar; } }

        /// <summary>
        /// Gets and sets the language capabilities.
        /// </summary>
        public GrammarCapabilities Capabilities
        {
            get { return grammar.Capabilities; }
            set
            {
                if (grammar.Capabilities == value) return;
                grammar.Capabilities = value;
                cachedParser = null;
            }
        }

        private Parser<ExpressionElement> cachedParser;

        /// <summary>
        /// Parse the given expression string, build the abstract syntax tree 
        /// (represented by the returned <see cref="ExpressionElement"/>) and check
        /// the tree for semantic errors.
        /// </summary>
        /// <param name="input">The expression string.</param>
        /// <returns>The root of the abstract syntax tree.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public ExpressionElement ParseAndCheckExpression(string input)
        {
            ExpressionElement element;
            if (cachedParser == null)
            {
                cachedParser = grammar.Expression.End();
            }
            try
            {
                element = cachedParser.Parse(input);
            }
            catch (Exception ex)
            {
                throw new SyntaxErrorException("Syntax error.", ex);
            }
            var sb = new StringBuilder();
            if (!element.CheckSemantic(this, sb))
            {
                throw new SemanticErrorException(sb.ToString());
            }
            return element;
        }

        private void CheckParameters(Type[] argTypes)
        {
            var parameters = ParameterExpressions.ToArray();
            if (parameters.Length != argTypes.Length)
            {
                throw new SemanticErrorException("The number of parameters does not match the evaluation context.");
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Type != argTypes[i])
                {
                    throw new SemanticErrorException(string.Format("The type of parameter '{0}' does not match.", parameters[i].Name));
                }
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string to a lambda with <see cref="Object"/> as return type.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<object> CompileExpression(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new Type[0]);
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(object))
                {
                    expr = Expression.Convert(expr, typeof(object));
                }
                var lambda = Expression.Lambda<Func<object>>(expr);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<TResult> CompileExpression<TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new Type[0]);
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<TResult>>(expr);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, TResult> CompileExpression<T1, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var parameterExpressions = ParameterExpressions.ToArray();
                var lambda = Expression.Lambda<Func<T1, TResult>>(expr, parameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, TResult> CompileExpression<T1, T2, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, T3, TResult> CompileExpression<T1, T2, T3, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2), typeof(T3) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, T3, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, T3, T4, TResult> CompileExpression<T1, T2, T3, T4, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, T3, T4, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, T3, T4, T5, TResult> CompileExpression<T1, T2, T3, T4, T5, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, T3, T4, T5, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, T3, T4, T5, T6, TResult> CompileExpression<T1, T2, T3, T4, T5, T6, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, T3, T4, T5, T6, T7, TResult> CompileExpression<T1, T2, T3, T4, T5, T6, T7, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks and compiles an expression string with the return type <typeparamref name="TResult"/>.
        /// </summary>
        /// <remarks>Calls <see cref="ParseAndCheckExpression"/> before compilation.</remarks>
        /// <param name="input">The expression string.</param>
        /// <returns>A lambda delegate of the compilation result.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> CompileExpression<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string input)
        {
            var model = ParseAndCheckExpression(input);
            CheckParameters(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
            var expr = model.GetExpression(this);
            try
            {
                if (expr.Type != typeof(TResult))
                {
                    expr = Expression.ConvertChecked(expr, typeof(TResult));
                }
                var lambda = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>(expr, ParameterExpressions);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                throw new SemanticErrorException("Error compiling the expression.", ex);
            }
        }

        /// <summary>
        /// Parses, checks, compiles and calls an expression string.
        /// </summary>
        /// <remarks>Calls <see cref="CompileExpression"/> before evaluating.</remarks>
        /// <param name="expression">The expression string.</param>
        /// <returns>The resulting value of the expression.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public object EvaluateExpression(string expression)
        {
            return CompileExpression(expression)();
        }

        /// <summary>
        /// Parses, checks, compiles and calls an expression string.
        /// </summary>
        /// <remarks>Calls <see cref="CompileExpression"/> before evaluating.</remarks>
        /// <param name="expression">The expression string.</param>
        /// <param name="p1">The first parameter value for the evaluation.</param>
        /// <returns>The resulting value of the expression.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public object EvaluateExpression<T1>(string expression, T1 p1)
        {
            return CompileExpression<T1, object>(expression)(p1);
        }

        /// <summary>
        /// Parses, checks, compiles and calls an expression string.
        /// </summary>
        /// <remarks>Calls <see cref="CompileExpression"/> before evaluating.</remarks>
        /// <param name="expression">The expression string.</param>
        /// <param name="p1">The first parameter value for the evaluation.</param>
        /// <param name="p2">The second parameter value for the evaluation.</param>
        /// <returns>The resulting value of the expression.</returns>
        /// <exception cref="SyntaxErrorException">Is thrown, if the expression has syntax errors.</exception>
        /// <exception cref="SemanticErrorException">Is thrown, if the expression has semantic errors.</exception>
        public object EvaluateExpression<T1, T2>(string expression, T1 p1, T2 p2)
        {
            return CompileExpression<T1, T2, object>(expression)(p1, p2);
        }

        #endregion
    }
}