@API @authentication
Feature: API authentication and access permissions

  Scenario Outline: Basic credentials require permission to read reports
    When I request protected reports with <credentials> Basic credentials
    Then the authentication API returns HTTP <status>

    Examples:
      | credentials | status |
      | reader      | 200    |
      | limited     | 403    |

  Scenario Outline: Basic authentication rejects missing or incorrect credentials
    When I request protected reports with <credentials> Basic credentials
    Then the authentication API returns HTTP 401
    And the response offers a Basic authentication challenge

    Examples:
      | credentials    |
      | missing        |
      | wrong-password |
      | unknown-user   |

  Scenario Outline: An OAuth 2.0 access token requires permission to read reports
    Given I obtain an OAuth 2.0 access token for scope "<scope>"
    When I request protected reports with OAuth 2.0 token state issued
    Then the authentication API returns HTTP <status>

    Examples:
      | scope         | status |
      | reports.read  | 200    |
      | reports.write | 403    |

  Scenario Outline: OAuth 2.0 rejects missing or unissued access tokens
    When I request protected reports with OAuth 2.0 token state <token>
    Then the authentication API returns HTTP 401
    And the response offers a Bearer authentication challenge

    Examples:
      | token   |
      | missing |
      | invalid |

  Scenario: OAuth 2.0 checks client credentials before issuing an access token
    When I request an OAuth 2.0 access token with incorrect client credentials
    Then the OAuth 2.0 token endpoint returns HTTP 401

  Scenario Outline: A signed JWT requires permission to read reports
    When I request protected reports with JWT state <token>
    Then the authentication API returns HTTP <status>

    Examples:
      | token  | status |
      | reader | 200    |
      | writer | 403    |

  Scenario Outline: JWT authentication rejects invalid tokens
    When I request protected reports with JWT state <token>
    Then the authentication API returns HTTP 401
    And the response offers a Bearer authentication challenge

    Examples:
      | token           |
      | missing         |
      | wrong-signature |
      | expired         |
