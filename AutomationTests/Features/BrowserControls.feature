@ui
Feature: Browser controls on The Internet

  Scenario: Select an option from a dropdown
    Given I open the dropdown example
    When I select Option 2
    Then the dropdown shows Option 2 as selected

  Scenario: Check and uncheck a checkbox
    Given I open the checkboxes example
    When I select the first checkbox
    Then the first checkbox is checked
    When I clear the first checkbox
    Then the first checkbox is unchecked

  Scenario: Add and remove a dynamic element
    Given I open the add and remove elements example
    When I add an element
    Then its Delete button is visible
    When I remove the element
    Then its Delete button is hidden

  Scenario: Dismiss a confirm dialog and submit a prompt
    Given I open the JavaScript alerts example
    When I dismiss the confirmation
    Then the dialog result is "You clicked: Cancel"
    When I enter "Selenium sample" in the prompt
    Then the dialog result is "You entered: Selenium sample"

  Scenario: Open a context menu with a right click
    Given I open the context menu example
    When I right click the context menu area
    Then the alert says "You selected a context menu"

  Scenario: Hover over a profile to reveal its name
    Given I open the hovers example
    When I hover over the first profile
    Then the first profile displays "name: user1"

  Scenario: Move the range slider with the keyboard
    Given I open the horizontal slider example
    When I move the slider one step right
    Then the slider shows "0.5"

  Scenario: Drag a column onto another column
    Given I open the drag and drop example
    When I drag column A onto column B
    Then the first column label is "B"

  Scenario: Read text inside nested frames
    Given I open the nested frames example
    Then the left frame contains "LEFT"
    And the bottom frame contains "BOTTOM"

  Scenario: Switch to a new window and return to the original window
    Given I open the multiple windows example
    When I follow the link to a new window
    Then the new window displays "New Window"
    When I close the new window
    Then the original window displays "Opening a new window"

  Scenario: Locate an element in a shadow root
    Given I open the shadow DOM example
    Then the first shadow component contains a slot named "my-text"

  Scenario: Read sortable table headers
    Given I open the sortable tables example
    Then the table has a column named "Last Name"

  Scenario: Upload a sample file
    Given I open the file upload example
    When I upload the sample text file
    Then the upload confirmation shows "upload-example.txt"

  @download
  Scenario: Download and read a text file
    Given I open the modern file download example
    When I download the example text file
    Then the downloaded file contains "Hello from a data URL"
