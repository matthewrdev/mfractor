# MFractor Rider

This directory contains the Rider host head for MFractor.

Current scope:

- Rider-only host scaffold based on the JetBrains Rider plugin template.
- .NET backend projects renamed to `MFractor.Rider`.
- Gradle wrapper, plugin metadata, and sandbox packaging wired for the Rider plugin build.
- Backend project references the MAUI-only MFractor core.

Build from this directory with:

```bash
./gradlew buildPlugin
```

The produced Rider plugin zip is written to `output/`.
