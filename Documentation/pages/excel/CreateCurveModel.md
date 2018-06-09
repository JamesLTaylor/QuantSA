---
title: CreateCurveModel
keywords:
last_updated: September 29, 2016
tags:
summary: Create a curve based valuation model.
sidebar: excel_sidebar
permalink: CreateCurveModel.html
folder: excel
---

## Description
Create a curve based valuation model.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    GeneralSwap.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **discountCurve** (IDiscountingSource)The discounting curve
* **rateForecastCurves** (IFloatingRateSource[])The floating rate forecast curves for all the rates that the products in the portfolio will need.
* **fxForecastCurves** *(IFXSource[])The FX rate forecast curves for all the cashflow currencies other than the discounting currency.(Default value = )

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

