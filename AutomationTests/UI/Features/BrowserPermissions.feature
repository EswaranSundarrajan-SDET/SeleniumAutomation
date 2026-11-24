@ui @advanced
Feature: Browser permissions

  @block-geolocation
  Scenario: The permissions page shows blocked location access
    Given I open the browser permissions example
    Then the browser and the page report geolocation as denied
