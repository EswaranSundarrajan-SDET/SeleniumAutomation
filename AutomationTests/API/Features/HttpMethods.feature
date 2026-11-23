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
      | DELETE |

  Scenario: HEAD returns metadata and an empty body
    When I send HEAD to the echo service
    Then the response contains JSON metadata and no body

  Scenario: OPTIONS returns supported methods for a preflight request
    When I send OPTIONS to the echo service for method PATCH
    Then the response allows PATCH in its CORS headers

  @transport
  Scenario Outline: Exercise additional request verbs with a controlled transport
    When I send <method> through the in-process transport
    Then the transport receives method "<method>"

    Examples:
      | method  |
      | TRACE   |
      | CONNECT |
      | COPY    |
      | MERGE   |
      | SEARCH  |
