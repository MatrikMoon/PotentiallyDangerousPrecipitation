using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/**
 * Added by Moon on 3/11/2022, 2:48AM on a Thursday. I have work tomorrow.
 * https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
 * This is necessary due to ILRepack
 */

namespace PotentiallyDangerousPrecipitation.Extensions
{
    public static class TypeLoadingExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
