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

    Not available

## Arguments

* **objectName** The name of the object to be created.
* **exerciseDates** The exercise dates.  The dates on which the person who is long optionality can exercise.([Date](Date.html))
* **longOptionality** if set to TRUE then the person valuing this product owns the optionality.([Boolean](Boolean.html))
* **startDate** First reset date of the underlying swap.([Date](Date.html))
* **tenor** Tenor of underlying swap, must be a whole number of years.  Example '5Y'.(Tenor)
* **rate** The fixed rate paid or received on the underlying swap.
* **payFixed** Is the fixed rate paid? Enter 'TRUE' for yes.([Boolean](Boolean.html))
* **notional** Flat notional for all dates.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

