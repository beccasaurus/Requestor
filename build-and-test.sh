#! /bin/bash
rm -rf Build
xbuild
nunit-color-console Build/Debug/Requestor.Specs.dll
