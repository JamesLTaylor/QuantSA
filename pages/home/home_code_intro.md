---
title: Introduction to the code
keywords: 
last_updated: October 14, 2016
tags: [getting_started, developers]
sidebar: home_sidebar
permalink: home_code_intro.html
folder: home
---

## Project overview
There are 4 main projects:

* General
* Valuations
* Excel
* QuantSAInterfaces

and 2 test projects

###  `General` and `Valuation`
Most maths and finance except valuation is implemented in `General`.  The simulators and other valuation tools are implemented in `Valuations`.  The main reason for this separation is that it keeps the valuation philosphy enforced.  A product in `General` can have no reference to the `Valuations` project this forces the developer to consider only the [market observables](home_valuation_principles.html#market-observables) when implementing the cashflows of the product and not how these will be generated.

###  `QuantSAInterfaces`
These are the interfaces that are used by plugins.

### `Excel`
The actual functions that get exposed to Excel and some utilities and features for the main add-in.  This project heavily uses ExcelDNA.

Whenever possible the functions in here should be wrappers that only convert data before sending it through to functions in `General` and `Valuation`

See [here](home_expose_to_excel.html) for more details about expsosing functions to Excel.

## Project details - General

### Curves
Various curves for providing fixes or forecasts.

### CurveTools
Tools used to contruct or generate curves.

### DataAnalysis

### Formulae
Standard formulae such as the Black Scholes equation.

### MarketObservables

## Project details - Valuation

## Project details - Excel
Each `public static` function that wraps something in `General` or `Valuation` should sit in a class and file called `XLSomething` where `something` is the name of the project or source sub folder in which the called function resides. 
