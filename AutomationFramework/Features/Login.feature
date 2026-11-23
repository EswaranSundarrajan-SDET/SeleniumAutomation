Feature: Login
  Scenario: Sign in with valid demo credentials
    Given I open the login page
    When I sign in with the demo account
    Then I see the settlement form

  Scenario: Reject an incorrect password
    Given I open the login page
    When I sign in with an incorrect password
    Then I see the invalid credentials message
