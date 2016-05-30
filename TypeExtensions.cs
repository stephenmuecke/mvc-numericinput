using System;
using System.Reflection;
using System.Linq;

namespace Sandtrap.Extensions
{

    public static class TypeExtensions
    {

        /// <summary>
        /// Extension method to determine if a type if numeric.
        /// </summary>
        /// <param name="type">
        /// The type
        /// </param>
        /// <returns>
        /// True if the type is numeric, otherwise false.
        /// </returns>
        public static bool IsNumeric(this Type type)
        {
            // Get the type code
            TypeCode typeCode = Type.GetTypeCode(type);
            // Test for numeric type
            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    return IsNumeric(Nullable.GetUnderlyingType(type));
            }
            return false;
        }

        private static bool IsObjectEqualsMethod(MethodInfo m)
        {
            return m.Name == "Equals"
                && m.GetBaseDefinition().DeclaringType.Equals(typeof(object));
        }

        public static bool OverridesEqualsMethod(this Type type)
        {
            var equalsMethod = type.GetMethods()
                                   .Single(IsObjectEqualsMethod);

            return !equalsMethod.DeclaringType.Equals(typeof(object));
        }


    }

}
