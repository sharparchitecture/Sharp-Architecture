# CSharp Unit-test generation

- Do not generate unit-tests to check for null constructor and method parameters, unless explicitly told to do this.
- Always analyze `Directory.Build.props` to check for package references, implicit using and target frameworks.
- Analyze existing unit-tests in the project where asked to generate test files to get learn about coding style and tests structure,
- When generating tests for internal classes, add InternalsVisibleTo attribute to source project to expose internal types to unit-test project.
- Use MOQ for mock generation
  - prefer `MockFactory` if more than one mock is needed.
  - prefer `Strict` mode when creating mock files.
- Use Shouldly library for assertions.
- Do not add Arrange, Act, Assert comments to tests.
