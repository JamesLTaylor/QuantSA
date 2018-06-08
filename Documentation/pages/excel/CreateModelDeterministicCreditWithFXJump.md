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

* **objectName** The name that this object will be assigned on the map.  Should be unique.
* **survivalProbSource** (ISurvivalProbabilitySource)A curve that provides survival probabilities.  Usually a hazard curve.
* **otherCurrency** ([Currency](Currency.html))The other currency required in the simulation.  The valuation currency will be inferred from the valueCurrencyDiscount.  This value needs to be explicitly set since fxSource may provide multiple pairs.
* **fxSource** (IFXSource)The source FX spot and forwards.
* **valueCurrencyDiscount** (IDiscountingSource)The value currency discount curve.
* **fxVol** (Double)The fx volatility.
* **relJumpSizeInDefault** (Double)The relative jump size in default.  For example if the value currency is ZAR and the other currency is USD then the fx is modelled as ZAR per USD and in default the fx rate will change to: rate before default * (1 + relJumpSizeInDefault).
* **expectedRecoveryRate** (Double)The constant recovery rate that will be assumed to apply in default.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

