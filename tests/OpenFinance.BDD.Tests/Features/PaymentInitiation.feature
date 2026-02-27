Feature: Payment Initiation
    As a bank participant integrated with the Open Finance Provider
    I want to initiate payments on behalf of consented customers
    So that customers can make payments through the Open Finance ecosystem

Background:
    Given the payment service is available
    And a valid consent id exists

Scenario: Initiate a valid PIX payment
    Given a payment request with amount 100.00 in "BRL"
    And the payment type is "Pix"
    And the debtor account is "ACC-DEBTOR-001"
    And the creditor account is "ACC-CREDITOR-002"
    And the creditor name is "Jane Doe"
    When I initiate the payment
    Then the payment is created successfully
    And the payment status is "Pending"
    And the payment has a valid id

Scenario: Payment initiation fails with zero amount
    Given a payment request with amount 0.00 in "BRL"
    And the payment type is "Pix"
    And the debtor account is "ACC-001"
    And the creditor account is "ACC-002"
    And the creditor name is "Jane Doe"
    When I initiate the payment
    Then the payment initiation fails

Scenario: Cancel a pending payment
    Given a pending payment exists
    When I cancel the payment with reason "Duplicate transaction"
    Then the payment status is "Cancelled"

Scenario: Cannot cancel a completed payment
    Given a completed payment exists
    When I cancel the payment with reason "Too late"
    Then the cancellation fails with an error
