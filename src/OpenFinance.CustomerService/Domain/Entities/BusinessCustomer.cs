using OpenFinance.Shared.Domain;

namespace OpenFinance.CustomerService.Domain.Entities;

public class BusinessCustomer : Entity
{
    private BusinessCustomer() { }

    public static BusinessCustomer Create(
        string userId,
        string cnpjNumber,
        string companyName,
        string tradeName,
        DateTime incorporationDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(cnpjNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tradeName);

        return new BusinessCustomer
        {
            UserId = userId,
            CnpjNumber = cnpjNumber,
            CompanyName = companyName,
            TradeName = tradeName,
            IncorporationDate = incorporationDate
        };
    }

    public string UserId { get; private set; } = default!;
    public string CnpjNumber { get; private set; } = default!;
    public string CompanyName { get; private set; } = default!;
    public string TradeName { get; private set; } = default!;
    public DateTime IncorporationDate { get; private set; }
}
