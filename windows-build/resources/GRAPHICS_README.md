# WiX Installer Graphics

## Overview

The WiX installer can display custom graphics during installation.

## Required Files

### banner.bmp
- **Size**: 493 x 58 pixels
- **Format**: BMP, 24-bit color
- **Usage**: Top banner on most dialogs
- **Design**: Company logo on left, neutral background

### dialog.bmp
- **Size**: 493 x 312 pixels  
- **Format**: BMP, 24-bit color
- **Usage**: Left side of main dialogs
- **Design**: Product artwork, can include features list

## Creating the Graphics

### Using GIMP

1. Create new image with correct dimensions
2. Design with RGB color mode
3. Export as BMP (no alpha channel)
4. Ensure file size is reasonable (< 500KB each)

### Design Guidelines

- **Banner**: Keep text to left side (first 200px), right side gets covered by dialog text
- **Dialog**: Important content in top 2/3, bottom may be obscured by buttons
- **Colors**: Match your brand, but ensure good contrast with white text overlays
- **Safe zones**: Leave 10px margin on all edges

## Placeholder Graphics

Until custom graphics are created, the installer will use default Windows Installer graphics (blue background, generic icons).

To use custom graphics:
1. Create files following the specifications above
2. Save as `resources/banner.bmp` and `resources/dialog.bmp`
3. Rebuild the installer

## Testing

After building the installer:
1. Run the MSI
2. Check all dialogs display correctly
3. Verify text is readable over graphics
4. Test on different DPI settings
