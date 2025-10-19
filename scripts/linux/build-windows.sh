#!/bin/bash
# Build TeaLauncher for Windows (cross-compile from Linux)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Parse command line arguments
CONFIGURATION="Debug"
RUNTIME="win-x64"
CLEAN=false

usage() {
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  -c, --config <config>    Build configuration: Debug or Release (default: Debug)"
    echo "  -r, --runtime <runtime>  Target runtime: win-x64 or win-arm64 (default: win-x64)"
    echo "  --clean                  Clean before building"
    echo "  -h, --help               Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                       # Debug build for win-x64"
    echo "  $0 -c Release            # Release build for win-x64"
    echo "  $0 -c Release --clean    # Clean Release build"
    echo "  $0 -r win-arm64          # Build for Windows ARM64"
}

while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--config)
            CONFIGURATION="$2"
            shift 2
            ;;
        -r|--runtime)
            RUNTIME="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
            shift
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

# Validate configuration
if [[ "$CONFIGURATION" != "Debug" && "$CONFIGURATION" != "Release" ]]; then
    echo "Error: Configuration must be Debug or Release"
    exit 1
fi

# Validate runtime
if [[ "$RUNTIME" != "win-x64" && "$RUNTIME" != "win-arm64" ]]; then
    echo "Error: Runtime must be win-x64 or win-arm64"
    exit 1
fi

echo "================================"
echo "Building TeaLauncher for Windows"
echo "================================"
echo "Configuration: $CONFIGURATION"
echo "Runtime:       $RUNTIME"
echo "Project:       $PROJECT_ROOT"
echo ""

cd "$PROJECT_ROOT"

# Clean if requested
if [ "$CLEAN" = true ]; then
    echo "Cleaning previous builds..."
    dotnet clean -c "$CONFIGURATION"
    echo ""
fi

# Build
echo "Building..."
dotnet build CommandLauncher/CommandLauncher.csproj -c "$CONFIGURATION" -r "$RUNTIME"

# Show output location
OUTPUT_DIR="$PROJECT_ROOT/CommandLauncher/bin/$CONFIGURATION/net8.0-windows/$RUNTIME"
echo ""
echo "================================"
echo "Build Complete!"
echo "================================"
echo "Output: $OUTPUT_DIR"
echo ""
echo "Executable: $OUTPUT_DIR/CommandLauncher.exe"
echo ""
echo "To test on Windows:"
echo "  1. Transfer the entire directory to Windows"
echo "  2. Run: CommandLauncher.exe resource\\conf\\my.conf"
echo ""
echo "To create a release package, use:"
echo "  ./scripts/linux/publish-windows.sh"
