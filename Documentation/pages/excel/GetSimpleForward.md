---
title: GetSimpleForward
keywords:
last_updated: August 12, 2018
tags:
summary: Get a simple forward rate between two dates.
sidebar: excel_sidebar
permalink: GetSimpleForward.html
folder: excel
---

## Description
Get a simple forward rate between two dates.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    Caplet.xlsx

## Arguments

* **curve** (IDiscountingSource)The curve from which the forward is required.
* **startDate** ([Date](Date.html))The start date of the required forward.  Cannot be before the anchor date of the curve.
* **endDate** ([Date](Date.html))The end date of the required forward.  Must be after the startDate.
* **daycountConvention** *([DayCountConvention](DayCountConvention.html))The convention that the simple rate will be used with.(Default value = ACT365)

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

