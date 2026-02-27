using FluentAssertions;
using Moq;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Tests.Application;

public class InitiatePaymentUseCaseTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly InitiatePaymentUseCase _sut;

    public InitiatePaymentUseCaseTests() =>
        _sut = new InitiatePaymentUseCase(_repository.Object);

    private static InitiatePaymentRequest ValidRequest() => new(
        ConsentId: Guid.NewGuid(),
        DebtorAccountId: "ACC-001",
        CreditorAccountId: "ACC-002",
        CreditorName: "Jane Doe",
        CreditorCpfCnpj: "987.654.321-00",
        Amount: 200m,
        Currency: "BRL",
        Description: "Test payment",
        Type: PaymentType.Pix);

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldCreatePendingPayment()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<Payment>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PaymentStatus.Pending);
        result.Value.Amount.Should().Be(200m);
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeAmount_ShouldReturnFailure()
    {
        var request = ValidRequest() with { Amount = -10m };
        var result = await _sut.ExecuteAsync(request);
        result.IsFailure.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyDebtorAccount_ShouldReturnFailure()
    {
        var request = ValidRequest() with { DebtorAccountId = "" };
        var result = await _sut.ExecuteAsync(request);
        result.IsFailure.Should().BeTrue();
    }
}
