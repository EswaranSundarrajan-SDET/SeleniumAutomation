@ui
Feature: Dynamic loading
  Scenario: Wait for text rendered after clicking Start
    Given I open the dynamic loading example
    When I start loading the message
    Then I see the loaded message "Hello World!"
