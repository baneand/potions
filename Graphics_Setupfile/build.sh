#!/bin/bash

set -e

rm -rf dist
pyinstaller --uac-admin GraphicsSettings.py