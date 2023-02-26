namespace HCL_ODA_TestPAD.Functional
{
    public abstract class ValueObjectTyped<T>
        where T : ValueObjectTyped<T>
    {
        public override bool Equals(object obj)
        {
            var valueObject = obj as T;

            if (ReferenceEquals(valueObject, null))
                return false;

            return EqualsCore(valueObject);
        }

        protected abstract bool EqualsCore(T other);

        public override int GetHashCode()
        {
            return GetHashCodeCore();
        }

        protected abstract int GetHashCodeCore();

        public static bool operator ==(ValueObjectTyped<T> a, ValueObjectTyped<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObjectTyped<T> a, ValueObjectTyped<T> b)
        {
            return !(a == b);
        }
    }
}

//public class Address : ValueObject<Address>
//{
//    public string Street { get; }
//    public string City { get; }
//    public string ZipCode { get; }

//    public Address(string street, string city, string zipCode)
//    {
//        Street = street;
//        City = city;
//        ZipCode = zipCode;
//    }

//    protected override bool EqualsCore(Address other)
//    {
//        return Street == other.Street
//               && City == other.City
//               && ZipCode == other.ZipCode;
//    }

//    protected override int GetHashCodeCore()
//    {
//        unchecked
//        {
//            int hashCode = Street.GetHashCode();
//            hashCode = (hashCode * 397) ^ City.GetHashCode();
//            hashCode = (hashCode * 397) ^ ZipCode.GetHashCode();
//            return hashCode;
//        }
//    }
//}

//As you can see, the base value object class does a pretty good job hiding most of the boilerplate code related to comparison operations.
//All you need to do is declare two methods.
//The first one is EqualsCore whose parameter other is already strongly typed, so you don’t need to deal with conversion or anything like that. And the second one is GetHashCodeCore.
//It doesn’t bring a lot of additional value as you could just override the standard GetHashCode,
//but the benefit of having this additional abstract method is that it helps you to not forget about defining it in the deriving classes.

//It’s possible to move even more responsibilities to the base ValueObject class.
//For example, I saw implementations where the base class used .NET reflection to get the list of all fields and properties in the deriving class and perform the comparison automatically.
//While it might seem like a good idea as it allows you to reduce the amount of code in Address even further, I would recommend against it.

//The reason why is because such approach fails in two scenarios:

//It doesn’t work if the value object contains a collection. Collections need special treatment:
//taking each element and comparing them one by one instead of simply calling Equals() on the collection instance.

//It doesn’t work if you need to exclude one of the fields from the comparison. It might be the case that not all properties in a value object created equal. There could be some Comment field that doesn’t need to be compared when evaluating equality. At the same time, there’s no way to inform the value object about this. All fields in the derived class will be taken into account and compared against each other.

//So, the actual comparison logic in value objects should be implemented consciously,
//there’s no way you can delegate it to an automatic tool.Hence you need to do a little bit of work in the derived classes by declaring EqualsCore
//and GetHashCodeCore.
