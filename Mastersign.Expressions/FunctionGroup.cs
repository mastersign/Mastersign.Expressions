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
        private readonly Dictionary<MethodBase, FunctionHandle> handles = new Dictionary<MethodBase, FunctionHandle>();

        /// <summary>
        /// Initializes an empty instance of the class <see cref="FunctionGroup"/>.
        /// </summary>
        public FunctionGroup()
        { }

        /// <summary>
        /// Initializes a new instance of the class <see cref="FunctionGroup"/>
        /// with a number of functions.
        /// </summary>
        /// <param name="handles">The functions for the group.</param>
        public FunctionGroup(IEnumerable<FunctionHandle> handles)
        {
            foreach (var fh in handles)
            {
                this.handles.Add(fh.Method, fh);
            }
        }

        /// <summary>
        /// Adds a function to the group.
        /// </summary>
        /// <param name="handle">The function.</param>
        public void Add(FunctionHandle handle)
        {
            handles.Add(handle.Method, handle);
        }

        /// <summary>
        /// Try to find a function in the group, that matches the given parameter types.
        /// </summary>
        /// <param name="parameterTypes">An array with parameter types.</param>
        /// <returns>The best matching function or <c>null</c> if no match was found.</returns>
        public FunctionHandle FindMatch(Type[] parameterTypes)
        {
            var binder = Type.DefaultBinder;
            try
            {
                var method = binder.SelectMethod(
                    BindingFlags.Default,
                    handles.Values.Select(fh => (MethodBase)fh.Method).ToArray(),
                    parameterTypes, null);
                return method != null
                    ? new FunctionHandle((MethodInfo)method, handles[method].Target)
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
            return handles.Values.GetEnumerator();
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
