#!/bin/bash
# Install QuestPDF for PDF invoice generation
# Run this script from the solution root directory

echo "========================================"
echo "Installing PDF Invoice Dependencies"
echo "========================================"
echo ""

echo "Installing QuestPDF in CateringEcommerce.BAL..."
cd CateringEcommerce.BAL
dotnet add package QuestPDF
cd ..

echo ""
echo "Optional: Installing Humanizer for number-to-words conversion..."
cd CateringEcommerce.BAL
dotnet add package Humanizer
cd ..

echo ""
echo "========================================"
echo "Installation Complete!"
echo "========================================"
echo ""
echo "Next Steps:"
echo "1. Place your company logo at: CateringEcommerce.API/wwwroot/logo.png"
echo "2. Update company details in appsettings.json"
echo "3. Create wwwroot/invoices folder for PDF output"
echo ""
