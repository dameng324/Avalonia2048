# Avalonia 2048

A cross-platform implementation of the classic [2048](https://play2048.co/) puzzle game built with [Avalonia UI](https://avaloniaui.net/).

## Features

- 🎮 Full 2048 gameplay — slide tiles, merge doubles, reach 2048 and beyond
- 🏆 Persistent best-score tracking across sessions
- 🔄 **Keep Going** after reaching 2048 — win threshold doubles each time (4096, 8192, …)
- ✨ Tile spawn and merge animations
- ⌨️ Keyboard (arrow keys / WASD) and touch/swipe support
- 🌐 Cross-platform: Desktop, Browser (WASM), Android, iOS

## Platform Support

| Platform | Status |
|----------|--------|
| Windows / macOS / Linux (Desktop) | ✅ |
| Browser (WebAssembly) | ✅ |
| Android | ✅ |
| iOS | ✅ |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later

### Run on Desktop

```bash
dotnet run --project src/avalonia2048.Desktop
```

### Run in Browser (WASM)

```bash
dotnet run --project src/avalonia2048.Browser
```

### Run on Android

Open the solution in Visual Studio / Rider with the Android workload installed, select the Android project and deploy to a device or emulator.

### Run on iOS

Open the solution in Visual Studio for Mac or Rider on macOS with the iOS workload installed.

### Run Tests

```bash
# Uses dotnet run (required by TUnit test runner)
dotnet run --project tests/avalonia2048.Tests
```

## Project Structure

```
src/
  avalonia2048/          # Shared core library (game logic, ViewModels, Views)
  avalonia2048.Desktop/  # Desktop entry point
  avalonia2048.Browser/  # WASM entry point
  avalonia2048.Android/  # Android entry point
  avalonia2048.iOS/      # iOS entry point
tests/
  avalonia2048.Tests/    # TUnit unit tests for game logic
```

## Contributing

Contributions are welcome! Please open an issue or pull request.

### Code Formatting

This project uses [CSharpier](https://csharpier.com/) for code formatting. After making changes, run:

```bash
dotnet tool restore
dotnet csharpier format .
```

## License

This project is licensed under the [MIT License](LICENSE).
