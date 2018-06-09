---
title: CreateLoanFixedRate
keywords:
last_updated: October 31, 2016
tags:
summary: Create fixed rate loan.
sidebar: excel_sidebar
permalink: CreateLoanFixedRate.html
folder: excel
---

## Description
Create fixed rate loan.

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
* **fixedRate** (Double)The simple rates that are paid at each payment date.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

