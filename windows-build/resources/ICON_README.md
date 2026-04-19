# Application Icon Setup

## Overview

Meta Skill Studio requires a Windows icon file (`app.ico`) with multiple resolutions for optimal display across Windows interfaces.

## Required Icon Sizes

The ICO file should contain these image sizes:
- 16x16 pixels (taskbar small, window icon)
- 32x32 pixels (taskbar standard, desktop icon)
- 48x48 pixels (Start Menu, File Explorer)
- 64x64 pixels (high DPI)
- 128x128 pixels (large icons, About dialog)
- 256x256 pixels (high DPI desktop, Store listing)

## Creating the Icon

### Option 1: Using GIMP (Free)

1. Create a 256x256 PNG of your logo
2. Open in GIMP
3. Export as ICO with these sizes: 16, 32, 48, 64, 128, 256

### Option 2: Using ImageMagick (Command Line)

```bash
# Create ICO from PNG with multiple sizes
convert logo.png -define icon:auto-resize=256,128,64,48,32,16 app.ico
```

### Option 3: Using Online Tools

- [ConvertICO](https://convertico.com/)
- [ICoConverter](https://icoconvert.com/)
- [CloudConvert](https://cloudconvert.com/png-to-ico)

### Option 4: Using Visual Studio

1. Create a Resource File (.rc)
2. Add icon resource
3. Build with RC compiler

## Design Guidelines

- Use transparent background (32-bit with alpha)
- Keep it simple - detailed logos don't scale down well
- Test at 16x16 - it should still be recognizable
- Use the primary brand color
- Consider a version with white/light background

## Placeholder Icon

Until a custom icon is created, the build system will use a default Windows application icon. To use your own:

1. Create your icon following the guidelines above
2. Save as `resources/app.ico`
3. Rebuild the application

## Testing the Icon

After building:
1. The EXE should show your icon in File Explorer
2. The taskbar should show the icon when running
3. The About dialog (if present) should show the large icon
4. The Add/Remove Programs should show the icon
