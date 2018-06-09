---
title: CreateLoanFloatingRate
keywords:
last_updated: October 31, 2016
tags:
summary: Create a floating rate loan.
sidebar: excel_sidebar
permalink: CreateLoanFloatingRate.html
folder: excel
---

## Description
Create a floating rate loan.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    Loans.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **currency** ([Currency](Currency.html))The currency of the cashflows.
* **balanceDates** ([Date](Date.html)[])The dates on which the loan balances are known.  All dates other than the first one will be assumed to also be cashflow dates.
* **balanceAmounts** (Double[])The notionals on which the payments are based.
* **floatingIndex** ([FloatingIndex](FloatingIndex.html))The reference index on which the floating flows are based.
* **floatingSpread** (Double)The spread that will be added to the floating index.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

