---
title: Value
keywords:
last_updated: September 29, 2016
tags:
summary: Perform a general valuation.
sidebar: excel_sidebar
permalink: Value.html
folder: excel
---

## Description
Perform a general valuation.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    GeneralSwap.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map.  Should be unique.
* **products** (Product[])A list of products.
* **valueDate** ([Date](Date.html))The value date.
* **model** (NumeraireSimulator)A model able to handle all the market observables required to calculate the cashflows in the portfolio.
* **nSims** *(Int32)Optional.  The number of simulations required if the model requires simulation.  If left blank will use a default value depending on the model.(Default value = 1)

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

