@ui
Feature: Form authentication
  Scenario: Sign in with valid credentials
    Given I open the login page
    When I sign in with the public demo account
    Then I see the secure area and a success message

  Scenario: Reject an incorrect password
    Given I open the login page
    When I sign in with an incorrect password
    Then I see the invalid password message
