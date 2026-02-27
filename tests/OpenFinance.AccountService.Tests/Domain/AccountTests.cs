using FluentAssertions;
using OpenFinance.AccountService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.AccountService.Tests.Domain;

public class AccountTests
{
    private static Account ValidAccount() =>
        Account.Create("user-1", "00012345-6", "0001", AccountType.Checking, "BRL", "John Doe", "123.456.789-00");

    [Fact]
    public void Create_WithValidData_ShouldCreateActiveAccount()
    {
        var account = ValidAccount();

        account.UserId.Should().Be("user-1");
        account.AccountNumber.Should().Be("00012345-6");
        account.BranchCode.Should().Be("0001");
        account.Type.Should().Be(AccountType.Checking);
        account.Currency.Should().Be("BRL");
        account.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "branch", "owner", "cpf")]
    [InlineData("accnum", "", "owner", "cpf")]
    [InlineData("accnum", "branch", "", "cpf")]
    [InlineData("accnum", "branch", "owner", "")]
    public void Create_WithMissingRequiredField_ShouldThrow(
        string accountNumber, string branchCode, string ownerName, string cpf)
    {
        var act = () => Account.Create("user", accountNumber, branchCode, AccountType.Checking, "BRL", ownerName, cpf);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var account = ValidAccount();
        account.Deactivate();
        account.IsActive.Should().BeFalse();
        account.UpdatedAt.Should().NotBeNull();
    }
}
