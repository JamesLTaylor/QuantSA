---
title: CreatePCACurveSimulator
keywords:
last_updated: September 29, 2016
tags:
summary: Create a curve simulator based on principle components.
sidebar: excel_sidebar
permalink: CreatePCACurveSimulator.html
folder: excel
---

## Description
Create a curve simulator based on principle components.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    Not available

## Arguments

* **simulatorName** The name of the simulator
* **anchorDate** The date from which the curve dates will be calculated.
* **initialRates** The starting rates.  Must be the same length as the elements in the component vectors.
* **tenorMonths** The months at which each rate applies.
* **components** The componenents.  Each component in a column.  Stack the columns side by side.
* **vols** The volatility for each component.  Must be the same length as the number of components.
* **multiplier** All rates will be multiplied by this amount.  This should almost always be 1.0.
* **useRelative** Indicates if the PCA was done on relative moves.  If not then it was done on absolute moves.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

