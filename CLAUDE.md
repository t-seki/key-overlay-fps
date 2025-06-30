# CLAUDE.md - Development Guidelines for KeyOverlayFPS

This document contains project-specific guidelines and best practices for AI-assisted development of the KeyOverlayFPS application.

## Project Overview

KeyOverlayFPS is a Windows WPF application that provides real-time keyboard and mouse input visualization for FPS gaming streams and recordings. The application features:

- Real-time keyboard input detection using Win32 API
- Mouse input visualization with 16-directional movement tracking
- Customizable layouts and themes
- YAML-based configuration management
- Layout editor with live preview

## Development Best Practices

### Commit Standards

- ALWAYS Conventional Commits must be followed.
- ALWAYS write commit messages in Japanese.

### Code Style and Standards

1. **Naming Conventions**
   - Use PascalCase for public members and classes
   - Use camelCase for private fields with underscore prefix (`_fieldName`)
   - Use descriptive names for methods (`UpdateKeyStateByName`, `ApplyLayoutToMainWindow`)

2. **Documentation Standards**
   - Use XML documentation comments for all public APIs
   - Include parameter descriptions and return value documentation
   - Document complex algorithms (e.g., 16-direction calculation)

3. **Error Handling**
   - Use try-catch blocks for file I/O and external API calls
   - Provide meaningful error messages in Japanese for user-facing errors
   - Log errors with System.Diagnostics.Debug for development

## Development Workflow Notes

- Commands such as `dotnet build`, `dotnet test`, and `dotnet run` must be run on Windows for WPF applications, but since Claude code is running on WSL, the dotnet command run on Claude Code will result in an error. As the development flow of this project, dotnet commands are done manually, so please do not execute them in Claude Code.
