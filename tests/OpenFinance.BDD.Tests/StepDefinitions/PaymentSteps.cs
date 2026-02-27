using FluentAssertions;
using OpenFinance.BDD.Tests.Support;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;
using TechTalk.SpecFlow;

namespace OpenFinance.BDD.Tests.StepDefinitions;

[Binding]
public class PaymentSteps
{
    private readonly InMemoryPaymentRepository _repository = new();
    private InitiatePaymentUseCase _initiateUseCase = default!;
    private CancelPaymentUseCase _cancelUseCase = default!;

    private Guid _consentId;
    private decimal _amount;
    private string _currency = "BRL";
    private string _paymentType = string.Empty;
    private string _debtorAccount = string.Empty;
    private string _creditorAccount = string.Empty;
    private string _creditorName = string.Empty;
    private Result<PaymentResponse>? _initiateResult;
    private Result? _cancelResult;
    private Payment? _currentPayment;

    [BeforeScenario]
    public void Setup()
    {
        _initiateUseCase = new InitiatePaymentUseCase(_repository);
        _cancelUseCase = new CancelPaymentUseCase(_repository);
    }

    [Given("the payment service is available")]
    public void GivenPaymentServiceIsAvailable() { }

    [Given("a valid consent id exists")]
    public void GivenValidConsentIdExists() => _consentId = Guid.NewGuid();

    [Given(@"a payment request with amount (.*) in ""(.*)""")]
    public void GivenPaymentRequestWithAmount(string amount, string currency)
    {
        _amount = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);
        _currency = currency;
    }

    [Given(@"the payment type is ""(.*)""")]
    public void GivenPaymentTypeIs(string type) => _paymentType = type;

    [Given(@"the debtor account is ""(.*)""")]
    public void GivenDebtorAccountIs(string account) => _debtorAccount = account;

    [Given(@"the creditor account is ""(.*)""")]
    public void GivenCreditorAccountIs(string account) => _creditorAccount = account;

    [Given(@"the creditor name is ""(.*)""")]
    public void GivenCreditorNameIs(string name) => _creditorName = name;

    [When("I initiate the payment")]
    public async Task WhenIInitiatePayment()
    {
        var type = Enum.Parse<PaymentType>(_paymentType);
        var request = new InitiatePaymentRequest(
            _consentId, _debtorAccount, _creditorAccount,
            _creditorName, "000.000.000-00",
            _amount, _currency, "Test payment", type);
        _initiateResult = await _initiateUseCase.ExecuteAsync(request);
    }

    [Then("the payment is created successfully")]
    public void ThenPaymentIsCreatedSuccessfully() =>
        _initiateResult!.IsSuccess.Should().BeTrue();

    [Then(@"the payment status is ""(.*)""")]
    public async Task ThenPaymentStatusIs(string expectedStatus)
    {
        if (_currentPayment is not null)
        {
            _currentPayment.Status.ToString().Should().Be(expectedStatus);
            return;
        }

        var paymentId = _initiateResult?.Value?.PaymentId ?? Guid.Empty;
        var payment = await _repository.GetByIdAsync(paymentId);
        payment!.Status.ToString().Should().Be(expectedStatus);
    }

    [Then("the payment has a valid id")]
    public void ThenPaymentHasValidId() =>
        _initiateResult!.Value.PaymentId.Should().NotBeEmpty();

    [Then("the payment initiation fails")]
    public void ThenPaymentInitiationFails() =>
        _initiateResult!.IsFailure.Should().BeTrue();

    [Given("a pending payment exists")]
    public async Task GivenPendingPaymentExists()
    {
        var request = new InitiatePaymentRequest(
            Guid.NewGuid(), "ACC-D", "ACC-C", "Creditor", "cpf", 100m, "BRL", "test", PaymentType.Pix);
        var result = await _initiateUseCase.ExecuteAsync(request);
        _currentPayment = await _repository.GetByIdAsync(result.Value.PaymentId);
    }

    [Given("a completed payment exists")]
    public async Task GivenCompletedPaymentExists()
    {
        var request = new InitiatePaymentRequest(
            Guid.NewGuid(), "ACC-D", "ACC-C", "Creditor", "cpf", 100m, "BRL", "test", PaymentType.Pix);
        var result = await _initiateUseCase.ExecuteAsync(request);
        _currentPayment = await _repository.GetByIdAsync(result.Value.PaymentId);
        _currentPayment!.MarkProcessing();
        _currentPayment.Complete();
        await _repository.UpdateAsync(_currentPayment);
    }

    [When(@"I cancel the payment with reason ""(.*)""")]
    public async Task WhenICancelPaymentWithReason(string reason)
    {
        _cancelResult = await _cancelUseCase.ExecuteAsync(_currentPayment!.Id, reason);
        _currentPayment = await _repository.GetByIdAsync(_currentPayment.Id);
    }

    [Then("the cancellation fails with an error")]
    public void ThenCancellationFailsWithError() =>
        _cancelResult!.IsFailure.Should().BeTrue();
}
