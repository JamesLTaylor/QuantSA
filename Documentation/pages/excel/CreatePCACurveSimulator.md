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

    PCA.xlsx

## Arguments

* **objectName** The name of the object to be created.
* **anchorDate** The date from which the curve dates will be calculated.([Date](Date.html))
* **initialRates** The starting rates.  Must be the same length as the elements in the component vectors.
* **tenors** An array of times at which each rate applies.  Each value must be valid tenor description.  The length must be the same as each component and 'initialRates'([Tenor](Tenor.html))
* **components** The componenents.  Stack the components in columns side by side or rows one underneath each other.
* **vols** The volatility for each component.  Must be the same length as the number of components.
* **multiplier** All rates will be multiplied by this amount.  This should almost always be 1.0.
* **useRelative** Indicates if the PCA was done on relative moves.  If not then it was done on absolute moves.([Boolean](Boolean.html))
* **floorAtZero** Should simulated rates be floored at zero?  This only applies to absolute moves, the default is 'True'.([Boolean](Boolean.html))

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

