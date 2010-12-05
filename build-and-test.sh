#! /bin/bash
rm -rf Build
xbuild
Tools/mspec.exe Build/Debug/Requestor.Specs.dll
