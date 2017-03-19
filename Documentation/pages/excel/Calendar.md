---
title: Calendars
keywords:
last_updated: March 18, 2017
tags:
summary: The known calendars and creating calendars from files.
sidebar: excel_sidebar
permalink: Calendar.html
folder: excel
---

## Description

Calendars are loaded from files in the `\StaticData\Holidays` of the Excel addin install directory.  Any file here can be turned into a QuantSA calendar by specifying the name of the file without the `.csv` extension.

## Format of files

A csv file where each line contains a date in the format `yyyy-mm-dd` for example christmas is `2017-12-25`

## Common files

The QuantSA installation usually includes:


 * `ZA` - The south african public holidays.
 * `TEST` - A test calendar that will never change.  Useful if you are testing functions and don't want the results to potentially change if a new public holiday happens to be declared.

## Related functions

 * [IsHoliday](IsHoliday)
