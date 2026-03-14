Feature: Credit Card Data Access
    As a bank participant integrated with the Open Finance Provider
    I want to access customer credit card data
    So that I can provide financial management services to customers

Background:
    Given the credit card service is available

Scenario: List active credit card accounts for a user
    Given a user "user-123" has active credit card accounts
    When I request the credit card accounts for user "user-123" with a valid consent
    Then the response contains a list of credit card accounts
    And all returned accounts have status "Active"

Scenario: Get credit card account details
    Given a credit card account exists for user "user-123"
    When I request the details for that credit card account
    Then the response contains the card last four digits
    And the response contains the card brand
    And the response contains the card holder name
    And the response contains the payment due day

Scenario: Get credit limits for a card account
    Given a credit card account "card-001" exists with a total limit of "15000.00"
    And the account has used "3000.00" of the credit limit
    When I request the credit limits for account "card-001"
    Then the response contains the total limit of "15000.00"
    And the available amount is "12000.00"

Scenario: List card bills ordered by due date
    Given a credit card account has 3 monthly bills
    When I request the bills for that card account
    Then the response contains 3 bills
    And the bills are ordered by due date descending

Scenario: Get transactions for a specific bill
    Given a credit card account "card-001" has a bill with 5 transactions
    When I request the transactions for that bill
    Then the response contains 5 transactions
    And each transaction has a transaction type
    And each transaction has an amount and currency

Scenario: Get transactions for a paid bill
    Given a closed and paid bill exists for credit card "card-001"
    When I request the transactions for that paid bill
    Then the response is successful
    And the bill status in the parent bill is "Paid"

Scenario: Card account not found returns not found response
    Given no credit card account exists with id "00000000-0000-0000-0000-000000000001"
    When I request the details for account "00000000-0000-0000-0000-000000000001"
    Then the response status is "NotFound"
