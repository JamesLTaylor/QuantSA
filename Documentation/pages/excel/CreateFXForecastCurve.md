---
title: CreateFXForecastCurve
keywords:
last_updated: September 29, 2016
tags:
summary: Create a curve to be used for FX rate forecasting.
sidebar: excel_sidebar
permalink: CreateFXForecastCurve.html
folder: excel
---

## Description
Create a curve to be used for FX rate forecasting.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    GeneralSwap.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map.  Should be unique.
* **baseCurrency** ([Currency](Currency.html))The base currency.  Values are measured in units of counter currency per one base currency.(Currency)
* **counterCurrency** ([Currency](Currency.html))The counter currency.  Values are measured in units of counter currency per one base currency.(Currency)
* **fxRateAtAnchorDate** (Double)The rate at the anchor date of the two curves.
* **baseCurrencyFXBasisCurve** (IDiscountingSource)A curve that will be used to obatin forward rates.
* **counterCurrencyFXBasisCurve** (IDiscountingSource)A curve that will be used to obtain forward rates.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

