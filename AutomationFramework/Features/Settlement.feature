Feature: Settlement
  Scenario: Submit a settlement and verify its recorded amount
    Given I open the login page
    When I sign in with the demo account
    And I submit the settlement from the test data
    Then the confirmation shows my settlement reference
    And the API returns the submitted settlement amount
