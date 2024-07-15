namespace HCL_ODA_TestPAD.Functional
{
    //public static implicit operator XElement(XmlBase xmlBase)
    //{
    //    return xmlBase.Xml;
    //}
    //XmlBase xmlBase = WhatEverGetTheXmlBase();
    //XElement xelement = xmlBase;
    //no explicit convert here like: XElement xelement = (XElement)xmlBase;
    //It's a way for you as a developer to tell the compiler:
    //"even though these look like two totally unrelated types,
    //there is actually a way to convert from one to the other;
    //just let me handle the logic for how to do it."

    public class Option<T>
    {
        //Function return values converted explicitly 
        //Usage 1: (Option<TResult>) map(some) -> Option<TResult>
        //public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> map) =>
        //      option is Some<T> some ? (Option<TResult>) map(some) : None.Value;
        //Usage 2: (Option<T>) value -> Option<T>
        //public static Option<T> When<T>(this T value, Func<T, bool> predicate) =>
        //      predicate(value) ? (Option<T>) value : None.Value;
        public static implicit operator Option<T>(T value) =>
            new Some<T>(value);

        public static implicit operator Option<T>(None none) =>
            new None<T>();
    }

    public class Some<T> : Option<T>
    {
        private T Content { get; }

        public Some(T content)
        {
            Content = content;
        }
        //map(some) : Func<T, TResult> map
        //map is a function which has an argument T
        //when an instance of Some<T> goes to an argument of a function T, this implicit operator called.
        //(T)some -> Option<T> : or when an instance of Some<T> is returned as function return value casted implicitly to T.
        public static implicit operator T(Some<T> value) =>
            value.Content;
    }

    public class None<T> : Option<T>
    {
    }

    public class None
    {
        public static None Value { get; } = new None();
        private None() { }
    }
}
