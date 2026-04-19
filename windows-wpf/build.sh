#!/bin/bash
# Build script for WPF edition - CI/CD compatible
# Works on Windows (with .NET SDK), Linux/macOS (for CI validation)

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Version override (optional)
VERSION=${1:-"1.0.0"}

echo -e "${GREEN}==========================================${NC}"
echo -e "${GREEN}Meta Skill Studio WPF - Build Script${NC}"
echo -e "${GREEN}Version: $VERSION${NC}"
echo -e "${GREEN}==========================================${NC}"
echo ""

# Check if running on Windows
IS_WINDOWS=false
if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]] || [[ "$OSTYPE" == "win32" ]] || [[ -n "$WINDIR" ]]; then
    IS_WINDOWS=true
fi

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found!${NC}"
    echo "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo ".NET SDK version: $DOTNET_VERSION"

if ! dotnet --list-sdks | grep -q "8.0"; then
    echo -e "${RED}WARNING: .NET 8.0 SDK not detected. Build may fail.${NC}"
fi

echo ""
echo -e "${YELLOW}Step 1: Restore NuGet packages${NC}"
dotnet restore MetaSkillStudio.sln
if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Restore failed!${NC}"
    exit 1
fi
echo -e "${GREEN}Restore completed successfully${NC}"

echo ""
echo -e "${YELLOW}Step 2: Build Debug configuration${NC}"
dotnet build MetaSkillStudio.sln \
    --configuration Debug \
    --no-restore \
    -p:Version=$VERSION \
    -p:AssemblyVersion=$VERSION.0 \
    -p:FileVersion=$VERSION.0
if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Debug build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}Debug build completed successfully${NC}"

echo ""
echo -e "${YELLOW}Step 3: Run tests${NC}"
dotnet test MetaSkillStudio.sln \
    --no-build \
    --configuration Debug \
    --verbosity normal
TEST_RESULT=$?
if [ $TEST_RESULT -ne 0 ]; then
    echo -e "${RED}ERROR: Tests failed!${NC}"
    exit 1
fi
echo -e "${GREEN}All tests passed${NC}"

echo ""
echo -e "${YELLOW}Step 4: Build Release configuration${NC}"
dotnet build MetaSkillStudio.sln \
    --configuration Release \
    --no-restore \
    -p:Version=$VERSION \
    -p:AssemblyVersion=$VERSION.0 \
    -p:FileVersion=$VERSION.0
if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Release build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}Release build completed successfully${NC}"

echo ""
echo -e "${YELLOW}Step 5: Publish single-file executable${NC}"
PUBLISH_DIR="./publish"
rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

dotnet publish MetaSkillStudio/MetaSkillStudio.csproj \
    --configuration Release \
    --no-build \
    --output "$PUBLISH_DIR" \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true
if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Publish failed!${NC}"
    exit 1
fi
echo -e "${GREEN}Publish completed successfully${NC}"

# Build installer only on Windows
if [ "$IS_WINDOWS" = true ]; then
    echo ""
    echo -e "${YELLOW}Step 6: Build MSI installer${NC}"
    
    # Check for WiX
    WIX_PATH=""
    for wix_dir in "/c/Program Files (x86)/WiX Toolset v3.11/bin" "/c/Program Files/WiX Toolset v3.11/bin" "${WIX}"; do
        if [ -d "$wix_dir" ] && [ -f "$wix_dir/candle.exe" ]; then
            WIX_PATH="$wix_dir"
            break
        fi
    done
    
    if [ -z "$WIX_PATH" ]; then
        echo -e "${YELLOW}WARNING: WiX Toolset not found. Skipping installer build.${NC}"
        echo "Install WiX with: winget install WiXToolset.WiXToolset"
    else
        echo "Found WiX at: $WIX_PATH"
        cd installer
        
        "$WIX_PATH/candle.exe" -nologo \
            -dProductVersion="$VERSION" \
            -dPublishDir="..\\$PUBLISH_DIR" \
            -out "MetaSkillStudio.wixobj" \
            "MetaSkillStudio.wxs"
        
        if [ $? -eq 0 ]; then
            "$WIX_PATH/light.exe" -nologo \
                -out "MetaSkillStudio-$VERSION.msi" \
                -ext WixUIExtension \
                "MetaSkillStudio.wixobj"
            
            if [ $? -eq 0 ]; then
                echo -e "${GREEN}MSI installer created: MetaSkillStudio-$VERSION.msi${NC}"
            else
                echo -e "${RED}ERROR: MSI linking failed!${NC}"
            fi
        else
            echo -e "${RED}ERROR: MSI compilation failed!${NC}"
        fi
        cd ..
    fi
else
    echo ""
    echo -e "${YELLOW}Step 6: Skipping MSI installer (Windows only)${NC}"
    echo "Run on Windows to build the installer."
fi

echo ""
echo -e "${GREEN}==========================================${NC}"
echo -e "${GREEN}Build completed successfully!${NC}"
echo -e "${GREEN}==========================================${NC}"
echo ""
echo "Output locations:"
echo "  Debug:   MetaSkillStudio/bin/Debug/net8.0-windows/"
echo "  Release: MetaSkillStudio/bin/Release/net8.0-windows/"
echo "  Publish: $PUBLISH_DIR/"
if [ "$IS_WINDOWS" = true ] && [ -f "installer/MetaSkillStudio-$VERSION.msi" ]; then
    echo "  MSI:     installer/MetaSkillStudio-$VERSION.msi"
fi
echo ""
