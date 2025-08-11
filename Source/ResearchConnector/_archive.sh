#!/bin/bash

# Path to WinRAR executable (adjust as needed)
WINRAR_PATH="C:/Program Files/WinRAR/WinRAR.exe"

# Output archive name with current date
ARCHIVE_NAME="cs_files_$(date +%Y%m%d).zip"

# Folder to search (current directory in this example)
SEARCH_DIR="."

# Find all .cs files recursively
CS_FILES=$(find "$SEARCH_DIR" -type f -name "*.cs")

# Remove existing archive if it exists
if [ -f "$ARCHIVE_NAME" ]; then
    echo "Removing existing archive: $ARCHIVE_NAME"
    rm -f "$ARCHIVE_NAME"
fi

# Check if we found any files
if [ -z "$CS_FILES" ]; then
    echo "No .cs files found."
else
    # Create the ZIP archive (recursive)
    "$WINRAR_PATH" a -afzip -ep1 "$ARCHIVE_NAME" $CS_FILES
    echo "Archive created: $ARCHIVE_NAME"
fi

# Wait for any key before exiting
read -n 1 -s -r -p "Press any key to exit..."
echo
