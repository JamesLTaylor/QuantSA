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

* **objectName** The name of the object to be created.
* **referenceEntity** The reference entity for whom these hazard rates apply.(ReferenceEntity)
* **anchorDate** The anchor date.  Survival probabilites can only be calculated up to dates after this date.([Date](Date.html))
* **dates** The dates on which the hazard rates apply.([Date](Date.html))
* **hazardRates** The hazard rates.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

