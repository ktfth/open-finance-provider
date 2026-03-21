namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for customer data services provided to bank participants.
/// Aligned with the Open Finance Brasil Customers API specification (Phase 2).
/// Covers personal identification, qualifications, and financial relations.
/// </summary>
public interface ICustomerContract
{
    Task<PersonalIdentificationResponse?> GetPersonalIdentificationAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<PersonalQualificationResponse?> GetPersonalQualificationAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<PersonalFinancialRelationResponse?> GetPersonalFinancialRelationAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<BusinessIdentificationResponse?> GetBusinessIdentificationAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<BusinessQualificationResponse?> GetBusinessQualificationAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<BusinessFinancialRelationResponse?> GetBusinessFinancialRelationAsync(string userId, Guid consentId, CancellationToken ct = default);
}

// ─── Personal ───────────────────────────────────────────────────────────────

public record PersonalIdentificationResponse(
    string UserId,
    string CpfNumber,
    string SocialName,
    string BirthDate,
    MaritalStatusType MaritalStatus,
    SexType Sex,
    string Nationality,
    string BirthCountry,
    IReadOnlyList<CustomerDocument> Documents,
    IReadOnlyList<CustomerAddress> Addresses,
    IReadOnlyList<CustomerPhone> Phones,
    IReadOnlyList<CustomerEmail> Emails
);

public record PersonalQualificationResponse(
    string UserId,
    OccupationType OccupationType,
    string OccupationDescription,
    InformedIncomeFrequency InformedIncomeFrequency,
    decimal InformedIncomeAmount,
    string InformedIncomeCurrency,
    DateTime InformedIncomeDate
);

public record PersonalFinancialRelationResponse(
    string UserId,
    DateTime StartDate,
    IReadOnlyList<string> ProductsServicesType,
    IReadOnlyList<AccountFinancialRelation> Accounts,
    IReadOnlyList<CustomerProcurator> Procurators
);

// ─── Business ────────────────────────────────────────────────────────────────

public record BusinessIdentificationResponse(
    string UserId,
    string CnpjNumber,
    string CompanyName,
    string TradeName,
    DateTime IncorporationDate,
    IReadOnlyList<CustomerAddress> Addresses,
    IReadOnlyList<CustomerPhone> Phones,
    IReadOnlyList<CustomerEmail> Emails,
    IReadOnlyList<BusinessPartner> Partners
);

public record BusinessQualificationResponse(
    string UserId,
    string EconomicActivityCode,
    string EconomicActivityDescription,
    decimal InformedRevenueAmount,
    string InformedRevenueCurrency,
    InformedIncomeFrequency InformedRevenueFrequency,
    DateTime InformedRevenueDate
);

public record BusinessFinancialRelationResponse(
    string UserId,
    DateTime StartDate,
    IReadOnlyList<string> ProductsServicesType,
    IReadOnlyList<AccountFinancialRelation> Accounts,
    IReadOnlyList<CustomerProcurator> Procurators
);

// ─── Shared Records ──────────────────────────────────────────────────────────

public record CustomerDocument(
    DocumentType Type,
    string Number,
    string? IssuingCountry,
    DateOnly? ExpirationDate
);

public record CustomerAddress(
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string PostalCode,
    string Country,
    AddressType AddressType
);

public record CustomerPhone(
    PhoneType PhoneType,
    string CountryCallingCode,
    string AreaCode,
    string Number
);

public record CustomerEmail(
    string Email,
    bool IsMain
);

public record AccountFinancialRelation(
    string BranchCode,
    string AccountNumber,
    AccountType AccountType
);

public record CustomerProcurator(
    string CpfNumber,
    string SocialName,
    ProcuratorType Type
);

public record BusinessPartner(
    string CpfCnpj,
    string Name,
    string PartnerType,
    decimal ParticipationPercentage
);

// ─── Enumerations ────────────────────────────────────────────────────────────

public enum MaritalStatusType
{
    Single,
    Married,
    Widowed,
    Separated,
    Divorced,
    CivilUnion,
    Other
}

public enum SexType
{
    Male,
    Female,
    Other
}

public enum OccupationType
{
    Employee,
    SelfEmployed,
    Retired,
    Unemployed,
    BusinessOwner,
    Student,
    Other
}

public enum InformedIncomeFrequency
{
    Daily,
    Weekly,
    Biweekly,
    Monthly,
    Bimonthly,
    Quarterly,
    Semiannual,
    Annual,
    Other
}

public enum DocumentType
{
    CPF,
    CNPJ,
    RG,
    Passport,
    CNH,
    Other
}

public enum AddressType
{
    Residential,
    Commercial,
    Other
}

public enum PhoneType
{
    Home,
    Mobile,
    Business,
    Other
}

public enum ProcuratorType
{
    Representative,
    Attorney,
    Other
}
