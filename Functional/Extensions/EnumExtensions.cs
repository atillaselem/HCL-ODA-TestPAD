using System;
using System.Collections.Generic;
using System.Linq;

namespace HCL_ODA_TestPAD.Extensions
{
    public static class EnumUtils
    {
        #region String to Enum
        public static T ParseEnum<T>(string inString, bool ignoreCase = true, bool throwException = true) where T : struct
        {
            return ParseEnum(inString, default(T), ignoreCase, throwException);
        }

        public static T ParseEnum<T>(string inString, T defaultValue,
                               bool ignoreCase = true, bool throwException = false) where T : struct
        {
            T returnEnum;

            if (!typeof(T).IsEnum || string.IsNullOrEmpty(inString))
            {
                throw new InvalidOperationException("Invalid Enum Type or Input String 'inString'. " + typeof(T).ToString() + "  must be an Enum");
            }

            try
            {
                bool success = Enum.TryParse(inString, ignoreCase, out returnEnum);
                if (!success && throwException)
                {
                    throw new InvalidOperationException("Invalid Cast");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid Cast", ex);
            }

            return returnEnum;
        }
        #endregion

        #region Int to Enum
        public static T ParseEnum<T>(int input, bool throwException = true) where T : struct
        {
            return ParseEnum(input, default(T), throwException);
        }
        public static T ParseEnum<T>(int input, T defaultValue, bool throwException = false) where T : struct
        {
            T returnEnum = defaultValue;
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("Invalid Enum Type. " + typeof(T).ToString() + "  must be an Enum");
            }
            if (Enum.IsDefined(typeof(T), input))
            {
                returnEnum = (T)Enum.ToObject(typeof(T), input);
            }
            else
            {
                if (throwException)
                {
                    throw new InvalidOperationException("Invalid Cast");
                }
            }

            return returnEnum;

        }
        #endregion

        #region String Extension Methods for Enum Parsing
        public static T ToEnum<T>(this string inString, bool ignoreCase = true, bool throwException = true) where T : struct
        {
            return ParseEnum<T>(inString, ignoreCase, throwException);
        }
        public static T ToEnum<T>(this string inString, T defaultValue, bool ignoreCase = true, bool throwException = false) where T : struct
        {
            return ParseEnum(inString, defaultValue, ignoreCase, throwException);
        }
        #endregion

        #region Int Extension Methods for Enum Parsing
        public static T ToEnum<T>(this int input, bool throwException = true) where T : struct
        {
            return ParseEnum(input, default(T), throwException);
        }

        public static T ToEnum<T>(this int input, T defaultValue, bool throwException = false) where T : struct
        {
            return ParseEnum(input, defaultValue, throwException);
        }
        #endregion

    }

    public static class EnumUtil
    {
        public static IEnumerable<T> Enumerate<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static TEnum ParseEnum<TEnum>(this string value,
            bool ignoreCase = false) where TEnum : struct
        {
            TEnum tenumResult;
            Enum.TryParse(value, ignoreCase, out tenumResult);
            return tenumResult;
        }
    }
    namespace ObjectExtensions
    {
        static class ExtensionMethods
        {
            public static T CastTo<T>(this object o) => (T)o;
            public static T As<T>(this object o) where T : class => o as T;
        }
    }
}
