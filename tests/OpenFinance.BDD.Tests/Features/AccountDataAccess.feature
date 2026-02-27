Feature: Account Data Access
    As a bank participant integrated with the Open Finance Provider
    I want to retrieve account information for consented customers
    So that third-party applications can display financial data

Background:
    Given the account service is available
    And a valid consent id for account access exists

Scenario: Get list of accounts for a user
    Given user "user-001" has 2 active accounts
    When I request accounts for user "user-001"
    Then the response contains 2 accounts

Scenario: Get transaction history with valid date range
    Given an account exists with id for user "user-002"
    And the account has 3 transactions in the last 7 days
    When I request transactions from 7 days ago to today
    Then the response contains 3 transactions

Scenario: Get transactions fails with invalid date range
    Given an account exists with id for user "user-003"
    When I request transactions with from date after to date
    Then the transaction request fails with an error
