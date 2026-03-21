using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Tests.Application;

public class CancelPaymentUseCaseTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly CancelPaymentUseCase _sut;

    public CancelPaymentUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new CancelPaymentUseCase(_repository.Object, _consentValidator.Object, NullLogger<CancelPaymentUseCase>.Instance);
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
    public async Task ExecuteAsync_WithValidConsentAndOwnedPendingPayment_ShouldCancelSuccessfully()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var payment = CreatePaymentWithConsent(consentId);
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync(payment);
        _repository.Setup(r => r.UpdateAsync(payment, default)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(consentId, paymentId, "User cancelled");

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.UpdateAsync(payment, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentBelongsToDifferentConsent_ShouldReturnFailure()
    {
        var requestingConsentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var payment = ValidPayment(); // Different consent
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync(payment);

        var result = await _sut.ExecuteAsync(requestingConsentId, paymentId, "User cancelled");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CONSENT_MISMATCH");
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Payment CreatePaymentWithConsent(Guid consentId) =>
        Payment.Create(
            consentId: consentId,
            debtorAccountId: "ACC-DEBTOR",
            creditorAccountId: "ACC-CREDITOR",
            creditorName: "Jane Doe",
            creditorCpfCnpj: "987.654.321-00",
            amount: 150m,
            currency: "BRL",
            description: "Test payment",
            type: PaymentType.Pix);

    [Fact]
    public async Task ExecuteAsync_WhenPaymentNotFound_ShouldReturnFailure()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync((Payment?)null);

        var result = await _sut.ExecuteAsync(consentId, paymentId, "User cancelled");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentInvalid_ShouldReturnFailureWithoutQueryingRepository()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _consentValidator
            .Setup(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent is expired."));

        var result = await _sut.ExecuteAsync(consentId, paymentId, "User cancelled");

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

        var result = await _sut.ExecuteAsync(consentId, paymentId, "User cancelled");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unavailable");
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentAlreadyCompleted_ShouldReturnFailure()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var payment = CreatePaymentWithConsent(consentId);
        payment.MarkProcessing();
        payment.Complete();
        _repository.Setup(r => r.GetByIdAsync(paymentId, default)).ReturnsAsync(payment);

        var result = await _sut.ExecuteAsync(consentId, paymentId, "Too late");

        result.IsFailure.Should().BeTrue();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesConsentBeforeQueryingRepository()
    {
        var consentId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _consentValidator
            .Setup(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InsufficientPermissions("Missing permission"));

        var result = await _sut.ExecuteAsync(consentId, paymentId, "User cancelled");

        result.IsFailure.Should().BeTrue();
        _consentValidator.Verify(v => v.ValidateAsync(consentId, It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
