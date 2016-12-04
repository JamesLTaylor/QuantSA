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

* **objectName** The name of the object to be created.
* **refEntity** The reference entity whose default is covered by this CDS.(ReferenceEntity)
* **ccy** The currency of the cashflows of the premium and default legs.([Currency](Currency.html))
* **paymentDates** The payment dates on which the premium is paid.([Date](Date.html))
* **notionals** The notionals that define the protection amount in the period until each payment date and the basis on which the premiums are calculated.
* **rates** The simple rates that apply until the default time.  Used to calculate the premium flows.
* **accrualFractions** The accrual fractions used to calculate the premiums paid on the paymentDates.
* **boughtProtection** If set to TRUE then protection has been bought and the premium will be paid.([Boolean](Boolean.html))

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

