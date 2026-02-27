using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.AccountService.Domain.Entities;

public class Account : Entity
{
    private Account() { }

    public static Account Create(
        string userId,
        string accountNumber,
        string branchCode,
        AccountType type,
        string currency,
        string ownerName,
        string cpf)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(branchCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cpf);

        return new Account
        {
            UserId = userId,
            AccountNumber = accountNumber,
            BranchCode = branchCode,
            Type = type,
            Currency = currency,
            OwnerName = ownerName,
            Cpf = cpf
        };
    }

    public string UserId { get; private set; } = default!;
    public string AccountNumber { get; private set; } = default!;
    public string BranchCode { get; private set; } = default!;
    public AccountType Type { get; private set; }
    public string Currency { get; private set; } = default!;
    public string OwnerName { get; private set; } = default!;
    public string Cpf { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
