@API
Feature: HTTP methods with HTTPBingo

  Scenario: HTTPBingo returns the GET method and query value
    When I send GET to HTTPBingo with query "selenium automation"
    Then the HTTPBingo response contains method "GET"
    And the HTTPBingo response contains query "selenium automation"

  Scenario Outline: HTTPBingo returns the method and JSON for write requests
    When I send <method> to HTTPBingo with a JSON name of "Selenium sample"
    Then the HTTPBingo response contains method "<method>"
    And the HTTPBingo response contains JSON name "Selenium sample"

    Examples:
      | method |
      | POST   |
      | PUT    |
      | PATCH  |
