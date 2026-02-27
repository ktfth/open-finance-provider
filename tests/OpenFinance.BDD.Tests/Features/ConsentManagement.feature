Feature: Consent Management
    As a bank participant integrated with the Open Finance Provider
    I want to manage customer consents
    So that customers can authorize sharing of their financial data

Background:
    Given the consent service is available

Scenario: Create a valid consent
    Given a bank client with id "bank-001"
    And a user with id "user-123"
    And the requested permissions are "ACCOUNTS_READ,TRANSACTIONS_READ"
    And the consent expires in 30 days
    When I create the consent
    Then the consent is created successfully
    And the consent status is "Pending"
    And the consent has a valid id

Scenario: Consent creation fails with empty client id
    Given a bank client with id ""
    And a user with id "user-123"
    And the requested permissions are "ACCOUNTS_READ"
    And the consent expires in 30 days
    When I create the consent
    Then the consent creation fails
    And the error contains "ClientId"

Scenario: Consent creation fails with no permissions
    Given a bank client with id "bank-001"
    And a user with id "user-123"
    And the requested permissions are ""
    And the consent expires in 30 days
    When I create the consent
    Then the consent creation fails

Scenario: Authorise and then revoke a consent
    Given a pending consent exists for client "bank-002" and user "user-456"
    When the consent is authorised
    Then the consent status is "Authorised"
    When the consent is revoked with reason "Customer requested deletion"
    Then the consent status is "Revoked"

Scenario: Validate active consent with required permissions
    Given an authorised consent with permissions "ACCOUNTS_READ,TRANSACTIONS_READ"
    When I validate the consent for permissions "ACCOUNTS_READ"
    Then the consent validation result is "true"

Scenario: Validate consent fails for missing permission
    Given an authorised consent with permissions "ACCOUNTS_READ"
    When I validate the consent for permissions "PAYMENTS_WRITE"
    Then the consent validation result is "false"
