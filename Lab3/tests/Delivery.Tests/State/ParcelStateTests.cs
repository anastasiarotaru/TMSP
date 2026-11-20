using Delivery.Behavioral.State;
using Delivery.Domain;
using Delivery.Tests.Stubs;

namespace Delivery.Tests.State;

public class ParcelStateTests
{
    [Fact]
    public void ValidTransitions_LeadFromRegisteredToDelivered()
    {
        Parcel parcel = TestParcels.New();
        var seen = new List<ParcelStatus> { parcel.Status };

        while (!parcel.IsFinal)
        {
            parcel.Advance();
            seen.Add(parcel.Status);
        }

        Assert.Equal(new[] { ParcelStatus.Registered, ParcelStatus.PickedUp,
                             ParcelStatus.InTransit, ParcelStatus.Delivered }, seen);
        Assert.IsType<DeliveredState>(parcel.State);
    }

    [Fact]
    public void Cancel_IsAllowedBeforeTransit_AndRejectedInTransit()
    {
        Parcel early = TestParcels.New();
        early.Cancel();
        Assert.Equal(ParcelStatus.Cancelled, early.Status);

        Parcel onTheRoad = TestParcels.New();
        onTheRoad.Advance();
        onTheRoad.Advance();
        Assert.Throws<InvalidOperationException>(onTheRoad.Cancel);
        Assert.Equal(ParcelStatus.InTransit, onTheRoad.Status);   // a rejected transition changes nothing
    }

    [Theory]
    [MemberData(nameof(FinalStates))]
    public void FinalStates_RejectEveryTransition(IParcelState state)
    {
        Assert.True(state.IsFinal);
        Assert.Throws<InvalidOperationException>(() => state.Advance());
        Assert.Throws<InvalidOperationException>(() => state.Cancel());
    }

    public static TheoryData<IParcelState> FinalStates => new() { new DeliveredState(), new CancelledState() };

    [Fact]
    public void ChangeDestination_IsDecidedByTheCurrentState()
    {
        Parcel parcel = TestParcels.New();
        var newAddress = new Address("Str. 2", "Balti");

        parcel.ChangeDestination(newAddress);               // Registered: allowed
        Assert.Same(newAddress, parcel.Destination);

        parcel.Advance();
        parcel.Advance();                                   // InTransit: not allowed
        Assert.Throws<InvalidOperationException>(() => parcel.ChangeDestination(TestParcels.Home));
        Assert.Same(newAddress, parcel.Destination);
    }
}
