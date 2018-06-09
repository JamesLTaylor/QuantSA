---
title: FormulaBlackScholes
keywords:
last_updated: October 09, 2016
tags:
summary: The Black Scholes formula for a call.
sidebar: excel_sidebar
permalink: FormulaBlackScholes.html
folder: excel
---

## Description
The Black Scholes formula for a call.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    EquityValuation.xlsx

## Arguments

* **strike** (Double)Strike
* **valueDate** ([Date](Date.html))The value date as and Excel date.
* **exerciseDate** ([Date](Date.html))The exercise date of the option.  Must be greater than the value date.
* **spotPrice** (Double)The spot price of the underlying at the value date.
* **vol** (Double)Annualized volatility.
* **riskfreeRate** (Double)Continuously compounded risk free rate.
* **divYield** *(Double)Continuously compounded dividend yield.(Default value = 0.0)

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

