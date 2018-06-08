---
title: CreateFixedLeg
keywords:
last_updated: September 29, 2016
tags:
summary: Create a general fixed leg of a swap.
sidebar: excel_sidebar
permalink: CreateFixedLeg.html
folder: excel
---

## Description
Create a general fixed leg of a swap.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    GeneralSwap.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map.  Should be unique.
* **currency** ([Currency](Currency.html))The currency of the cashflows.
* **paymentDates** ([Date](Date.html)[])The dates on which the payments are made.
* **notionals** (Double[])The notionals on which the payments are based.
* **rates** (Double[])The simple rates that are paid at each payment date.
* **accrualFractions** (Double[])The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

