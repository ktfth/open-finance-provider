Feature: Investment Data Access
    As a bank participant integrated with the Open Finance Provider
    I want to access customer investment portfolio data
    So that I can provide wealth management and advisory services

Background:
    Given the investment service is available

# ─── Fixed Income ────────────────────────────────────────────────────────────

Scenario: List fixed income positions for a user
    Given a user "user-123" holds fixed income investments
    When I request the fixed income list for user "user-123" with a valid consent
    Then the response contains a list of fixed income investments
    And each investment has a product name, issuer and maturity date

Scenario: Get fixed income investment details - CDB
    Given a user "user-123" holds a CDB investment with ISIN "BRTST11CDB001"
    When I request the details for that CDB investment
    Then the response contains the investment type "CDB"
    And the response contains the indexer "CDI"
    And the response contains the remuneration type

Scenario: Get fixed income investment details - LCI with tax exemption
    Given a user "user-123" holds an LCI investment with 100% tax exemption
    When I request the details for that LCI investment
    Then the response contains the investment type "LCI"
    And the tax exemption percentage is "100"

Scenario: Get fixed income balance with marked-to-market values
    Given a fixed income CDB investment exists for user "user-123"
    When I request the balance for that investment
    Then the balance contains the gross amount
    And the balance contains the net amount after income tax
    And the balance contains the updated unit price
    And the yield percentage reflects the price appreciation

Scenario: Get fixed income transactions within a date range
    Given a CDB investment has a purchase transaction on "2024-01-15"
    When I request transactions from "2024-01-01" to "2024-12-31"
    Then the response contains 1 transaction
    And the transaction type is "Purchase"

Scenario: Fixed income transactions fail when from date is after to date
    Given a fixed income investment exists
    When I request transactions from "2024-12-31" to "2024-01-01"
    Then the response fails with a date range validation error

# ─── Variable Income ─────────────────────────────────────────────────────────

Scenario: List variable income positions for a user
    Given a user "user-123" holds stocks and ETFs
    When I request the variable income list for user "user-123" with a valid consent
    Then the response contains a list of variable income investments
    And each investment has a ticker and product name

Scenario: Get stock investment details with income tax calculation
    Given a user holds 100 shares of "PETR4" at average cost R$ 30.00
    And the current price of "PETR4" is R$ 35.00
    When I request the details for that stock investment
    Then the gross amount is "3500.00"
    And the income tax is "75.00" (15% of 500 profit)
    And the net amount is "3425.00"

Scenario: Get variable income balance at current market price
    Given a user holds "BOVA11" ETF positions
    When I request the balance for that ETF investment
    Then the balance contains the current quantity
    And the balance contains the current unit price
    And the yield percentage is calculated from average cost

Scenario: Get stock buy and sell transactions
    Given a stock investment has buy and dividend transactions
    When I request transactions for that stock
    Then the transactions include transaction type, quantity, unit price, and net value
    And dividend transactions show zero brokerage fee

# ─── Treasury Bonds (Tesouro Direto) ─────────────────────────────────────────

Scenario: List Tesouro Direto positions for a user
    Given a user "user-123" holds Tesouro Selic and Tesouro IPCA+ positions
    When I request the treasury bonds list for user "user-123" with a valid consent
    Then the response contains a list of treasury bond positions
    And each bond has a product name, bond type, and maturity date

Scenario: Get Tesouro Selic details
    Given a user holds a "Tesouro Selic 2027" position
    When I request the details for that treasury bond
    Then the response contains bond type "Selic"
    And the response contains the nominal unit price and updated unit price
    And the response contains the rate type

Scenario: Treasury bond income tax uses regressive table after 720 days
    Given a user purchased Tesouro Prefixado more than 720 days ago
    When I request the balance for that treasury bond
    Then the income tax rate applied is "15%"
    And the net amount equals gross amount minus income tax

Scenario: Treasury bond income tax is 22.5% within 180 days of purchase
    Given a user purchased Tesouro IPCA+ less than 180 days ago
    When I request the balance for that treasury bond
    Then the income tax rate applied is "22.5%"
