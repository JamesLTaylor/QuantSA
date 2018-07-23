---
title: CreateModelDeterministicCreditWithFXJump
keywords:
last_updated: December 04, 2016
tags:
summary: Create a model that will simulate a single FX process and default for a single reference entity.
sidebar: excel_sidebar
permalink: CreateModelDeterministicCreditWithFXJump.html
folder: excel
---

## Description
Create a model that will simulate a single FX process and default for a single reference entity.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    CDS.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **survivalProbSource** (ISurvivalProbabilitySource)A curve that provides survival probabilities.  Usually a hazard curve.
* **currencyPair** ([Currency](Currency.html)Pair)The currency pair to be simulated.  It should have the value currency as its counter currency.
* **fxSource** (IFXSource)The source FX spot and forwards.
* **valueCurrencyDiscount** (IDiscountingSource)The value currency discount curve.
* **fxVol** (Double)The FX volatility.
* **relJumpSizeInDefault** (Double)The relative jump size in default.  For example if the value currency is ZAR and the other currency is USD then the fx is modelled as ZAR per USD and in default the fx rate will change to: rate before default * (1 + relJumpSizeInDefault).
* **expectedRecoveryRate** (Double)The constant recovery rate that will be assumed to apply in default.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

