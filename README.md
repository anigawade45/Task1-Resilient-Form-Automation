# Resilient Form Automation using C# and Selenium

## Overview

This project automates form interactions on a web page using **C#, .NET, and Selenium WebDriver**.

The main goal is to build reliable automation that continues working even when the page structure changes. Instead of depending entirely on fixed IDs, names, classes, or XPath expressions, the automation uses semantic and contextual information to identify form elements.

## Problem Statement

Automate the following fields on the CloudQA Automation Practice Form:

- First Name text box
- Gender radio button
- State dropdown

The automation should remain reliable when:
- The order of form fields changes.
- Element IDs, names, or classes change.
- The HTML structure changes slightly.
- Similar-looking or unrelated elements are present.

The target URL must also be configurable.

## Tech Stack

- C#
- .NET
- Selenium WebDriver
- Google Chrome
- ChromeDriver

## Key Features

- **Context-aware element identification:** Locates form fields using their labels and surrounding context.
- **Reduced dependency on fixed selectors:** Avoids relying exclusively on hardcoded IDs, names, classes, or XPath.
- **Configurable URL:** Supports a custom target URL.
- **Multiple field interactions:** Supports text inputs, radio buttons, and dropdowns.
- **Verification:** Checks whether the expected values were entered or selected.
- **Configurable browser driver:** Supports a custom ChromeDriver path.

## Project Structure

```text
Task1-Resilient-Form-Automation/
│
├── Program.cs
├── README.md
├── .gitignore
└── screenshots/
```

## Prerequisites

1. Install the .NET SDK.
2. Install Google Chrome.
3. Install a compatible ChromeDriver.
4. Clone this repository.

## Configuration

The target URL can be configured using the `--url` argument or the `AUTOMATION_FORM_URL` environment variable.

ChromeDriver can be configured using:

```powershell
$env:CHROMEDRIVER_PATH="D:\path\to\chromedriver.exe"
```

## Run the Project

Open a terminal in the project directory and execute:

```bash
dotnet restore
dotnet build
dotnet run
```

To run against a different page:

```bash
dotnet run -- --url "https://app.cloudqa.io/home/AutomationPracticeForm"
```

Additional form values can be supplied through the supported command-line arguments, such as `--first-name`, `--gender`, and `--state`.

## Approach

The implementation focuses on identifying elements based on their meaning and context rather than assuming that their attributes or positions will remain unchanged.

The general workflow is:

1. Open the configured URL.
2. Inspect the page and identify relevant form controls.
3. Associate controls with their labels and surrounding context.
4. Filter candidates by the expected control type.
5. Interact with the selected elements.
6. Verify the entered or selected values.

## Testing

The implementation was tested against local page variations involving:

- Reordered form fields.
- Changed element IDs, names, and classes.
- Similar-looking decoy elements.
- Modified nested HTML structures.

These tests help demonstrate the resilience of the locator strategy.

## Limitations

- Major changes to the meaning or wording of labels may require improvements to the identification logic.
- Completely different form layouts may require additional locator strategies.
- Complex components such as deeply nested Shadow DOM or cross-origin iframes may need dedicated handling.
- Reliability depends on the quality and uniqueness of the contextual information available on the page.

## Conclusion

This project demonstrates a more resilient approach to Selenium-based form automation by reducing dependence on fixed selectors and using semantic and contextual element identification.