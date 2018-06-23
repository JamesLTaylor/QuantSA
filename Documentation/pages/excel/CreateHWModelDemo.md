---
title: CreateHWModelDemo
keywords:
last_updated: November 07, 2016
tags:
summary: Create demo Hull White model.
sidebar: excel_sidebar
permalink: CreateHWModelDemo.html
folder: excel
---

## Description
Create demo Hull White model.  Will be used for discounting and forecasting any indices specified.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    EPE.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **meanReversion** (Double)The constant rate of mean reversion.
* **flatVol** (Double)The constant short rate volatility.  Note that this is a Gaussian vol and will in general be lower than the vol that would be used in Black.
* **baseCurve** (IDiscountingSource)The curve to which zero coupon bond prices will be calibrated.
* **forecastIndices** ([FloatRateIndex](FloatRateIndex.html))The indices that should be forecast with this same curve.  No spreads are added.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

