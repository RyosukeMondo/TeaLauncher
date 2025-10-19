#!/bin/bash
# Build TeaLauncher Avalonia for Windows (cross-compile from Linux)
# Creates a single-file, self-contained executable optimized for Windows deployment

set -e  # Exit on any error

# Script location and project paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_FILE="$PROJECT_ROOT/TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj"
OUTPUT_BASE="$PROJECT_ROOT/dist"

# Build configuration
CONFIGURATION="Release"
RUNTIME="win-x64"

# Color output for better readability
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Helper functions
error() {
    echo -e "${RED}Error: $1${NC}" >&2
    exit 1
}

info() {
    echo -e "${GREEN}$1${NC}"
}

warn() {
    echo -e "${YELLOW}$1${NC}"
}

# Parse command line arguments
CLEAN=false

usage() {
    echo "Usage: $0 [options]"
    echo ""
    echo "Build TeaLauncher Avalonia for Windows deployment"
    echo ""
    echo "Options:"
    echo "  --clean              Clean output directory before building"
    echo "  -h, --help           Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                   # Standard release build"
    echo "  $0 --clean           # Clean build from scratch"
}

while [[ $# -gt 0 ]]; do
    case $1 in
        --clean)
            CLEAN=true
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            error "Unknown option: $1\nRun '$0 --help' for usage information"
            ;;
    esac
done

# Validate environment
if ! command -v dotnet &> /dev/null; then
    error ".NET SDK not found. Please install .NET 8 SDK.\nSee BUILD.md for installation instructions."
fi

DOTNET_VERSION=$(dotnet --version)
info "Found .NET SDK: $DOTNET_VERSION"

# Validate project file exists
if [ ! -f "$PROJECT_FILE" ]; then
    error "Project file not found: $PROJECT_FILE"
fi

# Print build configuration
echo ""
info "================================"
info "TeaLauncher Avalonia Build"
info "================================"
echo "Configuration: $CONFIGURATION"
echo "Runtime:       $RUNTIME"
echo "Project:       TeaLauncher.Avalonia"
echo "Output:        $OUTPUT_BASE/TeaLauncher-$RUNTIME/"
echo ""

# Clean output directory if requested
if [ "$CLEAN" = true ]; then
    if [ -d "$OUTPUT_BASE" ]; then
        warn "Cleaning output directory: $OUTPUT_BASE"
        rm -rf "$OUTPUT_BASE"
    fi
fi

# Create output directory
mkdir -p "$OUTPUT_BASE"

# Change to project root
cd "$PROJECT_ROOT"

# Restore dependencies
info "Restoring NuGet packages..."
dotnet restore "$PROJECT_FILE" -r "$RUNTIME" || error "Failed to restore packages"

# Build project
info "Building TeaLauncher.Avalonia..."
echo ""

dotnet publish "$PROJECT_FILE" \
    -c "$CONFIGURATION" \
    -r "$RUNTIME" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:EnableCompressionInSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    || error "Build failed"

# Locate the published executable
PUBLISH_DIR="$PROJECT_ROOT/TeaLauncher.Avalonia/bin/$CONFIGURATION/net8.0-windows/$RUNTIME/publish"

if [ ! -f "$PUBLISH_DIR/TeaLauncher.Avalonia.exe" ]; then
    error "Build succeeded but executable not found at: $PUBLISH_DIR/TeaLauncher.Avalonia.exe"
fi

# Create distribution directory
DIST_DIR="$OUTPUT_BASE/TeaLauncher-$RUNTIME"
mkdir -p "$DIST_DIR"

# Copy executable
info "Packaging distribution..."
cp "$PUBLISH_DIR/TeaLauncher.Avalonia.exe" "$DIST_DIR/TeaLauncher.exe"

# Copy configuration file
if [ -f "$PROJECT_ROOT/TeaLauncher.Avalonia/commands.yaml" ]; then
    cp "$PROJECT_ROOT/TeaLauncher.Avalonia/commands.yaml" "$DIST_DIR/"
else
    warn "commands.yaml not found, skipping"
fi

# Create a README for the distribution
cat > "$DIST_DIR/README.txt" << 'EOF'
TeaLauncher - Quick Command Launcher for Windows
================================================

A fast, keyboard-driven command launcher with global hotkey support.

Quick Start
-----------
1. Double-click TeaLauncher.exe to start
2. Press Ctrl+Space to show the launcher window
3. Type a command name (e.g., "google", "notepad")
4. Press Tab for auto-completion
5. Press Enter to execute the command
6. Press Escape to hide the window

Configuration
-------------
Edit commands.yaml to add or modify commands.

Example command:
  - name: github
    linkto: https://github.com/
    description: Open GitHub homepage

Special Commands
----------------
!reload  - Reload commands.yaml without restarting
!version - Show version information
!exit    - Exit the application

Requirements
------------
- Windows 10 version 1607+ or Windows 11
- No additional software required (self-contained)

Support
-------
For issues and documentation, visit:
https://github.com/YOUR_USERNAME/TeaLauncher

License: See LICENSE file in source repository
EOF

# Calculate file sizes
EXE_SIZE=$(du -h "$DIST_DIR/TeaLauncher.exe" | cut -f1)

# Print success message
echo ""
info "================================"
info "Build Complete!"
info "================================"
echo ""
echo "Output directory: $DIST_DIR"
echo ""
echo "Distribution files:"
echo "  - TeaLauncher.exe    ($EXE_SIZE) - Main executable"
echo "  - commands.yaml      - Configuration file"
echo "  - README.txt         - User guide"
echo ""
info "Deployment Instructions:"
echo "  1. Transfer the entire $DIST_DIR folder to Windows"
echo "  2. Run TeaLauncher.exe (no installation needed)"
echo "  3. Press Ctrl+Space to activate"
echo ""
info "Build Type: Self-contained single-file"
echo "  - No .NET runtime installation required on Windows"
echo "  - All dependencies embedded in TeaLauncher.exe"
echo "  - Optimized for size (debug symbols stripped)"
echo ""

# Optional: Create a zip archive
if command -v zip &> /dev/null; then
    ARCHIVE_NAME="TeaLauncher-$RUNTIME-$(date +%Y%m%d).zip"
    info "Creating archive: $OUTPUT_BASE/$ARCHIVE_NAME"
    cd "$OUTPUT_BASE"
    zip -q -r "$ARCHIVE_NAME" "TeaLauncher-$RUNTIME/"
    ARCHIVE_SIZE=$(du -h "$OUTPUT_BASE/$ARCHIVE_NAME" | cut -f1)
    echo "  Archive: $ARCHIVE_NAME ($ARCHIVE_SIZE)"
    echo ""
fi

info "Build completed successfully!"
