---
title: CreateZARSwap
keywords:
last_updated: September 29, 2016
tags:
summary: Create a ZAR quarterly, fixed for float Jibar swap.
sidebar: excel_sidebar
permalink: CreateZARSwap.html
folder: excel
---

## Description
Create a ZAR quarterly, fixed for float Jibar swap.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    ZARSwap.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **startDate** ([Date](Date.html))First reset date of the swap
* **tenor** ([Tenor](Tenor.html))Tenor of swap, must be a whole number of years.  Example '5Y'.
* **rate** (Double)The fixed rate paid or received
* **payFixed** ([Boolean](Boolean.html))Is the fixed rate paid? Enter 'TRUE' for yes.
* **notional** (Double)Flat notional for all dates.
* **jibar** *([FloatRateIndex](FloatRateIndex.html))The float rate index of the swap.(Default value = DEFAULT)

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

