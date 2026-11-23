@ui
Feature: Advanced controls on The Internet

  Scenario: Choose a radio button
    Given I open the radio buttons example
    When I choose the red color
    Then only red is selected in the color group

  Scenario: Select and deselect multiple options
    Given I open the native multiple select example
    When I select options 1 and 3 and deselect option 1
    Then only Option 3 remains selected

  Scenario: Select an option from a custom dropdown
    Given I open the custom dropdown example
    When I choose Banana from the custom dropdown
    Then the dropdown reports that banana was selected

  Scenario: Choose an autocomplete suggestion after it arrives
    Given I open the autocomplete example
    When I type "ban" and choose "Banana"
    Then the autocomplete field contains "Banana"

  Scenario: Pick a day in the calendar
    Given I open the calendar for April 2026
    When I pick "2026-04-15"
    Then the appointment field contains "2026-04-15"

  Scenario: Type into a dynamically enabled input
    Given I open the dynamic controls example
    Then the dynamic input is disabled
    When I enable the dynamic input and type "Ready"
    Then the dynamic input contains "Ready"

  Scenario: Re-find an element after it is replaced
    Given I open the stale element example
    When I rebuild the list
    Then the replacement list starts with "Item 1"

  Scenario: Swipe and long press using pointer actions
    Given I open the pointer events example
    When I swipe the area to the right
    Then the swipe result is "Swiped right (mouse)"
    When I press and hold the long press button
    Then the long press result is "Long press"
