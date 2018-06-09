---
title: CreateMultiHWAndFXToy
keywords:
last_updated: March 18, 2017
tags:
summary: A sample model that simulates FX processes according to geometric Brownian motion and short rates according to Hull White.
sidebar: excel_sidebar
permalink: CreateMultiHWAndFXToy.html
folder: excel
---

## Description
A sample model that simulates FX processes according to geometric Brownian motion and short rates according to Hull White.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    PFE.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map. Should be unique.
* **anchorDate** ([Date](Date.html))The date from which the model applies
* **numeraireCcy** ([Currency](Currency.html))The currency into which all valuations will be converted.
* **rateSimulators** (HullWhite1F[])Hull White simulators for each of the currencies
* **currencies** ([Currency](Currency.html)[])The list of other currencies to be simulated.
* **spots** (Double[])The initial values for the FX processes at the anchor date.  These would actually need to be discounted spot rates.
* **vols** (Double[])The volatilities for the FX processes.
* **correlations** (Double[,])A correlation matrix for the FX processes, rows and columns must be in the order of the currencies in 'currencies'

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

