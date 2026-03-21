namespace OpenFinance.Shared.Consent;

/// <summary>
/// Standard Open Finance Brasil permission constants.
/// Used to validate consent permissions before data access.
/// </summary>
public static class OpenFinancePermissions
{
    // Phase 2 - Accounts
    public const string AccountsRead = "ACCOUNTS_READ";
    public const string AccountsBalancesRead = "ACCOUNTS_BALANCES_READ";
    public const string AccountsTransactionsRead = "ACCOUNTS_TRANSACTIONS_READ";
    public const string AccountsOverdraftLimitsRead = "ACCOUNTS_OVERDRAFT_LIMITS_READ";

    // Phase 2 - Credit Cards
    public const string CreditCardsAccountsRead = "CREDIT_CARDS_ACCOUNTS_READ";
    public const string CreditCardsAccountsLimitsRead = "CREDIT_CARDS_ACCOUNTS_LIMITS_READ";
    public const string CreditCardsAccountsBillsRead = "CREDIT_CARDS_ACCOUNTS_BILLS_READ";
    public const string CreditCardsAccountsBillsTransactionsRead = "CREDIT_CARDS_ACCOUNTS_BILLS_TRANSACTIONS_READ";

    // Phase 2 - Customer
    public const string CustomersPersonalIdentificationsRead = "CUSTOMERS_PERSONAL_IDENTIFICATIONS_READ";
    public const string CustomersPersonalAdittionalInfoRead = "CUSTOMERS_PERSONAL_ADITTIONALINFO_READ";
    public const string CustomersBusinessIdentificationsRead = "CUSTOMERS_BUSINESS_IDENTIFICATIONS_READ";
    public const string CustomersBusinessAdittionalInfoRead = "CUSTOMERS_BUSINESS_ADITTIONALINFO_READ";

    // Phase 2 - Credit Operations
    public const string LoansRead = "LOANS_READ";
    public const string LoansWarrantiesRead = "LOANS_WARRANTIES_READ";
    public const string LoansScheduledInstalmentsRead = "LOANS_SCHEDULED_INSTALMENTS_READ";
    public const string LoansPaymentsRead = "LOANS_PAYMENTS_READ";
    public const string FinancingsRead = "FINANCINGS_READ";
    public const string FinancingsScheduledInstalmentsRead = "FINANCINGS_SCHEDULED_INSTALMENTS_READ";
    public const string FinancingsPaymentsRead = "FINANCINGS_PAYMENTS_READ";
    public const string UnarrangedAccountsOverdraftRead = "UNARRANGED_ACCOUNTS_OVERDRAFT_READ";

    // Phase 3 - Payments
    public const string PaymentsRead = "PAYMENTS_READ";
    public const string PaymentsInitiate = "PAYMENTS_INITIATE";

    // Phase 2/3 - Resources
    public const string ResourcesRead = "RESOURCES_READ";

    // Phase 4 - Investments
    public const string InvestmentsRead = "INVESTMENTS_READ";

    // Phase 4 - Insurance
    public const string InsurancesRead = "INSURANCES_READ";

    // Phase 4 - Pension
    public const string PensionsRead = "PENSIONS_READ";

    // Phase 4 - Capitalization
    public const string CapitalizationBondsRead = "CAPITALIZATION_BONDS_READ";

    // Phase 4 - Exchange
    public const string ExchangesRead = "EXCHANGES_READ";
}
