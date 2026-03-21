using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CustomerService.Domain.Entities;

public class PersonalCustomer : Entity
{
    private PersonalCustomer() { }

    public static PersonalCustomer Create(
        string userId,
        string cpfNumber,
        string socialName,
        string birthDate,
        MaritalStatusType maritalStatus,
        SexType sex,
        string nationality,
        string birthCountry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(cpfNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(socialName);
        ArgumentException.ThrowIfNullOrWhiteSpace(birthDate);
        ArgumentException.ThrowIfNullOrWhiteSpace(nationality);
        ArgumentException.ThrowIfNullOrWhiteSpace(birthCountry);

        return new PersonalCustomer
        {
            UserId = userId,
            CpfNumber = cpfNumber,
            SocialName = socialName,
            BirthDate = birthDate,
            MaritalStatus = maritalStatus,
            Sex = sex,
            Nationality = nationality,
            BirthCountry = birthCountry
        };
    }

    public string UserId { get; private set; } = default!;
    public string CpfNumber { get; private set; } = default!;
    public string SocialName { get; private set; } = default!;
    public string BirthDate { get; private set; } = default!;
    public MaritalStatusType MaritalStatus { get; private set; }
    public SexType Sex { get; private set; }
    public string Nationality { get; private set; } = default!;
    public string BirthCountry { get; private set; } = default!;
}
