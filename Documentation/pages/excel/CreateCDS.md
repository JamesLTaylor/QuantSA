---
title: CreateCDS
keywords:
last_updated: December 04, 2016
tags:
summary: Create a par style CDS.
sidebar: excel_sidebar
permalink: CreateCDS.html
folder: excel
---

## Description
Create a par style CDS.  Protection will always apply from the value date until the last payment date.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    CDS.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **refEntity** (ReferenceEntity)The reference entity whose default is covered by this CDS.
* **ccy** ([Currency](Currency.html))The currency of the cashflows of the premium and default legs.
* **paymentDates** ([Date](Date.html)[])The payment dates on which the premium is paid.
* **notionals** (Double[])The notionals that define the protection amount in the period until each payment date and the basis on which the premiums are calculated.
* **rates** (Double[])The simple rates that apply until the default time.  Used to calculate the premium flows.
* **accrualFractions** (Double[])The accrual fractions used to calculate the premiums paid on the paymentDates.
* **boughtProtection** ([Boolean](Boolean.html))If set to TRUE then protection has been bought and the premium will be paid.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

