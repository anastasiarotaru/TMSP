using Delivery.Behavioral.Command;
using Delivery.Domain;
using Delivery.Services;
using Delivery.Tests.Stubs;

namespace Delivery.Tests.Command;

public class CommandTests
{
    private readonly DeliveryService receiver = TestParcels.Receiver();
    private readonly CommandInvoker invoker = new();

    [Fact]
    public void AdvanceThenUndo_RestoresTheOriginalState()
    {
        Parcel parcel = TestParcels.Register(receiver);

        invoker.Execute(new AdvanceParcelCommand(receiver, parcel.TrackingId));
        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);

        invoker.Undo();
        Assert.Equal(ParcelStatus.Registered, parcel.Status);
        parcel.Advance();                                   // the restored state works normally again
        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);
    }

    [Fact]
    public void ChangeDestinationThenUndo_RestoresTheOldAddress()
    {
        Parcel parcel = TestParcels.Register(receiver);
        var newAddress = new Address("Str. 2", "Balti");

        invoker.Execute(new ChangeDestinationCommand(receiver, parcel.TrackingId, newAddress));
        Assert.Same(newAddress, parcel.Destination);

        invoker.Undo();
        Assert.Same(TestParcels.Home, parcel.Destination);
    }

    [Fact]
    public void Invoker_UndoesCommandsInReverseOrder()
    {
        Parcel parcel = TestParcels.Register(receiver);
        invoker.Execute(new AdvanceParcelCommand(receiver, parcel.TrackingId));   // PickedUp
        invoker.Execute(new CancelParcelCommand(receiver, parcel.TrackingId));    // Cancelled

        invoker.Undo();
        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);
        invoker.Undo();
        Assert.Equal(ParcelStatus.Registered, parcel.Status);
        Assert.False(invoker.CanUndo);
    }

    [Fact]
    public void RejectedCommand_IsNotAddedToTheHistory()
    {
        Parcel parcel = TestParcels.Register(receiver);
        invoker.Execute(new CancelParcelCommand(receiver, parcel.TrackingId));

        Assert.Throws<InvalidOperationException>(() =>
            invoker.Execute(new AdvanceParcelCommand(receiver, parcel.TrackingId)));   // Cancelled is final

        Assert.Single(invoker.History);
        invoker.Undo();                                     // undoes the cancel, not the failed advance
        Assert.Equal(ParcelStatus.Registered, parcel.Status);
    }

    [Fact]
    public void Undo_WithNothingDone_ReturnsFalse()
    {
        Assert.False(invoker.Undo());
    }
}
