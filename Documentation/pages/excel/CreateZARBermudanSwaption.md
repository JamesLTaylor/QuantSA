---
title: CreateZARBermudanSwaption
keywords:
last_updated: December 04, 2016
tags:
summary: Create a ZAR Bermudan swaption based a ZAR quarterly, fixed for float Jibar swap.
sidebar: excel_sidebar
permalink: CreateZARBermudanSwaption.html
folder: excel
---

## Description
Create a ZAR Bermudan swaption based a ZAR quarterly, fixed for float Jibar swap.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    BermudanSwaption.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **exerciseDates** ([Date](Date.html)[])The exercise dates.  The dates on which the person who is long optionality can exercise.
* **longOptionality** ([Boolean](Boolean.html))if set to TRUE then the person valuing this product owns the optionality.
* **startDate** ([Date](Date.html))First reset date of the underlying swap.
* **tenor** ([Tenor](Tenor.html))Tenor of underlying swap, must be a whole number of years.  Example '5Y'.
* **rate** (Double)The fixed rate paid or received on the underlying swap.
* **payFixed** ([Boolean](Boolean.html))Is the fixed rate paid? Enter 'TRUE' for yes.
* **notional** (Double)Flat notional for all dates.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

