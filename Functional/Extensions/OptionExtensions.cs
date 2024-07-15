using System;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class OptionExtensions
    {
        //Adapter Option<T> -> Option<TResult>
        //attached to Option<T> object.
        //This is the adapter which wraps a function which does not operate on Options
        //but turns it into a full-blown function on Options.
        public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> map) =>
            option is Some<T> some ? (Option<TResult>)map(some) : None.Value;
        //-------------------------------------|------------------------------ In public class Option<T>
        //public static implicit operator Option<T>(T value) => new Some<T>(value);
        //--------------------------------------------------------------|----- In public class Option<T>
        //public static implicit operator Option<T>(None none) =>new None<T>();
        //--------------------------------------------------|----------------- In  public class Some<T> : Option<T>{..}
        //public static implicit operator T(Some<T> value) =>value.Content;

        //Adapter T -> Option<T>
        //This adapter receives a common object of type T, and turns it into an optional T
        //by inspecting result of a predicate.
        public static Option<T> When<T>(this T value, Func<T, bool> predicate) =>
            predicate(value) ? value : None.Value;//-> This is implicitly converted into Option<T> which is actually a None of T.
        //-----------------------------|------------------------------ In public class Option<T>
        //public static implicit operator Option<T>(T value) => new Some<T>(value);
        //-----------------------------------------------|------------ In public class Option<T>
        //public static implicit operator Option<T>(None none) =>new None<T>();

        //Adapter (Option<T>, T) -> T
        //It turns a full-blown option into its wrapped typed T.
        //If option is Some, then it already has an object we need.
        //Otherwise the reduce operator must receive a replacement object which will be used to replace None.
        public static T Reduce<T>(this Option<T> option, T whenNone) =>
            option is Some<T> some ? some : whenNone;
        //----------------------------|----------------- In  public class Some<T> : Option<T>{..}
        //public static implicit operator T(Some<T> value) =>value.Content;

        //Adapter (Option<T>, T) -> T (Lazy variant)
        //Lazy version of Reduce which takes a lambda executed only needed.
        public static T Reduce<T>(this Option<T> option, Func<T> whenNone) =>
            option is Some<T> some ? some : whenNone();
        //----------------------------|----------------- In  public class Some<T> : Option<T>{..}
        //public static implicit operator T(Some<T> value) =>value.Content;
    }
}
