---
title: Day count conventions
keywords:
last_updated: March 18, 2017
tags:
summary: Known day count conventions and how to make them.
sidebar: excel_sidebar
permalink: DayCountConvention.html
folder: excel
---

## Description

## Allowed values

 * `ACT365`, `ACT365F` - Actual 365 Fixed - The number of days over 365.
 * `30360EU` - The 30/360 convention with European adjustments.
 * `ACTACT` - Actual/actual 
 * `ACT360` - Actual/360 - The number of days over 360.
 
a `Business252` convention cannot be made directly from a string since it needs a calendar.  In this is required use [CreateBusiness252DayCount](CreateBusiness252DayCount.html).
 
## Related functions

 * [GetYearFraction](GetYearFraction.html)
 * [CreateBusiness252DayCount](CreateBusiness252DayCount.html)
