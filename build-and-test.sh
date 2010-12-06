#! /bin/bash
rm -rf Build
xbuild
MSPEC_PATH=Tools/mspec.exe Tools/mspec-color.exe Build/Debug/Requestor.Specs.dll
