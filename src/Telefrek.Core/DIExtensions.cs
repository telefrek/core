using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Telefrek.Core
{
    public static partial class TelefrekExtensions
    {

        /// <summary>
        /// Creates an instance of the given type, injecting parameters where possible from the provider
        /// </summary>
        /// <param name="provider">The provider to use for parameter resolution</param>
        /// <param name="supplied">The supplied parameters</param>
        /// <typeparam name="T">The type of object to craete</typeparam>
        /// <returns>A new instance of the object if it can be created</returns>
        public static T CreateInstance<T>(this IServiceProvider provider, params object[] supplied)
            => (T)provider.CreateInstance(typeof(T), supplied);

        /// <summary>
        /// Creates an instance of the given type, injecting parameters where possible from the provider
        /// </summary>
        /// <param name="provider">The provider to use for parameter resolution</param>
        /// <param name="t"></param>
        /// <param name="supplied">The supplied parameters</param>
        /// <returns>A new instance of the object if it can be created</returns>
        public static object CreateInstance(this IServiceProvider provider, Type t, params object[] supplied)
        {
            var types = supplied.Select(s => s.GetType()).ToArray();
            var cinfo = t.GetConstructors().Where(c => IsMatch(c, types)).FirstOrDefault();

            if (cinfo != null)
            {
                // Basically the same loop as match test but with calls to provider to resolve missing parameters
                var pArr = cinfo.GetParameters();
                var pMap = new object[pArr.Length];

                // Fill the supplied and parameter mixed
                var idx = 0;
                var i = 0;
                for (; i < pArr.Length && idx < types.Length; ++i)
                    if (pArr[i].ParameterType.IsAssignableFrom(types[idx]))
                        pMap[i] = supplied[idx++];
                    else if (pArr[i].ParameterType.IsAssignableFrom(typeof(ILogger)))
                        pMap[i] = provider.GetRequiredService<ILoggerFactory>().CreateLogger(t);
                    else
                        pMap[i] = provider.GetService(pArr[i].ParameterType);

                // Finish any remaining injections
                for (; i < pArr.Length; ++i)
                    pMap[i] = provider.GetService(pArr[i].ParameterType);

                // Hope for the best
                return cinfo.Invoke(pMap);
            }

            // Boom goes the dynamite!
            throw new InvalidOperationException("Failed to locate constructor to match parameters supplied with provider");
        }

        /// <summary>
        /// Test to validate if it's possible to use this constructor with the supplied types
        /// 
        /// NOTE: This is really not safe, but a convenient hack for extending DI in cases of partial data matching
        /// </summary>
        /// <param name="constructorInfo">The constructor to test</param>
        /// <param name="suppliedTypes">The types supplied already</param>
        /// <returns>True if the constructor is a match for creating</returns>
        static bool IsMatch(ConstructorInfo constructorInfo, Type[] suppliedTypes)
        {
            try
            {
                // Filter out non-public and static constructors
                if (!constructorInfo.IsPublic) return false;
                if (constructorInfo.IsStatic) return false;

                // Get the parameters
                var pArr = constructorInfo.GetParameters();

                // Can't use one with less than the supplied
                if (pArr.Length < suppliedTypes.Length) return false;

                // Supplied types must be in order, interfaces ASSUMED to be in service provider if not matched...
                var idx = 0;
                var i = 0;
                for (i = 0; i < pArr.Length && idx < suppliedTypes.Length; ++i)
                {
                    if (pArr[i].ParameterType.IsAssignableFrom(suppliedTypes[idx]))
                        idx++;
                    else if (!pArr[i].ParameterType.IsInterface)
                        break;
                }

                // May have residual types left over
                for (; i < pArr.Length; ++i)
                    if (!pArr[i].ParameterType.IsInterface)
                        break;

                // Have to make it through both arrays for this to be true
                return idx == suppliedTypes.Length && i == pArr.Length;
            }
            catch
            {
                return false;
            }
        }
    }
}