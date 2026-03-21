using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CustomerService.Domain.Entities;

public class PersonalQualification : Entity
{
    private PersonalQualification() { }

    public static PersonalQualification Create(
        string userId,
        OccupationType occupationType,
        string occupationDescription,
        InformedIncomeFrequency informedIncomeFrequency,
        decimal informedIncomeAmount,
        string informedIncomeCurrency,
        DateTime informedIncomeDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(occupationDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(informedIncomeCurrency);

        return new PersonalQualification
        {
            UserId = userId,
            OccupationType = occupationType,
            OccupationDescription = occupationDescription,
            InformedIncomeFrequency = informedIncomeFrequency,
            InformedIncomeAmount = informedIncomeAmount,
            InformedIncomeCurrency = informedIncomeCurrency,
            InformedIncomeDate = informedIncomeDate
        };
    }

    public string UserId { get; private set; } = default!;
    public OccupationType OccupationType { get; private set; }
    public string OccupationDescription { get; private set; } = default!;
    public InformedIncomeFrequency InformedIncomeFrequency { get; private set; }
    public decimal InformedIncomeAmount { get; private set; }
    public string InformedIncomeCurrency { get; private set; } = default!;
    public DateTime InformedIncomeDate { get; private set; }
}
