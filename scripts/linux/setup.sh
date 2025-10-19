#!/bin/bash
# TeaLauncher Linux Setup Script
# Installs .NET 8 SDK and prepares environment for Windows cross-compilation

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "================================"
echo "TeaLauncher Setup for Linux"
echo "================================"
echo ""

# Check if running with sudo
if [ "$EUID" -eq 0 ]; then
    echo "WARNING: This script should NOT be run with sudo."
    echo "It will prompt for sudo password only when needed."
    exit 1
fi

# Detect Linux distribution
if [ -f /etc/os-release ]; then
    . /etc/os-release
    DISTRO=$ID
    VERSION=$VERSION_ID
else
    echo "Cannot detect Linux distribution"
    exit 1
fi

echo "Detected: $DISTRO $VERSION"
echo ""

# Function to install .NET 8 SDK using Microsoft's script
install_dotnet_sdk() {
    echo "Installing .NET 8 SDK..."

    # Download Microsoft's installation script
    DOTNET_INSTALL_SCRIPT="/tmp/dotnet-install.sh"
    wget https://dot.net/v1/dotnet-install.sh -O "$DOTNET_INSTALL_SCRIPT"
    chmod +x "$DOTNET_INSTALL_SCRIPT"

    # Install to user's home directory (no sudo required)
    "$DOTNET_INSTALL_SCRIPT" --channel 8.0 --install-dir "$HOME/.dotnet"

    # Clean up
    rm "$DOTNET_INSTALL_SCRIPT"

    echo ""
    echo ".NET 8 SDK installed to: $HOME/.dotnet"
}

# Function to setup PATH
setup_path() {
    echo "Setting up PATH..."

    DOTNET_ROOT="$HOME/.dotnet"
    BASHRC="$HOME/.bashrc"

    # Check if already in PATH
    if echo "$PATH" | grep -q "$DOTNET_ROOT"; then
        echo "dotnet is already in PATH"
    else
        # Add to .bashrc if not already there
        if ! grep -q "export DOTNET_ROOT" "$BASHRC"; then
            echo "" >> "$BASHRC"
            echo "# .NET SDK" >> "$BASHRC"
            echo "export DOTNET_ROOT=\"\$HOME/.dotnet\"" >> "$BASHRC"
            echo "export PATH=\"\$DOTNET_ROOT:\$PATH\"" >> "$BASHRC"
            echo "Added .NET to .bashrc"
        fi

        # Export for current session
        export DOTNET_ROOT="$HOME/.dotnet"
        export PATH="$DOTNET_ROOT:$PATH"
    fi
}

# Function to install system dependencies
install_dependencies() {
    echo "Installing system dependencies..."

    case "$DISTRO" in
        ubuntu|debian)
            echo "Installing dependencies for Ubuntu/Debian..."
            sudo apt-get update
            sudo apt-get install -y wget libc6 libgcc1 libgssapi-krb5-2 libssl3 libstdc++6 zlib1g
            ;;
        fedora)
            echo "Installing dependencies for Fedora..."
            sudo dnf install -y wget compat-openssl11
            ;;
        arch|manjaro)
            echo "Installing dependencies for Arch Linux..."
            sudo pacman -Sy --noconfirm wget openssl
            ;;
        *)
            echo "WARNING: Unknown distribution. You may need to install dependencies manually."
            echo "Required: wget, libc6, libgcc, libssl, libstdc++, zlib"
            ;;
    esac

    echo ""
}

# Check if .NET 8 SDK is already installed
check_dotnet() {
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version)
        if [[ "$DOTNET_VERSION" =~ ^8\. ]]; then
            echo ".NET SDK $DOTNET_VERSION is already installed"
            return 0
        else
            echo "Found .NET SDK $DOTNET_VERSION, but version 8 is required"
            return 1
        fi
    else
        echo ".NET SDK not found"
        return 1
    fi
}

# Main installation flow
main() {
    echo "Step 1: Checking system dependencies..."
    install_dependencies

    echo "Step 2: Checking .NET 8 SDK..."
    if check_dotnet; then
        echo "✓ .NET 8 SDK is ready"
    else
        install_dotnet_sdk
        setup_path

        # Verify installation
        export DOTNET_ROOT="$HOME/.dotnet"
        export PATH="$DOTNET_ROOT:$PATH"

        if "$HOME/.dotnet/dotnet" --version &> /dev/null; then
            INSTALLED_VERSION=$("$HOME/.dotnet/dotnet" --version)
            echo "✓ .NET SDK $INSTALLED_VERSION installed successfully"
        else
            echo "✗ Installation failed. Please install manually from:"
            echo "  https://dotnet.microsoft.com/download/dotnet/8.0"
            exit 1
        fi
    fi

    echo ""
    echo "Step 3: Restoring project dependencies..."
    cd "$PROJECT_ROOT"
    dotnet restore

    echo ""
    echo "================================"
    echo "Setup Complete!"
    echo "================================"
    echo ""
    echo "To start building:"
    echo "  source ~/.bashrc  # Or restart your terminal"
    echo "  cd $PROJECT_ROOT"
    echo "  dotnet build -r win-x64"
    echo ""
    echo "Or use the helper scripts:"
    echo "  ./scripts/linux/build-windows.sh      # Build for Windows"
    echo "  ./scripts/linux/publish-windows.sh    # Create release"
    echo ""
    echo "For full documentation, see BUILD.md"
}

main
