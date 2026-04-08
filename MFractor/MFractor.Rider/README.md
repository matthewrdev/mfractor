# MFractor Rider

This directory contains the Rider host head for MFractor.

Current scope:

- Rider-only host scaffold based on the JetBrains Rider plugin template.
- A collapsed Rider solution surface built around `MFractor.Core`, `MFractor.Maui`, and the Rider host projects.
- Rider-native UI work targets the IntelliJ frontend plus Rider protocol; the legacy Xwt view layer remains outside the Rider head until each screen is replaced.
- Gradle wrapper, plugin metadata, and sandbox packaging wired for the Rider plugin build.
- Empty RD/protocol scaffolding removed from the active build until a real Rider frontend/backend model is needed.

Build from this directory with:

```bash
./gradlew buildPlugin
```

The produced Rider plugin zip is written to `output/`.
