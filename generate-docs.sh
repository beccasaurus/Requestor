#! /bin/sh

echo "Removing old documentation"
rm -rf XmlDocs
rm -rf HtmlDocs

echo "Importing XML documentation"
monodocer -pretty -importslashdoc:Build/Debug/Requestor.xml -assembly:Build/Debug/Requestor.dll -path:XmlDocs/

echo "Exporting Html documents"
mdoc export-html --out=HtmlDocs XmlDocs/

echo "Listing HtmlDocs"
ls HtmlDocs/
