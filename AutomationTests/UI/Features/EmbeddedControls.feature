@ui @advanced
Feature: Frames and web components on The Internet

  Scenario: Use sandboxed, inline, and shadow DOM iframes
    Given I open the advanced iframes example
    When I send a message from the sandboxed iframe
    Then the parent page receives the sandboxed iframe message
    And I can enter "Inline frame text" in the inline iframe
    And the iframe inside the shadow root displays "Frame: shadow"

  Scenario: Use nested and closed shadow roots and submit a custom rating
    Given I open the advanced shadow DOM example
    When I click the button inside the nested shadow roots
    Then the page shows that the inner button was clicked
    And I can enter "Closed root text" in the closed shadow root input
    And the title slot displays "Title from the light DOM"
    When I submit a 3-star rating for "Selenium"
    Then the form shows the submitted name and rating "Submitted name: Selenium, rating: 3"

  Scenario: Replace and format text in the rich-text editor
    Given I open the TinyMCE editor example
    When I replace the editor content with "Selenium edited this text" in bold
    Then the editor shows "Selenium edited this text" in bold
