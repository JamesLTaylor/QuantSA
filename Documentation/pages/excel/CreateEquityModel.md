---
title: CreateEquityModel
keywords:
last_updated: October 09, 2016
tags:
summary: Create a model that simulates multiple equites in one currency.
sidebar: excel_sidebar
permalink: CreateEquityModel.html
folder: excel
---

## Description
Create a model that simulates multiple equites in one currency.  Assumes lognormal dynamics.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    EquityValuation.xlsx

## Arguments

* **objectName** The name of the object to be created.
* **discountCurve** The discounting curve.  Will be used for discounting and as the drift rate for the equities.
* **shares** Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.(Share)
* **spotPrices** The values of all the shares on the anchor date of the discounting curve. 
* **volatilities** A single volatility for each share.
* **divYields** A single continuous dividend yield rate for each equity.
* **correlations** A square matrix of correlations between shares, the rows and columns must be in the same order as the shares were listed in shareCodes.
* **rateForecastCurves** The floating rate forecast curves for all the rates that the products in the portfolio will need.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

