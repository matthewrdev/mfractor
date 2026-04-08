# Rider Native Views

Rider-native UI for MFractor should be implemented in the IntelliJ frontend and invoked from the .NET backend through Rider protocol when backend features need user interaction.

Current rules:

- Do not move Xwt dialogs into the Rider head and call them migrated.
- Keep legacy Xwt code in [MFractor.Views](/Users/matthewrobbins/Development/git/mfractor/MFractor/Libraries/MFractor.Views) until a native replacement exists.
- Use IntelliJ `Messages`, `DialogWrapper`, `JBPopup`, `FileChooser`, notifications, and `Configurable` pages for Rider UI.
- Introduce RD/protocol models when the backend must open a custom dialog, wizard, or progress surface in the frontend.

Migration targets by legacy view area:

- `About`, `Activation`, `Licensing`, `Onboarding`, `Request Trial`: frontend dialogs built with `DialogWrapper`.
- `Settings`: IntelliJ settings pages via `Configurable` instead of modal dialogs.
- `Context Menu`: IntelliJ action groups and popups, not custom menu widgets.
- `Picker`, `Text Input`, `Name Value Input`: frontend prompts using `Messages` or small `DialogWrapper` forms.
- `Project Selection`: frontend selection dialogs or popups backed by protocol payloads.
- `Image Manager`, `Image Importer`, `App Icon Importer`, `Font Importer`: frontend dialogs or tool windows with protocol-driven operations.
- `MVVM Wizard`, `Value Converter Wizard`, `Scaffolding`, `Type Simplification Wizard`, `XAML Style Editor`: custom frontend dialogs/wizards with protocol contracts.
- `Progress`: background tasks plus frontend notifications/progress UI.

Native foundation in this plugin head starts in [RiderDialogService.kt](/Users/matthewrobbins/Development/git/mfractor/MFractor/MFractor.Rider/src/rider/main/kotlin/com/mfractor/rider/views/RiderDialogService.kt).
