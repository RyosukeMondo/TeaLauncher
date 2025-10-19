#!/bin/bash
# Publish TeaLauncher for Windows distribution

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Default settings
RUNTIME="win-x64"
DEPLOYMENT_TYPE="framework-dependent"

usage() {
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  -r, --runtime <runtime>        Target runtime: win-x64 or win-arm64 (default: win-x64)"
    echo "  -t, --type <type>              Deployment type:"
    echo "                                   framework-dependent (default, ~1-5MB, requires .NET runtime)"
    echo "                                   self-contained (large ~60-80MB, no runtime required)"
    echo "  -h, --help                     Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                                              # Framework-dependent win-x64"
    echo "  $0 -t self-contained                            # Self-contained single file"
    echo "  $0 -r win-arm64 -t framework-dependent          # Framework-dependent ARM64"
}

while [[ $# -gt 0 ]]; do
    case $1 in
        -r|--runtime)
            RUNTIME="$2"
            shift 2
            ;;
        -t|--type)
            DEPLOYMENT_TYPE="$2"
            shift 2
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Validate runtime
if [[ "$RUNTIME" != "win-x64" && "$RUNTIME" != "win-arm64" ]]; then
    echo "Error: Runtime must be win-x64 or win-arm64"
    exit 1
fi

# Validate deployment type
if [[ "$DEPLOYMENT_TYPE" != "framework-dependent" && "$DEPLOYMENT_TYPE" != "self-contained" ]]; then
    echo "Error: Deployment type must be framework-dependent or self-contained"
    exit 1
fi

echo "========================================"
echo "Publishing TeaLauncher for Windows"
echo "========================================"
echo "Runtime:         $RUNTIME"
echo "Deployment type: $DEPLOYMENT_TYPE"
echo ""

cd "$PROJECT_ROOT"

# Build appropriate publish command
PROJECT_FILE="CommandLauncher/CommandLauncher.csproj"

if [ "$DEPLOYMENT_TYPE" = "framework-dependent" ]; then
    echo "Building framework-dependent deployment..."
    echo "  Size: ~1-5 MB"
    echo "  Requires: .NET 8 Desktop Runtime on target Windows"
    echo ""

    dotnet publish -c Release -r "$RUNTIME" -f net8.0-windows "$PROJECT_FILE"

else
    echo "Building self-contained deployment..."
    echo "  Size: ~60-80 MB"
    echo "  Requires: No .NET installation on target Windows"
    echo ""

    dotnet publish -c Release \
        -r "$RUNTIME" \
        -f net8.0-windows \
        --self-contained \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        "$PROJECT_FILE"
fi

# Show output
OUTPUT_DIR="$PROJECT_ROOT/CommandLauncher/bin/Release/net8.0-windows/$RUNTIME/publish"

echo ""
echo "========================================"
echo "Publish Complete!"
echo "========================================"
echo "Output directory: $OUTPUT_DIR"
echo ""

if [ "$DEPLOYMENT_TYPE" = "framework-dependent" ]; then
    echo "Files to distribute:"
    echo "  - CommandLauncher.exe"
    echo "  - CommandLauncher.dll"
    echo "  - CommandLauncher.deps.json"
    echo "  - CommandLauncher.runtimeconfig.json"
    echo "  - resource/conf/my.conf (your configuration)"
    echo ""
    echo "User requirements:"
    echo "  - Windows 10 version 1607+ or Windows 11"
    echo "  - .NET 8 Desktop Runtime"
    echo "    Download: https://dotnet.microsoft.com/download/dotnet/8.0"
else
    echo "Files to distribute:"
    echo "  - CommandLauncher.exe (single file, includes runtime)"
    echo "  - resource/conf/my.conf (your configuration)"
    echo ""
    echo "User requirements:"
    echo "  - Windows 10 version 1607+ or Windows 11"
    echo "  - No .NET installation required"
fi

echo ""
echo "Next steps:"
echo "  1. Copy the publish directory to Windows"
echo "  2. Copy resource/conf/my.conf to the publish directory"
echo "  3. Test: CommandLauncher.exe resource\\conf\\my.conf"
echo ""

# Optional: Create a zip package
read -p "Create a zip package for distribution? (y/n) " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
    PACKAGE_NAME="TeaLauncher-$RUNTIME-$DEPLOYMENT_TYPE-$(date +%Y%m%d).zip"

    echo "Creating package: $PACKAGE_NAME"
    cd "$OUTPUT_DIR"

    # Copy config file if it exists
    if [ -f "$PROJECT_ROOT/resource/conf/my.conf" ]; then
        mkdir -p resource/conf
        cp "$PROJECT_ROOT/resource/conf/my.conf" resource/conf/
    fi

    zip -r "$PROJECT_ROOT/$PACKAGE_NAME" .

    echo ""
    echo "Package created: $PROJECT_ROOT/$PACKAGE_NAME"
    echo "Size: $(du -h "$PROJECT_ROOT/$PACKAGE_NAME" | cut -f1)"
fi
