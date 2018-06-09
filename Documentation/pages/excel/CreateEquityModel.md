---
title: CreateEquityModel
keywords:
last_updated: October 09, 2016
tags:
summary: Create a model that simulates multiple equities in one currency.
sidebar: excel_sidebar
permalink: CreateEquityModel.html
folder: excel
---

## Description
Create a model that simulates multiple equities in one currency.  Assumes lognormal dynamics.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    EquityValuation.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **discountCurve** (IDiscountingSource)The discounting curve.  Will be used for discounting and as the drift rate for the equities.
* **shares** ([Share](Share.html)[])Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.
* **spotPrices** (Double[])The values of all the shares on the anchor date of the discounting curve. 
* **volatilities** (Double[])A single volatility for each share.
* **divYields** (Double[])A single continuous dividend yield rate for each equity.
* **correlations** (Double[,])A square matrix of correlations between shares, the rows and columns must be in the same order as the shares were listed in shareCodes.
* **rateForecastCurves** *(IFloatingRateSource[])The floating rate forecast curves for all the rates that the products in the portfolio will need.(Default value = )

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

