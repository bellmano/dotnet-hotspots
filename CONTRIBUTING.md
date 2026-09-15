## How to Contribute

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes and add or update tests
4. Format the code with [CSharpier](https://csharpier.com): `dotnet csharpier format .`
5. Run the tests: `dotnet test`
6. Commit your changes: `git commit -m 'Add some feature'`
7. Push to the branch: `git push origin feature/your-feature`
8. Open a Pull Request

## Development Setup

1. Clone the repository: `git clone https://github.com/bellmano/dotnet-hotspots.git`
2. Restore dependencies: `dotnet restore`
3. Restore the local tools: `dotnet tool restore`
4. Build the project: `dotnet build`

## Code Style

- All C# code must be formatted with [CSharpier](https://csharpier.com) before it is committed or pushed. Run `dotnet csharpier format .` from the repository root.
- Write meaningful commit messages

## Testing

- Add or update unit tests for new features or bug fixes
- Ensure all tests pass before submitting a PR (`dotnet test`)

## Issues

If you find a bug or have a feature request, please [open an issue](https://github.com/bellmano/dotnet-hotspots/issues).
