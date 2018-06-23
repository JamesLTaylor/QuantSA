---
title: Floating rate index
keywords:
last_updated: Jun 23, 2018
tags:
summary: The floating rate index input type
sidebar: excel_sidebar
permalink: FloatRateIndex.html
folder: excel
---

## Description
Many functions require the input of a FloatingIndex.  There is no idea of a general floating rate in QuantSA, the indices are all well defined.  This is more in keeping with the multicurve valuation standard.  Any time a forward rate is required the user needs to think about which forward rate it is so that the correct forecast curve is always used.

A FloatingRateIndex is  entered into QuantSA functions string.  See below for the supported values.

## Available values

* **JIBAR1M**
* **JIBAR3M**
* **JIBAR6M**
* **PRIME1M_AVG** The average SA prime rate for 1 month.
* **LIBOR1M** 
* **LIBOR3M** 
* **LIBOR6M** 
* **EURIBOR3M** 
* **EURIBOR6M** 

