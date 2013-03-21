using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace de.mastersign.expressions
{
    /// <summary>
    /// A function group represents a potentially overloaded function.
    /// </summary>
    public class FunctionGroup : IEnumerable<FunctionHandle>
    {
        private readonly List<FunctionHandle> handles = new List<FunctionHandle>();

        /// <summary>
        /// Initializes an empty instance of the class <see cref="FunctionGroup"/>.
        /// </summary>
        public FunctionGroup()
        {}

        /// <summary>
        /// Initializes a new instance of the class <see cref="FunctionGroup"/>
        /// with a number of functions.
        /// </summary>
        /// <param name="handles">The functions for the group.</param>
        public FunctionGroup(IEnumerable<FunctionHandle> handles)
        {
            this.handles.AddRange(handles);
        }

        /// <summary>
        /// Adds a function to the group.
        /// </summary>
        /// <param name="handle">The function.</param>
        public void Add(FunctionHandle handle)
        {
            handles.Add(handle);
        }

        /// <summary>
        /// Try to find a function in the group, that matches the given parameter types.
        /// </summary>
        /// <param name="parameterTypes">An array with parameter types.</param>
        /// <returns>The best matching function or <c>null</c> if no match was found.</returns>
        public FunctionHandle FindMatch(Type[] parameterTypes)
        {
            //return handles.FirstOrDefault(h => h.MatchesDirect(parameterTypes))
            //    ?? handles.FirstOrDefault(h => h.MatchesWithConversion(parameterTypes));
            var binder = Type.DefaultBinder;
            try
            {
                var method = binder.SelectMethod(
                    BindingFlags.Default,
                    handles.Select(fh => (MethodBase) fh.Method).ToArray(),
                    parameterTypes, null);
                return method != null 
                    ? new FunctionHandle((MethodInfo)method)
                    : null;
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{FunctionHandle}"/> object to iterator over all functions in the group.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<FunctionHandle> GetEnumerator()
        {
            return handles.GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> object to iterator over all functions in the group.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
