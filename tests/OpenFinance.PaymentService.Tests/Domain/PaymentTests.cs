using FluentAssertions;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Tests.Domain;

public class PaymentTests
{
    private static Payment ValidPayment() =>
        Payment.Create(
            consentId: Guid.NewGuid(),
            debtorAccountId: "ACC-DEBTOR",
            creditorAccountId: "ACC-CREDITOR",
            creditorName: "Jane Doe",
            creditorCpfCnpj: "987.654.321-00",
            amount: 150.00m,
            currency: "BRL",
            description: "Rent payment",
            type: PaymentType.Pix);

    [Fact]
    public void Create_WithValidData_ShouldCreatePendingPayment()
    {
        var payment = ValidPayment();

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(150m);
        payment.Currency.Should().Be("BRL");
        payment.Type.Should().Be(PaymentType.Pix);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        var act = () => Payment.Create(Guid.NewGuid(), "d", "c", "name", "cpf", 0, "BRL", "", PaymentType.Pix);
        act.Should().Throw<ArgumentException>().WithMessage("*Amount*");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Payment.Create(Guid.NewGuid(), "d", "c", "name", "cpf", -10, "BRL", "", PaymentType.Pix);
        act.Should().Throw<ArgumentException>().WithMessage("*Amount*");
    }

    [Fact]
    public void MarkProcessing_WhenPending_ShouldChangeStatus()
    {
        var payment = ValidPayment();
        payment.MarkProcessing();
        payment.Status.Should().Be(PaymentStatus.Processing);
    }

    [Fact]
    public void Complete_WhenProcessing_ShouldCompletePayment()
    {
        var payment = ValidPayment();
        payment.MarkProcessing();
        payment.Complete();
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenNotProcessing_ShouldThrow()
    {
        var payment = ValidPayment();
        var act = () => payment.Complete();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_WhenPending_ShouldSetFailedStatus()
    {
        var payment = ValidPayment();
        payment.Fail("Insufficient funds");
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Fail_WhenCompleted_ShouldThrow()
    {
        var payment = ValidPayment();
        payment.MarkProcessing();
        payment.Complete();
        var act = () => payment.Fail("late fail");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenPending_ShouldCancelPayment()
    {
        var payment = ValidPayment();
        payment.Cancel("User cancelled");
        payment.Status.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrow()
    {
        var payment = ValidPayment();
        payment.MarkProcessing();
        payment.Complete();
        var act = () => payment.Cancel("too late");
        act.Should().Throw<InvalidOperationException>();
    }
}
