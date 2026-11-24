@ui @advanced
Feature: Forms, consent, scrolling, and sign-in on The Internet

  Scenario: Reject an empty username and a username already in use
    Given I open the form validation example
    When I try to submit the empty validation form
    Then the browser requires a username
    When I submit the validation form with the public demo username
    Then the page reports that the username is already taken

  Scenario: Keep the cookie consent choice after a reload
    Given I open the cookie consent example with no saved choice
    When I choose only necessary cookies and reload the page
    Then the cookie banner stays hidden and analytics cookies stay blocked

  Scenario: Load more content by scrolling
    Given I open the infinite scroll example and wait for its first item
    When I scroll to load more items
    Then the existing page contains more items

  Scenario: Sign in with the displayed one-time code
    Given I open the two-factor login example
    When I enter the public demo credentials for two-factor login
    And I submit the one-time code shown on the page
    Then I see the two-factor secure area
