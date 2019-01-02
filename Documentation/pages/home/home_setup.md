---
title: Setup a development environment
keywords: 
last_updated: August 26, 2018
tags: [getting_started]
sidebar: home_sidebar
permalink: home_setup.html
folder: home
---

## Github

QuantSA is written in C#.  It includes visual Studio 2017 solution files so the easiest way to get started is to open the solution in Visual Studio and begin writing.

* If you don't already have it download and install [Visual Studio Community Edition](https://visualstudio.microsoft.com/) 
* Download the source from <https://github.com/JamesLTaylor/QuantSA>
* Open `QuantSA.sln`
* Rebuild (Visual studio will sort out the project dependencies)

## Debugging with the Excel Add-in
* Set `QuantSA.Excel.Addin` as the startup project
* In the settings for that project go to 
`Debug -> Start External Program` and browse to your installed version of Microsoft Excel (for example: C:\Program Files (x86)\Microsoft Office\Office14\EXCEL.EXE).
* In `Debug -> Command Line Arguments` type `"QuantSA.xll"`
* Several example sheets can be found in the QuantSA repo under QuantSA/ExampleSheets
 

## Running tests
* Each project comes with its own test project, find more details [here](home_tests.html).