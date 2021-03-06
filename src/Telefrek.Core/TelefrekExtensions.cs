using System;
using System.Collections;

namespace Telefrek.Core
{
    public static partial class TelefrekExtensions
    {
        /// <summary>
        /// Validates if an object is the default for it's type
        /// </summary>
        /// <param name="instance">an instance of the object</param>
        /// <typeparam name="T">The type of object</typeparam>
        /// <returns>True if the object is default, null or empty</returns>
        public static bool IsNullOrDefault<T>(this T instance)
        {
            if (instance == null) return true;
            if (object.Equals(instance, default(T))) return true;

            var methodType = typeof(T);
            if (Nullable.GetUnderlyingType(methodType) != null) return false;

            var instanceType = instance.GetType();
            if (typeof(ICollection).IsAssignableFrom(instanceType))
            {
                var col = instance as ICollection;
                if (col == null || col.Count == 0)
                    return true;
            }

            if (instanceType.IsValueType && instanceType != methodType)
            {
                object obj = Activator.CreateInstance(instance.GetType());
                return obj.Equals(instance);
            }

            return false;
        }
    }
}