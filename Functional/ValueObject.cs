using System.Collections.Generic;
using System.Linq;

namespace HCL_ODA_TestPAD.Functional
{
    //However, it is still possible to reduce the amount of work needed.
    //Instead of doing the actual comparison and hash code calculation in the derived classes,
    //you can ask them to enumerate the members that participate in the comparison and do the rest of the work in the base class.

    //This idea was proposed by a reader of this blog, Steven Roberts. Here’s how the new ValueObject class looks :

    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //        return false;

        //    if (GetType() != obj.GetType())
        //        return false;

        //    var valueObject = (ValueObject)obj;

        //    return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        //}

        public override bool Equals(object obj) =>
            obj is ValueObject other &&
            GetEqualityComponents()
                .SequenceEqual(other.GetEqualityComponents());


        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        public static bool operator ==(ValueObject a, ValueObject b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject a, ValueObject b)
        {
            return !(a == b);
        }
    }
}


//And this is the updated Address:

//public class Address : ValueObject
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

//    protected override IEnumerable<object> GetEqualityComponents()
//    {
//        yield return Street;
//        yield return City;
//        yield return ZipCode;
//    }
//}

//This approach reduces the amount of repeating code in the derived classes and at the same time doesn’t exhibit
//the drawbacks of the implementation with the fully automatic comparison.
//You can still choose which fields you want to take into consideration. And if the value object contains collections,
//it will work too. All you need to do is enumerate all collection elements alongside with other properties.

//This is what you would need to do should you decide to add a list of tenants to the Address value object:

//public class Address : ValueObject
//{
//    public string Street { get; }
//    public string City { get; }
//    public string ZipCode { get; }
//    public List<Tenant> Tenants { get; }

//    public Address(string street, string city, string zipCode, List<Tenant> tenants)
//    {
//        Street = street;
//        City = city;
//        ZipCode = zipCode;
//        Tenants = tenants;
//    }

//    protected override IEnumerable<object> GetEqualityComponents()
//    {
//        yield return Street;
//        yield return City;
//        yield return ZipCode;

//        foreach (Tenant tenant in Tenants)
//        {
//            yield return tenant;
//        }
//    }
//}

//And here’s what to do if you need to override the default Equals() behavior,
//for example, implement case-insensitive comparison for a string and specify the precision for a float / double / decimal type:

//public class Money : ValueObject
//{
//    public string Currency { get; }
//    public decimal Amount { get; }

//    public Money(string currency, decimal amount)
//    {
//        Currency = currency;
//        Amount = amount;
//    }

//    protected override IEnumerable<object> GetEqualityComponents()
//    {
//        yield return Currency.ToUpper();
//        yield return Math.Round(Amount, 2);
//    }
//}

//Note that GetEqualityComponents() transforms Currency into the upper case and rounds Amount up to two decimal points,
//so that now it doesn’t matter if you use

//var money1 = new Money("usd", 2.2222m);
//or

//var money2 = new Money("USD", 2.22m);
//They would be deemed equal from the domain’s point of view.

//Conclusion
//I like this implementation a lot as it’s simpler than the one I used previously. All kudos to Steve Roberts.
