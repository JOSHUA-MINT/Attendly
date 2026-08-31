#!/bin/bash
# Attendly Quick Setup Script
# This script helps you set up and run the Attendly application

echo "═══════════════════════════════════════════"
echo " Attendly - Quick Setup"
echo "═══════════════════════════════════════════"
echo ""

# Check for .NET SDK
echo "Checking for .NET SDK..."
if ! command -v dotnet &> /dev/null; then
 echo "ERROR: .NET SDK not found. Please install from https://dotnet.microsoft.com/download"
 exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo ".NET SDK version: $DOTNET_VERSION"
echo ""

# Check if appsettings.json is configured
echo "Checking configuration..."
if grep -q "YOUR_RAZORPAY_KEY_ID" appsettings.json; then
 echo "WARNING: Razorpay keys are not configured."
 echo " Edit appsettings.json or appsettings.Development.json to add your Razorpay keys."
 echo " Payments will not work without Razorpay keys."
 echo ""
fi

# Restore packages
echo "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
 echo "ERROR: Failed to restore packages"
 exit 1
fi
echo "Packages restored successfully!"
echo ""

# Build the project
echo "Building project..."
dotnet build --no-restore
if [ $? -ne 0 ]; then
 echo "ERROR: Build failed"
 exit 1
fi
echo "Build successful!"
echo ""

echo "═══════════════════════════════════════════"
echo " Setup complete!"
echo "═══════════════════════════════════════════"
echo ""
echo "Next steps:"
echo "1. Ensure your Supabase database has the schema from Data/migrations/001_initial_schema.sql"
echo "2. Run the application with: dotnet run"
echo "3. Open https://localhost:5001 (or the URL shown in console)"
echo "4. Register a new account and start using Attendly!"
echo ""
echo "Optional:"
echo "- Set up Razorpay keys in appsettings.json for payment features"
echo "- Run 'dotnet watch run' for hot reload during development"
echo ""
