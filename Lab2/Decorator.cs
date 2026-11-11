using DeliverySOLID;

namespace DeliveryStructural;

// Decorator de baza pentru pret: implementeaza aceeasi interfata ca obiectul
// decorat si ii delega calculul. Subclasele adauga cate un cost suplimentar.
public abstract class PricingDecorator : IPricingStrategy
{
    protected readonly IPricingStrategy inner;

    protected PricingDecorator(IPricingStrategy inner)
    {
        this.inner = inner;
    }

    public virtual double CalculatePrice(double weightKg) => inner.CalculatePrice(weightKg);
}

// Asigurare: 2% din valoarea declarata, dar minim 3 lei.
public class InsuranceDecorator : PricingDecorator
{
    private const double Rate = 0.02;
    private const double MinFee = 3.0;

    private readonly double declaredValue;

    public InsuranceDecorator(IPricingStrategy inner, double declaredValue) : base(inner)
    {
        if (declaredValue <= 0)
        {
            throw new ArgumentException("Valoarea declarata trebuie sa fie pozitiva");
        }

        this.declaredValue = declaredValue;
    }

    public override double CalculatePrice(double weightKg) =>
        Math.Round(base.CalculatePrice(weightKg) + Math.Max(MinFee, declaredValue * Rate), 2);
}

// Manipulare speciala pentru colete fragile: taxa fixa de 10 lei.
public class FragileDecorator : PricingDecorator
{
    private const double Fee = 10.0;

    public FragileDecorator(IPricingStrategy inner) : base(inner)
    {
    }

    public override double CalculatePrice(double weightKg) =>
        Math.Round(base.CalculatePrice(weightKg) + Fee, 2);
}
