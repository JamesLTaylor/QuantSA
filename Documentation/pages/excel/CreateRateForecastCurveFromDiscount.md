---
title: CreateRateForecastCurveFromDiscount
keywords:
last_updated: September 29, 2016
tags:
summary: Create a curve to forecast floating interest rates based on a discount curve.
sidebar: excel_sidebar
permalink: CreateRateForecastCurveFromDiscount.html
folder: excel
---

## Description
Create a curve to forecast floating interest rates based on a discount curve.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    GeneralSwap.xlsx

## Arguments

* **objectName** The name of the object to be created.
* **floatingRateIndex** The floating rate that this curve will be used to forecast.([FloatingIndex](FloatingIndex.html))
* **discountCurve** The name of the discount curve that will be used to obtain the forward rates.
* **fixingCurve** Optional: The name of the fixing curve for providing floating rates at dates before the anchor date of the discount curve.  If it is left out then the first floating rate implied by the discount curve will be used for all historical fixes.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

