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

* **objectName** The name of the object to be created.
* **currency** The currency of the cashflows. ([Currency](Currency.html))(Currency)
* **floatingIndex** A string describing the floating index.([FloatingIndex](FloatingIndex.html))
* **resetDates** The dates on which the floating indices reset.([Date](Date.html))
* **paymentDates** The dates on which the payments are made.([Date](Date.html))
* **notionals** The notionals on which the payments are based.
* **spreads** The spreads that apply to the simple floating rates on each of the payment dates.
* **accrualFractions** The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

