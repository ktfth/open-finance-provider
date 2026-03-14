using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CreditCardService.Domain.Entities;

/// <summary>
/// Represents a credit card account held by a customer.
/// Maps to the Open Finance Brasil Credit Card Account resource.
/// </summary>
public class CardAccount : Entity
{
    private CardAccount() { }

    public static CardAccount Create(
        string userId,
        string lastFourDigits,
        CardBrand brand,
        CardType cardType,
        CardNetworkType networkType,
        string holderName,
        string holderCpf,
        int paymentDay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastFourDigits);
        ArgumentException.ThrowIfNullOrWhiteSpace(holderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(holderCpf);

        if (lastFourDigits.Length != 4 || !lastFourDigits.All(char.IsDigit))
            throw new ArgumentException("lastFourDigits must be exactly 4 numeric characters.", nameof(lastFourDigits));

        if (paymentDay is < 1 or > 31)
            throw new ArgumentException("paymentDay must be between 1 and 31.", nameof(paymentDay));

        return new CardAccount
        {
            UserId = userId,
            LastFourDigits = lastFourDigits,
            Brand = brand,
            CardType = cardType,
            NetworkType = networkType,
            HolderName = holderName,
            HolderCpf = holderCpf,
            PaymentDay = paymentDay,
            Status = CardAccountStatus.Active
        };
    }

    public string UserId { get; private set; } = default!;
    public string LastFourDigits { get; private set; } = default!;
    public CardBrand Brand { get; private set; }
    public CardType CardType { get; private set; }
    public CardNetworkType NetworkType { get; private set; }
    public string HolderName { get; private set; } = default!;
    public string HolderCpf { get; private set; } = default!;
    public int PaymentDay { get; private set; }
    public CardAccountStatus Status { get; private set; }

    public void Block()
    {
        if (Status == CardAccountStatus.Cancelled)
            throw new InvalidOperationException("Cannot block a cancelled card.");
        Status = CardAccountStatus.Blocked;
        SetUpdated();
    }

    public void Unblock()
    {
        if (Status != CardAccountStatus.Blocked)
            throw new InvalidOperationException("Card is not blocked.");
        Status = CardAccountStatus.Active;
        SetUpdated();
    }

    public void Cancel()
    {
        if (Status == CardAccountStatus.Cancelled)
            throw new InvalidOperationException("Card is already cancelled.");
        Status = CardAccountStatus.Cancelled;
        SetUpdated();
    }

    public bool IsActive() => Status == CardAccountStatus.Active;
}
