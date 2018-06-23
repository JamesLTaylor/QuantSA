---
title: Introduction to the code
keywords: 
last_updated: June 11, 2018
tags: [getting_started, developers]
sidebar: home_sidebar
permalink: home_code_intro.html
folder: home
---

## Project overview
Within the main QuantSA solution there There are 4  projects other than the Excel and test projects:

* QuantSA.Shared
* QuantSA.Core
* QuantSA.CoreExtensions
* QuantSA.Valuations

### QuantSA.Shared

This contains all the most basic types and interfaces that are then implemented in other projects or in user's private projects.

### QuantSA.Core

Implementations of products and general tools that are useful for Valuation and CoreExtensions.

### Valuation

The simulators and other valuation tools are implemented in `Valuations`.  The main reason for this separation is that it keeps the valuation philosophy enforced.  A product in `Core` can have no reference to the `Valuations` project this forces the developer to consider only the [market observables](home_valuation_principles.html#market-observables) when implementing the cashflows of the product and not how these will be generated.

### QuantSA.CoreExtensions

Extension methods of objects implemented in Core that we want to exist completely independently of the valuation framework.  For example a closed for option price.  Calibrators will typically be implemented in CoreExtensions as well since they are likely to require such closed form expressions.

## Project details - Shared

