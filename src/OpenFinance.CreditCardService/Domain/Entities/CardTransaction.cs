using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CreditCardService.Domain.Entities;

/// <summary>
/// Represents a single transaction on a credit card bill.
/// </summary>
public class CardTransaction : Entity
{
    private CardTransaction() { }

    public static CardTransaction Create(
        Guid cardAccountId,
        Guid billId,
        string identificationNumber,
        string lineName,
        string transactionName,
        string billIdentification,
        CardTransactionType transactionType,
        decimal amount,
        string currency,
        DateTime transactionDateTime,
        decimal billPostDate,
        decimal payeeMcc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identificationNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(lineName);
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(billIdentification);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new CardTransaction
        {
            CardAccountId = cardAccountId,
            BillId = billId,
            IdentificationNumber = identificationNumber,
            LineName = lineName,
            TransactionName = transactionName,
            BillIdentification = billIdentification,
            TransactionType = transactionType,
            Amount = amount,
            Currency = currency,
            TransactionDateTime = transactionDateTime,
            BillPostDate = billPostDate,
            PayeeMCC = payeeMcc
        };
    }

    public Guid CardAccountId { get; private set; }
    public Guid BillId { get; private set; }
    public string IdentificationNumber { get; private set; } = default!;
    public string LineName { get; private set; } = default!;
    public string TransactionName { get; private set; } = default!;
    public string BillIdentification { get; private set; } = default!;
    public CardTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateTime TransactionDateTime { get; private set; }
    public decimal BillPostDate { get; private set; }
    public decimal PayeeMCC { get; private set; }
}
