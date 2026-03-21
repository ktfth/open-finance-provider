using FluentAssertions;
using Moq;
using OpenFinance.AccountService.Application.UseCases;
using OpenFinance.AccountService.Domain.Entities;
using OpenFinance.BDD.Tests.Support;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;
using TechTalk.SpecFlow;

namespace OpenFinance.BDD.Tests.StepDefinitions;

[Binding]
public class AccountSteps
{
    private readonly InMemoryAccountRepository _repository = new();
    private GetAccountsUseCase _getAccountsUseCase = default!;
    private GetTransactionsUseCase _getTransactionsUseCase = default!;

    private Guid _consentId;
    private Account? _currentAccount;
    private Result<AccountListResponse>? _accountsResult;
    private Result<TransactionListResponse>? _transactionsResult;

    [BeforeScenario]
    public void Setup()
    {
        var consentValidator = new Mock<IConsentValidator>();
        consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _getAccountsUseCase = new GetAccountsUseCase(_repository, consentValidator.Object);
        _getTransactionsUseCase = new GetTransactionsUseCase(_repository, consentValidator.Object);
    }

    [Given("the account service is available")]
    public void GivenAccountServiceIsAvailable() { }

    [Given("a valid consent id for account access exists")]
    public void GivenValidConsentIdExists() => _consentId = Guid.NewGuid();

    [Given(@"user ""(.*)"" has (\d+) active accounts")]
    public async Task GivenUserHasActiveAccounts(string userId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var account = Account.Create(userId, $"ACC{i:D6}", "0001", AccountType.Checking, "BRL", "Owner", "12345678901");
            await _repository.AddAsync(account);
        }
    }

    [When(@"I request accounts for user ""(.*)""")]
    public async Task WhenIRequestAccountsForUser(string userId)
    {
        _accountsResult = await _getAccountsUseCase.ExecuteAsync(userId, _consentId);
    }

    [Then(@"the response contains (\d+) accounts")]
    public void ThenResponseContainsAccounts(int count) =>
        _accountsResult!.Value.Accounts.Should().HaveCount(count);

    [Given(@"an account exists with id for user ""(.*)""")]
    public async Task GivenAccountExistsForUser(string userId)
    {
        _currentAccount = Account.Create(userId, "ACC999999", "0001", AccountType.Checking, "BRL", "Owner", "98765432100");
        await _repository.AddAsync(_currentAccount);
    }

    [Given(@"the account has (\d+) transactions in the last (\d+) days")]
    public void GivenAccountHasTransactions(int count, int days)
    {
        for (int i = 0; i < count; i++)
        {
            var tx = Transaction.Create(
                _currentAccount!.Id,
                100m * (i + 1),
                "BRL",
                "CREDIT",
                $"Transaction {i + 1}",
                DateTime.UtcNow.AddDays(-(i + 1)));
            _repository.AddTransaction(tx);
        }
    }

    [When(@"I request transactions from (\d+) days ago to today")]
    public async Task WhenIRequestTransactions(int daysAgo)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-daysAgo));
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        _transactionsResult = await _getTransactionsUseCase.ExecuteAsync(_currentAccount!.Id, _consentId, from, to);
    }

    [Then(@"the response contains (\d+) transactions")]
    public void ThenResponseContainsTransactions(int count) =>
        _transactionsResult!.Value.Transactions.Should().HaveCount(count);

    [When("I request transactions with from date after to date")]
    public async Task WhenIRequestTransactionsWithInvalidRange()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        _transactionsResult = await _getTransactionsUseCase.ExecuteAsync(
            _currentAccount?.Id ?? Guid.NewGuid(), _consentId, from, to);
    }

    [Then("the transaction request fails with an error")]
    public void ThenTransactionRequestFails() =>
        _transactionsResult!.IsFailure.Should().BeTrue();
}
