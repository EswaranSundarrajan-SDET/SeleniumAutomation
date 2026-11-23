@http
Feature: HTTP status codes
  Scenario Outline: Receive the status code shown in the example
    When I request the status code example for <code>
    Then the HTTP response status is <code>

    Examples:
      | code |
      | 200  |
      | 404  |
