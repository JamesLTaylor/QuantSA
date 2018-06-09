---
title: CreateFloatLeg
keywords:
last_updated: September 29, 2016
tags:
summary: Create a general floating leg of a swap.
sidebar: excel_sidebar
permalink: CreateFloatLeg.html
folder: excel
---

## Description
Create a general floating leg of a swap.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    GeneralSwap.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **currency** ([Currency](Currency.html))The currency of the cashflows. (Currency)
* **floatingIndex** ([FloatingIndex](FloatingIndex.html))A string describing the floating index.
* **resetDates** ([Date](Date.html)[])The dates on which the floating indices reset.
* **paymentDates** ([Date](Date.html)[])The dates on which the payments are made.
* **notionals** (Double[])The notionals on which the payments are based.
* **spreads** (Double[])The spreads that apply to the simple floating rates on each of the payment dates.
* **accrualFractions** (Double[])The accrual fraction to be used in calculating the fixed flow.  Will depend on the daycount convention agreed in the contract.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

