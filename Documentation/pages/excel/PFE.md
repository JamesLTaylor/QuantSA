---
title: PFE
keywords:
last_updated: March 18, 2017
tags:
summary: An approximate PFE for a portfolio of trades.
sidebar: excel_sidebar
permalink: PFE.html
folder: excel
---

## Description
An approximate PFE for a portfolio of trades.

<!--HUMAN EDIT START-->

<!--## Details-->

<!--HUMAN EDIT END-->

## Example Sheet

    PFE.xlsx

## Arguments

* **products** A list of products.
* **valueDate** The value date.([Date](Date.html))
* **forwardValueDates** The dates at which the expected positive exposure is required.([Date](Date.html))
* **requiredPecentiles** The required percentiles.  95th percentile should be entered as 0.95.  Can be a list of percentiles and the PFE will be calculated at each of the provided levels.
* **model** A model able to handle all the market observables required to calculate the cashflows in the portfolio.
* **nSims** The number of simulations required.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

