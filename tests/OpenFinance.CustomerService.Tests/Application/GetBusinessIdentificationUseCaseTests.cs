using FluentAssertions;
using Moq;
using OpenFinance.CustomerService.Application.UseCases;
using OpenFinance.CustomerService.Domain.Entities;
using OpenFinance.CustomerService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using DomainAddress = OpenFinance.CustomerService.Domain.Entities.CustomerAddress;
using DomainContact = OpenFinance.CustomerService.Domain.Entities.CustomerContact;

namespace OpenFinance.CustomerService.Tests.Application;

public class GetBusinessIdentificationUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetBusinessIdentificationUseCase _sut;

    public GetBusinessIdentificationUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetBusinessIdentificationUseCase(_repository.Object, _consentValidator.Object);
    }

    private static BusinessCustomer ValidBusinessCustomer() =>
        BusinessCustomer.Create(
            userId: "biz-user-001",
            cnpjNumber: "12.345.678/0001-99",
            companyName: "Acme Corporation Ltda",
            tradeName: "Acme Corp",
            incorporationDate: new DateTime(2010, 3, 15));

    [Fact]
    public async Task ExecuteAsync_WhenBusinessCustomerExists_ShouldReturnMappedResponse()
    {
        var customer = ValidBusinessCustomer();
        var consentId = Guid.NewGuid();

        _repository.Setup(r => r.GetBusinessByUserIdAsync("biz-user-001", default))
            .ReturnsAsync(customer);
        _repository.Setup(r => r.GetAddressesAsync("biz-user-001", default))
            .ReturnsAsync(Array.Empty<DomainAddress>());
        _repository.Setup(r => r.GetContactsAsync("biz-user-001", default))
            .ReturnsAsync(Array.Empty<DomainContact>());

        var result = await _sut.ExecuteAsync("biz-user-001", consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CnpjNumber.Should().Be("12.345.678/0001-99");
        result.Value.CompanyName.Should().Be("Acme Corporation Ltda");
        result.Value.TradeName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task ExecuteAsync_WhenBusinessCustomerNotFound_ShouldReturnSuccessWithNull()
    {
        _repository.Setup(r => r.GetBusinessByUserIdAsync("ghost-user", default))
            .ReturnsAsync((BusinessCustomer?)null);

        var result = await _sut.ExecuteAsync("ghost-user", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        _repository.Verify(r => r.GetAddressesAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBusinessCustomerExists_ShouldReturnEmptyPartnersCollection()
    {
        var customer = ValidBusinessCustomer();

        _repository.Setup(r => r.GetBusinessByUserIdAsync("biz-user-001", default))
            .ReturnsAsync(customer);
        _repository.Setup(r => r.GetAddressesAsync("biz-user-001", default))
            .ReturnsAsync(Array.Empty<DomainAddress>());
        _repository.Setup(r => r.GetContactsAsync("biz-user-001", default))
            .ReturnsAsync(Array.Empty<DomainContact>());

        var result = await _sut.ExecuteAsync("biz-user-001", Guid.NewGuid());

        result.Value!.Partners.Should().BeEmpty();
    }
}
