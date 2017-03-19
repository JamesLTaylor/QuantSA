---
title: Business day conventions
keywords:
last_updated: March 18, 2017
tags:
summary: Busininess day convention input type
sidebar: excel_sidebar
permalink: BusinessDayConvention.html
folder: excel
---

## Description

A business day convention adjusts a date that may represent a non business day to another dates that is a good business day.  Different conventions do this in different ways but for all of them the date is unadjusted if it is already a business day.

## Allowed values

All allowed values are:

 * `F`, `FOLLOWING` - the next business day.
 * `MF`, `MODFOLLOW`, `MODIFIEDFOLLOWING` - the next business day unless that is in the next month then the previous business day.
 * `P`, `PRECEDING` - the previous business day.
 * `MP`, `MODIFIEDPRECEDING` - non standard convention. The previous business day unless that is in the previous month then the next business day.
 * `U`, `UNADJUSTED` - do no adjustment even if the provided date is a holiday.


## Related functions     

 * [ApplyBusinessDayAdjustment](ApplyBusinessDayAdjustment.html)
 
