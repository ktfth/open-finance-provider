using FluentAssertions;
using OpenFinance.CustomerService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CustomerService.Tests.Domain;

public class PersonalCustomerTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePersonalCustomer()
    {
        var customer = PersonalCustomer.Create(
            "user-001",
            "123.456.789-00",
            "Jane Doe",
            "1985-07-20",
            MaritalStatusType.Married,
            SexType.Female,
            "Brazilian",
            "Brazil");

        customer.CpfNumber.Should().Be("123.456.789-00");
        customer.SocialName.Should().Be("Jane Doe");
        customer.MaritalStatus.Should().Be(MaritalStatusType.Married);
        customer.BirthCountry.Should().Be("Brazil");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        var act = () => PersonalCustomer.Create(
            "",
            "123.456.789-00",
            "Jane Doe",
            "1985-07-20",
            MaritalStatusType.Single,
            SexType.Female,
            "Brazilian",
            "Brazil");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyCpf_ShouldThrow()
    {
        var act = () => PersonalCustomer.Create(
            "user-001",
            "",
            "Jane Doe",
            "1985-07-20",
            MaritalStatusType.Single,
            SexType.Female,
            "Brazilian",
            "Brazil");

        act.Should().Throw<ArgumentException>();
    }
}
