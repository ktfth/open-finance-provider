using FluentAssertions;
using OpenFinance.BDD.Tests.Support;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;
using TechTalk.SpecFlow;

namespace OpenFinance.BDD.Tests.StepDefinitions;

[Binding]
public class ConsentSteps
{
    private readonly InMemoryConsentRepository _repository = new();
    private CreateConsentUseCase _createUseCase = default!;
    private RevokeConsentUseCase _revokeUseCase = default!;
    private ValidateConsentUseCase _validateUseCase = default!;

    private string _clientId = string.Empty;
    private string _userId = string.Empty;
    private string[] _permissions = [];
    private DateTime _expiresAt;
    private Result<ConsentResponse>? _createResult;
    private Consent? _currentConsent;
    private bool _validationResult;

    [BeforeScenario]
    public void Setup()
    {
        _createUseCase = new CreateConsentUseCase(_repository);
        _revokeUseCase = new RevokeConsentUseCase(_repository);
        _validateUseCase = new ValidateConsentUseCase(_repository);
    }

    [Given("the consent service is available")]
    public void GivenConsentServiceIsAvailable() { }

    [Given(@"a bank client with id ""(.*)""")]
    public void GivenBankClientWithId(string clientId) => _clientId = clientId;

    [Given(@"a user with id ""(.*)""")]
    public void GivenUserWithId(string userId) => _userId = userId;

    [Given(@"the requested permissions are ""(.*)""")]
    public void GivenRequestedPermissions(string permissions) =>
        _permissions = string.IsNullOrEmpty(permissions)
            ? []
            : permissions.Split(',');

    [Given(@"the consent expires in (\d+) days")]
    public void GivenConsentExpiresIn(int days) => _expiresAt = DateTime.UtcNow.AddDays(days);

    [When("I create the consent")]
    public async Task WhenICreateTheConsent()
    {
        var request = new CreateConsentRequest(_clientId, _userId, _permissions, _expiresAt);
        _createResult = await _createUseCase.ExecuteAsync(request);
    }

    [Then("the consent is created successfully")]
    public void ThenConsentIsCreatedSuccessfully() =>
        _createResult!.IsSuccess.Should().BeTrue();

    [Then(@"the consent status is ""(.*)""")]
    public async Task ThenConsentStatusIs(string expectedStatus)
    {
        if (_currentConsent is not null)
        {
            _currentConsent.Status.ToString().Should().Be(expectedStatus);
            return;
        }

        var consentId = _createResult?.Value?.ConsentId ?? Guid.Empty;
        var consent = await _repository.GetByIdAsync(consentId);
        consent!.Status.ToString().Should().Be(expectedStatus);
    }

    [Then("the consent has a valid id")]
    public void ThenConsentHasValidId() =>
        _createResult!.Value.ConsentId.Should().NotBeEmpty();

    [Then("the consent creation fails")]
    public void ThenConsentCreationFails() =>
        _createResult!.IsFailure.Should().BeTrue();

    [Then(@"the error contains ""(.*)""")]
    public void ThenErrorContains(string errorFragment) =>
        _createResult!.Error.Should().Contain(errorFragment);

    [Given(@"a pending consent exists for client ""(.*)"" and user ""(.*)""")]
    public async Task GivenPendingConsentExistsForClientAndUser(string clientId, string userId)
    {
        var request = new CreateConsentRequest(clientId, userId, ["ACCOUNTS_READ"], DateTime.UtcNow.AddDays(30));
        var result = await _createUseCase.ExecuteAsync(request);
        _currentConsent = await _repository.GetByIdAsync(result.Value.ConsentId);
    }

    [When("the consent is authorised")]
    public async Task WhenConsentIsAuthorised()
    {
        _currentConsent!.Authorise();
        await _repository.UpdateAsync(_currentConsent);
    }

    [When(@"the consent is revoked with reason ""(.*)""")]
    public async Task WhenConsentIsRevokedWithReason(string reason)
    {
        await _revokeUseCase.ExecuteAsync(_currentConsent!.Id, reason);
        _currentConsent = await _repository.GetByIdAsync(_currentConsent.Id);
    }

    [Given(@"an authorised consent with permissions ""(.*)""")]
    public async Task GivenAuthorisedConsentWithPermissions(string permissions)
    {
        var perms = permissions.Split(',');
        var request = new CreateConsentRequest("bank-test", "user-test", perms, DateTime.UtcNow.AddDays(30));
        var result = await _createUseCase.ExecuteAsync(request);
        _currentConsent = await _repository.GetByIdAsync(result.Value.ConsentId);
        _currentConsent!.Authorise();
        await _repository.UpdateAsync(_currentConsent);
    }

    [When(@"I validate the consent for permissions ""(.*)""")]
    public async Task WhenIValidateConsentForPermissions(string permissions)
    {
        var perms = permissions.Split(',');
        var result = await _validateUseCase.ExecuteAsync(_currentConsent!.Id, perms);
        _validationResult = result.Value;
    }

    [Then(@"the consent validation result is ""(.*)""")]
    public void ThenConsentValidationResultIs(string expected) =>
        _validationResult.Should().Be(bool.Parse(expected));
}
