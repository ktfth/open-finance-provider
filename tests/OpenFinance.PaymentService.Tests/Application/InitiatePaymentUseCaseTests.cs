using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Tests.Application;

public class InitiatePaymentUseCaseTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly InitiatePaymentUseCase _sut;

    public InitiatePaymentUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new InitiatePaymentUseCase(_repository.Object, _consentValidator.Object, NullLogger<InitiatePaymentUseCase>.Instance);
    }

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

    [Fact]
    public async Task ExecuteAsync_WhenConsentInvalid_ShouldReturnFailureWithoutCreatingPayment()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent is expired."));

        var result = await _sut.ExecuteAsync(ValidRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent is expired.");
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateIdempotencyKey_ShouldReturnExistingPayment()
    {
        var existingPayment = Payment.Create(
            Guid.NewGuid(), "ACC-001", "ACC-002", "Jane Doe", "987.654.321-00",
            200m, "BRL", "Existing payment", PaymentType.Pix, "idem-key-123");

        _repository
            .Setup(r => r.GetByIdempotencyKeyAsync("idem-key-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var result = await _sut.ExecuteAsync(ValidRequest(), "idem-key-123");

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().Be(existingPayment.Id);
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNewIdempotencyKey_ShouldCreatePayment()
    {
        _repository
            .Setup(r => r.GetByIdempotencyKeyAsync("new-key-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        _repository.Setup(r => r.AddAsync(It.IsAny<Payment>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(ValidRequest(), "new-key-456");

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullIdempotencyKey_ShouldSkipCheckAndCreatePayment()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<Payment>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(ValidRequest(), null);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), default), Times.Once);
    }
}
