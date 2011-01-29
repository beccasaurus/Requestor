#! /bin/bash
rm -rf bin
rm -rf TestResult.xml
xbuild
nunit-color-console -labels "$@" bin/Debug/Requestor.Specs.dll
