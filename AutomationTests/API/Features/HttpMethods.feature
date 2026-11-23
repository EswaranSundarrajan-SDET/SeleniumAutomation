@http
Feature: HTTP methods

  Scenario: GET echoes a query parameter
    When I send GET to the echo service with query "selenium automation"
    Then the response echoes method "GET"
    And the response echoes query "selenium automation"

  Scenario Outline: Write methods echo a JSON payload
    When I send <method> to the echo service with a JSON name of "Selenium sample"
    Then the response echoes method "<method>"
    And the response echoes JSON name "Selenium sample"

    Examples:
      | method |
      | POST   |
      | PUT    |
      | PATCH  |
