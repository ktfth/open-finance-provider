using FluentAssertions;
using Moq;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Tests.Application;

public class GetPaymentStatusUseCaseTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetPaymentStatusUseCase _sut;

    public GetPaymentStatusUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetPaymentStatusUseCase(_repository.Object, _consentValidator.Object);
    }

    private static Payment ValidPayment() =>
        Payment.Create(
            consentId: Guid.NewGuid(),
            debtorAccountId: "ACC-DEBTOR",
            creditorAccountId: "ACC-CREDITOR",
            creditorName: "Jane Doe",
            creditorCpfCnpj: "987.654.321-00",
            amount: 150m,
            currency: "BRL",
            description: "Test payment",
            type: PaymentType.Pix);

    [Fact]
    public async Task ExecuteAsync_WithValidConsentAndOwnedPayment_ShouldReturnPaymentResponse()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var payment = Payment.Create(
            consentId: consentId, // Same consent ID — this payment belongs to this consent
            debtorAccountId: "ACC-DEBTOR",
            creditorAccountId: "ACC-CREDITOR",
            creditorName: "Jane Doe",
            creditorCpfCnpj: "987.654.321-00",
            amount: 150m,
            currency: "BRL",
            description: "Test payment",
            type: PaymentType.Pix);
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync(payment);

        var result = await _sut.ExecuteAsync(consentId, paymentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Amount.Should().Be(150m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentBelongsToDifferentConsent_ShouldReturnFailure()
    {
        var requestingConsentId = Guid.NewGuid();
        var paymentConsentId = Guid.NewGuid(); // Different consent
        var paymentId = Guid.NewGuid();
        var payment = Payment.Create(
            consentId: paymentConsentId,
            debtorAccountId: "ACC-DEBTOR",
            creditorAccountId: "ACC-CREDITOR",
            creditorName: "Jane Doe",
            creditorCpfCnpj: "987.654.321-00",
            amount: 150m,
            currency: "BRL",
            description: "Test payment",
            type: PaymentType.Pix);
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync(payment);

        var result = await _sut.ExecuteAsync(requestingConsentId, paymentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CONSENT_MISMATCH");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentNotFound_ShouldReturnSuccessWithNullValue()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync((Payment?)null);

        var result = await _sut.ExecuteAsync(consentId, paymentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentInvalid_ShouldReturnFailureWithoutQueryingRepository()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _consentValidator
            .Setup(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent is expired."));

        var result = await _sut.ExecuteAsync(consentId, paymentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent is expired.");
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentServiceUnavailable_ShouldReturnFailure()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _consentValidator
            .Setup(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.ServiceUnavailable("Consent service is unavailable"));

        var result = await _sut.ExecuteAsync(consentId, paymentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unavailable");
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesConsentBeforeQueryingRepository()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _consentValidator
            .Setup(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InsufficientPermissions("Missing permission"));

        var result = await _sut.ExecuteAsync(consentId, paymentId);

        result.IsFailure.Should().BeTrue();
        _consentValidator.Verify(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
