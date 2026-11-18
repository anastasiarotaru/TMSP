namespace Delivery.Domain;

// Delivery address.
public class Address
{
    public string Street { get; }
    public string City { get; }

    public Address(string street, string city)
    {
        Street = street;
        City = city;
    }

    public override string ToString() => $"{Street}, {City}";
}
