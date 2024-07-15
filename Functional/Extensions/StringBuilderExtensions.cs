using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendFormatLine(
            this StringBuilder @this, string format, params object[] args)
            => @this.AppendFormat(format, args).AppendLine();

        public static StringBuilder AppendLineWhen(
            this StringBuilder @this, Func<bool> predicate, string line)
            => predicate() ? @this.AppendLine(line) : @this;

        public static StringBuilder AppendWhen(
            this StringBuilder @this, Func<bool> predicate,
            Func<StringBuilder, StringBuilder> func)
            => predicate() ? func(@this) : @this;

        public static StringBuilder AppendSequence<T>(
            this StringBuilder @this, IEnumerable<T> seq,
            Func<StringBuilder, T, StringBuilder> func)
            => seq.Aggregate(@this, func);
    }
}
