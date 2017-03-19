---
title: CreateMultiHWAndFXToy
keywords:
last_updated: March 18, 2017
tags:
summary: A sample model that simulates FX processes according to geometric brownian motion and short rates according to Hull White.
sidebar: excel_sidebar
permalink: CreateMultiHWAndFXToy.html
folder: excel
---

## Description
A sample model that simulates FX processes according to geometric brownian motion and short rates according to Hull White.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    PFE.xlsx

## Arguments

* **objectName** The name of the object to be created.
* **anchorDate** The date from which the model applies([Date](Date.html))
* **numeraireCcy** The currency into which all valuations will be converted.([Currency](Currency.html))
* **rateSimulators** Hull White simulators for each of the currencies
* **currencies** The list of other currencies to be simulated.([Currency](Currency.html))
* **spots** The initial values for the FX processes at the anchor date.  These would actually need to be discounted spot rates.
* **vols** The volatilities for the FX processes.
* **correlations** A correlation matrix for the FX processes, rows and columns must be in the order of the currencies in 'currencies'

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

