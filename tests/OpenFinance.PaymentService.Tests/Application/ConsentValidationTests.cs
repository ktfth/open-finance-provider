using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Tests.Application;

public class ConsentValidationTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();

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
    public async Task InitiatePayment_WithInvalidConsent_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent not found"));

        var useCase = new InitiatePaymentUseCase(_repository.Object, _consentValidator.Object, NullLogger<InitiatePaymentUseCase>.Instance);
        var result = await useCase.ExecuteAsync(ValidRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent not found");
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitiatePayment_WithInsufficientPermissions_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InsufficientPermissions("Missing payment permission"));

        var useCase = new InitiatePaymentUseCase(_repository.Object, _consentValidator.Object, NullLogger<InitiatePaymentUseCase>.Instance);
        var result = await useCase.ExecuteAsync(ValidRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Missing");
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitiatePayment_WithServiceUnavailable_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.ServiceUnavailable("Consent service is unavailable"));

        var useCase = new InitiatePaymentUseCase(_repository.Object, _consentValidator.Object, NullLogger<InitiatePaymentUseCase>.Instance);
        var result = await useCase.ExecuteAsync(ValidRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unavailable");
        _repository.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
