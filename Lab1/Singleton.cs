namespace DeliverySOLID;

// SINGLETON: there is only one instance of the AWB code generator.
// This way two parcels can never get the same code, even if they are created in different places.
public sealed class TrackingIdGenerator
{
    // Lazy: the instance is created only on first use. Lazy<T> is thread-safe by default.
    private static readonly Lazy<TrackingIdGenerator> instance =
        new Lazy<TrackingIdGenerator>(() => new TrackingIdGenerator());

    private readonly object gate = new object();
    private int next = 1;

    // Private constructor: nobody outside the class can call new TrackingIdGenerator().
    private TrackingIdGenerator() { }

    public static TrackingIdGenerator Instance => instance.Value;

    // Returns the next code, in the form RO000001, RO000002, ...
    public string Next()
    {
        lock (gate)
        {
            string id = $"RO{next:D6}";
            next++;
            return id;
        }
    }
}
