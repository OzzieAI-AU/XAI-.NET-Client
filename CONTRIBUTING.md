# Contributing to XAI .NET Client

![Contribution Welcome](Images/img4.png)

We are excited that you are interested in contributing to the **XAI .NET Client**! This project aims to make Explainable AI accessible to the .NET community, and your contributions—whether they are bug fixes, new features, or documentation improvements—are highly valued.

## Development Setup

1.  **Fork** the repository on GitHub.
2.  **Clone** your fork locally:
    ```bash
    git clone [https://github.com/OzzieAI-AU/XAI-.NET-Client.git](https://github.com/OzzieAI-AU/XAI-.NET-Client.git)
    ```
3.  Ensure you have the **.NET 8.0 SDK** (or later) installed.
4.  Restore dependencies:
    ```bash
    dotnet restore
    ```

![Workflow Diagram](Images/img5.png)

## Contribution Guidelines

* **Coding Standard:** Please adhere to the standard C# coding conventions used throughout the project.
* **Testing:** All new features or bug fixes should include corresponding unit tests in the `XAI.Client.Tests` project.
* **Commit Messages:** Use descriptive, imperative commit messages (e.g., "Add support for streaming responses").

```csharp
// Example: Adding a new XAI feature
public class NewReasoningModule : IReasoningProvider 
{
    public string GenerateExplanation() => "Detailed logic here.";
}
```

## Pull Request Process

1.  Create a new branch for your feature or fix.
2.  Submit a Pull Request (PR) once your changes pass all local tests.
3.  The maintainers will review your PR and provide feedback.

![Review Process](Images/img6.png)

## Need Help or Have Questions?

If you run into issues or want to discuss a major change before starting, please visit our support channels:

* **Developer Support:** [OzzieAI Website](https://www.ozzieai.com)
* **Discussion Board:** [OzzieAI Forum](https://forum.ozzieai.com)

![Community](img7.png)

Thank you for helping us build a more transparent AI future!