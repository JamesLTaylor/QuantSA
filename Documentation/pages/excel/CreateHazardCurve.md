---
title: CreateHazardCurve
keywords:
last_updated: December 04, 2016
tags:
summary: Create hazard rate curve that can be used to provide survival probabilities for a reference entity between dates.
sidebar: excel_sidebar
permalink: CreateHazardCurve.html
folder: excel
---

## Description
Create hazard rate curve that can be used to provide survival probabilities for a reference entity between dates.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    CDS.xlsx

## Arguments

* **objectName** The name that this object will be assigned on the map.  Should be unique.
* **referenceEntity** (ReferenceEntity)The reference entity for whom these hazard rates apply.
* **anchorDate** ([Date](Date.html))The anchor date.  Survival probabilities can only be calculated up to dates after this date.
* **dates** ([Date](Date.html)[])The dates on which the hazard rates apply.
* **hazardRates** (Double[])The hazard rates.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

