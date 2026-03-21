using FluentAssertions;
using Moq;
using OpenFinance.CustomerService.Application.UseCases;
using OpenFinance.CustomerService.Domain.Entities;
using OpenFinance.CustomerService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using DomainAddress = OpenFinance.CustomerService.Domain.Entities.CustomerAddress;
using DomainContact = OpenFinance.CustomerService.Domain.Entities.CustomerContact;

namespace OpenFinance.CustomerService.Tests.Application;

public class GetPersonalIdentificationUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetPersonalIdentificationUseCase _sut;

    public GetPersonalIdentificationUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetPersonalIdentificationUseCase(_repository.Object, _consentValidator.Object);
    }

    private static PersonalCustomer ValidPersonalCustomer() =>
        PersonalCustomer.Create(
            userId: "user-001",
            cpfNumber: "123.456.789-00",
            socialName: "John Doe",
            birthDate: "1990-01-15",
            maritalStatus: MaritalStatusType.Single,
            sex: SexType.Male,
            nationality: "Brazilian",
            birthCountry: "Brazil");

    [Fact]
    public async Task ExecuteAsync_WhenPersonalCustomerExists_ShouldReturnMappedResponse()
    {
        var customer = ValidPersonalCustomer();
        var consentId = Guid.NewGuid();

        _repository.Setup(r => r.GetPersonalByUserIdAsync("user-001", default))
            .ReturnsAsync(customer);
        _repository.Setup(r => r.GetAddressesAsync("user-001", default))
            .ReturnsAsync(Array.Empty<DomainAddress>());
        _repository.Setup(r => r.GetContactsAsync("user-001", default))
            .ReturnsAsync(Array.Empty<DomainContact>());

        var result = await _sut.ExecuteAsync("user-001", consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CpfNumber.Should().Be("123.456.789-00");
        result.Value.SocialName.Should().Be("John Doe");
        result.Value.MaritalStatus.Should().Be(MaritalStatusType.Single);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPersonalCustomerNotFound_ShouldReturnSuccessWithNull()
    {
        _repository.Setup(r => r.GetPersonalByUserIdAsync("unknown-user", default))
            .ReturnsAsync((PersonalCustomer?)null);

        var result = await _sut.ExecuteAsync("unknown-user", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerHasAddressesAndContacts_ShouldMapThemToResponse()
    {
        var customer = ValidPersonalCustomer();
        var consentId = Guid.NewGuid();

        var address = DomainAddress.Create(
            "user-001", "Main St", "100", null, "Downtown", "São Paulo", "SP", "01310-000", "Brazil", AddressType.Residential);
        var contact = DomainContact.Create(
            "user-001", PhoneType.Mobile, "55", "11", "999999999", "john@example.com", true);

        _repository.Setup(r => r.GetPersonalByUserIdAsync("user-001", default)).ReturnsAsync(customer);
        _repository.Setup(r => r.GetAddressesAsync("user-001", default))
            .ReturnsAsync(new DomainAddress[] { address });
        _repository.Setup(r => r.GetContactsAsync("user-001", default))
            .ReturnsAsync(new DomainContact[] { contact });

        var result = await _sut.ExecuteAsync("user-001", consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Addresses.Should().HaveCount(1);
        result.Value.Phones.Should().HaveCount(1);
        result.Value.Emails.Should().HaveCount(1);
    }
}
