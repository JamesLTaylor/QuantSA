---
title: Converting excel types
keywords: 
last_updated: March 18, 2017
tags: [developers]
sidebar: home_sidebar
permalink: home_converting_excel_types.html
folder: home
summary: "Converting from values in spreadsheets to objects used in QuantSA"
---

## Introduction

Many objects used in QuantSA are more complex than strings, dates or numbers that can be typed into cells in a spreadsheet.  Most of the required conversions take place in `QuantSA.Excel.ExcelUtilities`.  The two main special types are:

 * Types that can be created directly from strings, and
 * Types that need to be created by a call to QuantSA and the name of the object is placed in the spreadsheet.

## Types from strings

Objects such as daycount conventions, calendars or floating rate indices can be made directly from strings.  These types are all described [here](Boolean.html)

### Adding a new type that can be created from a string.

When adding a new type that can be created from a string one needs to do the following things:

 1. Write a converter for this type in `QuantSA.Excel.ExcelUtilities`
 1. Add the type to `QuantSA.Excel.ExcelUtilities.InputTypeShouldHaveHelpLink`
 1. Update the XL code generator at `GenerateXLCode.TypeInformation.InputTypeHasConversion`
 1. Update the help generator at `PrepareRelease.MarkdownGenerator.InputTypeShouldHaveHelpLink`
 1. Write a page for the new type in `\Documentation\pages\excel`, this page should describe what values are permissable
 1. Update `\Documentation\_data\sidebars\excel_sidebar.yml` to point to this new page.
 

## Objects created by QuantSA

These are all handled automatically, if you add an excel function that returns some complex object and allow the excel wrapper code to be generated automatically as described in [Exposing Functions to Excel](home_expose_to_excel.html).  Similarly if your function requires a complex object as an input, that too will be handled automatically if the excel wrapper code is generated.
