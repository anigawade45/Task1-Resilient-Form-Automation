using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

const string defaultUrl = "https://app.cloudqa.io/home/AutomationPracticeForm";
var url = GetOption(args, "--url") ??
          Environment.GetEnvironmentVariable("AUTOMATION_FORM_URL") ??
          defaultUrl;
var firstName = GetOption(args, "--first-name") ?? "Automation";
var gender = GetOption(args, "--gender") ?? "Male";
var state = GetOption(args, "--state") ?? "India";
var lastName = GetOption(args, "--last-name") ?? "Tester";
var dateOfBirth = GetOption(args, "--date-of-birth") ?? "1990-01-01";
var email = GetOption(args, "--email") ?? "tester@example.com";
var driverPath = GetOption(args, "--driver-path") ??
                 Environment.GetEnvironmentVariable("CHROMEDRIVER_PATH") ??
                 FindOnPath("chromedriver.exe") ??
                 FindWorkspaceDriver();

if (!Uri.TryCreate(url, UriKind.Absolute, out var formUri) ||
    (formUri.Scheme != Uri.UriSchemeHttp && formUri.Scheme != Uri.UriSchemeHttps))
{
    throw new ArgumentException($"The form URL must be an absolute HTTP or HTTPS URL: {url}");
}

if (string.IsNullOrWhiteSpace(driverPath))
{
    throw new FileNotFoundException(
        "ChromeDriver was not found. Install the ChromeDriver version matching your Chrome browser, " +
        "then pass --driver-path \"C:\\path\\to\\chromedriver.exe\" or set CHROMEDRIVER_PATH. " +
        "The executable must be permitted by your organization's security software.");
}

var resolvedDriverPath = Path.GetFullPath(driverPath);
if (Directory.Exists(resolvedDriverPath))
{
    resolvedDriverPath = Path.Combine(resolvedDriverPath, "chromedriver.exe");
}

if (!File.Exists(resolvedDriverPath))
{
    throw new FileNotFoundException($"ChromeDriver executable was not found: {resolvedDriverPath}");
}

var driverService = ChromeDriverService.CreateDefaultService(
    Path.GetDirectoryName(resolvedDriverPath)!,
    Path.GetFileName(resolvedDriverPath));
using IWebDriver driver = new ChromeDriver(driverService, new ChromeOptions());
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
driver.Navigate().GoToUrl(formUri);

const string textInputSelector =
    "input:not([type]), input[type='text'], input[type='search'], input[type='date'], input[type='email'], textarea";
FillAndVerifyTextField(driver, "First Name", firstName, textInputSelector);
FillAndVerifyTextField(driver, "Last Name", lastName, textInputSelector);
FillAndVerifyTextField(driver, "Date of Birth", dateOfBirth, textInputSelector);
FillAndVerifyTextField(driver, "Email", email, textInputSelector);

var genderRadios = FindControlsNearLabel(driver, "Gender", "input[type='radio']");
var genderRadio = FindRadioByOption(driver, genderRadios, gender)
    ?? throw new NoSuchElementException($"Could not find the '{gender}' option in the Gender radio group.");
genderRadio.Click();
if (!genderRadio.Selected)
{
    throw new InvalidOperationException($"The '{gender}' Gender option was not selected.");
}

var stateField = RequireSingleControl(FindControlsNearLabel(driver, "State", "select"), "State");
stateField.SendKeys(state);

var selectedState = stateField.FindElements(By.TagName("option"))
    .FirstOrDefault(option => option.Selected)?.Text.Trim();
if (!string.Equals(selectedState, state, StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        $"Could not select '{state}' in the State dropdown. Selected option: '{selectedState ?? "(none)"}'.");
}

Console.WriteLine($"Form fields verified at {formUri}");
Console.WriteLine($"First Name: {firstName}");
Console.WriteLine($"Last Name: {lastName}");
Console.WriteLine($"Date of Birth: {dateOfBirth}");
Console.WriteLine($"Email: {email}");
Console.WriteLine($"Gender: {gender}");
Console.WriteLine($"State: {selectedState}");

static string? GetOption(string[] arguments, string optionName)
{
    for (var index = 0; index < arguments.Length; index++)
    {
        if (!string.Equals(arguments[index], optionName, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (index + 1 >= arguments.Length || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Expected a value after {optionName}.");
        }

        return arguments[index + 1];
    }

    return null;
}

static string? FindOnPath(string executableName)
{
    var path = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrWhiteSpace(path))
    {
        return null;
    }

    foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        var candidate = Path.Combine(directory.Trim('"'), executableName);
        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    return null;
}

static string? FindWorkspaceDriver()
{
    var candidate = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..",
        "drivers",
        "chromedriver.exe"));
    return File.Exists(candidate) ? candidate : null;
}

static void FillAndVerifyTextField(
    IWebDriver driver,
    string label,
    string value,
    string controlSelector)
{
    var field = RequireSingleControl(
        FindControlsNearLabel(driver, label, controlSelector),
        label);
    field.Clear();
    field.SendKeys(value);
    if (!string.Equals(field.GetAttribute("value"), value, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"The {label} field did not retain the entered value.");
    }
}

static IWebElement RequireSingleControl(IReadOnlyCollection<IWebElement> elements, string label)
{
    return elements.Count switch
    {
        0 => throw new NoSuchElementException($"Could not find the {label} field."),
        1 => elements.Single(),
        _ => throw new InvalidOperationException(
            $"Found {elements.Count} possible controls for {label}; refusing to select an ambiguous field.")
    };
}

static IReadOnlyCollection<IWebElement> FindControlsNearLabel(
    IWebDriver driver,
    string label,
    string controlSelector)
{
    const string script = """
        const label = String(arguments[0]).trim().toLocaleLowerCase();
        const selector = arguments[1];
        const normalize = value => String(value || '').replace(/\s+/g, ' ').trim().replace(/[:*]\s*$/, '').toLocaleLowerCase();
        const isVisible = element => !!(element.offsetWidth || element.offsetHeight || element.getClientRects().length);
        const accessibleName = element => {
          const names = [
            element.getAttribute('aria-label'),
            element.getAttribute('aria-labelledby')
              ? element.getAttribute('aria-labelledby').split(/\s+/)
                  .map(id => document.getElementById(id)?.innerText || '').join(' ')
              : '',
            element.labels ? Array.from(element.labels).map(item => item.innerText).join(' ') : ''
          ];
          return names.map(normalize);
        };
        const allControls = Array.from(document.querySelectorAll(selector)).filter(isVisible);
        const namedControls = allControls.filter(element => accessibleName(element).includes(label));
        if (namedControls.length) return namedControls;

        const matches = new Set();
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
        let textNode;
        while ((textNode = walker.nextNode())) {
          if (normalize(textNode.nodeValue) !== label) continue;

          const labelElement = textNode.parentElement?.closest('label');
          if (labelElement) {
            const labelControls = Array.from(labelElement.querySelectorAll(selector)).filter(isVisible);
            if (labelControls.length === 1) {
              matches.add(labelControls[0]);
              continue;
            }
          }

          let container = textNode.parentElement;
          for (let depth = 0; container && depth < 6; depth++, container = container.parentElement) {
            const followingControls = Array.from(container.querySelectorAll(selector))
              .filter(isVisible)
              .filter(element => textNode.compareDocumentPosition(element) & Node.DOCUMENT_POSITION_FOLLOWING);
            if (followingControls.length) {
              followingControls.forEach(control => matches.add(control));
              break;
            }
          }
        }
        return Array.from(matches);
        """;

    var result = ((IJavaScriptExecutor)driver).ExecuteScript(script, label, controlSelector);
    if (result is not IEnumerable<object> elements)
    {
        throw new InvalidOperationException(
            $"Could not read the discovered controls for the {label} field.");
    }

    return elements.OfType<IWebElement>().ToArray();
}

static IWebElement? FindRadioByOption(
    IWebDriver driver,
    IReadOnlyCollection<IWebElement> radios,
    string option)
{
    const string script = """
        const radio = arguments[0];
        const normalize = value => String(value || '').replace(/\s+/g, ' ').trim().replace(/[:*]\s*$/, '').toLocaleLowerCase();
        const target = normalize(arguments[1]);
        const names = [
          radio.getAttribute('aria-label'),
          radio.getAttribute('title'),
          radio.labels ? Array.from(radio.labels).map(item => item.innerText).join(' ') : '',
          radio.value
        ].map(normalize);
        if (names.includes(target)) return true;

        for (let sibling = radio.nextSibling; sibling; sibling = sibling.nextSibling) {
          if (sibling.nodeType === Node.ELEMENT_NODE && sibling.querySelector('input[type="radio"]')) break;
          const siblingText = normalize(sibling.textContent);
          if (siblingText) return siblingText === target;
        }

        for (let parent = radio.parentElement, depth = 0; parent && depth < 3; parent = parent.parentElement, depth++) {
          if (normalize(parent.innerText) === target) return true;
        }
        return false;
        """;

    var javascript = (IJavaScriptExecutor)driver;
    var matches = radios
        .Where(radio => (bool)javascript.ExecuteScript(script, radio, option)!)
        .ToArray();
    return matches.Length switch
    {
        0 => null,
        1 => matches[0],
        _ => throw new InvalidOperationException(
            $"Found multiple '{option}' radio buttons for Gender; refusing to select an ambiguous option.")
    };
}
