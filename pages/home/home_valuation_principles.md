---
title: Valuation Principles
keywords: 
last_updated: September 18, 2016
tags: 
sidebar: home_sidebar
permalink: home_valuation_principles.html
folder: home
---

# Products

A product is an encapsulation of a financial contract.  It only knows:

   * Which [market obserables](home_valuation_principles.html#market-observables) determine its cashflows. 
   * How the observables are used to calculate the cashflows.
   * On what dates the cashflows will take place.

A product does not in general have any implementation of a valuation or knowledge of the models that could be used to value it.

## Market Observables
These are quantities that can be observed unambigiously in the market and are referenced in contracts betweem parties to determine (either directly or indirectly) what the amount of money one party needs to the other.

Examples include:

* A share price
* A floating rate index
* A value for the consumer price index
* The value of one currency in terms of units of another currency, i.e. an exchange rate 

## Valuation Usage

A typical valuation model will follow this process to find the value of a product.

1. The model will tell the contract after which date cashflows will be needed.  
1. The model will then ask the contract which market obserables it depends on.
1. For each of these observables the model will:
    1. Ask the contract at which dates it needs to know the indices.
    1. By some means produce relaiztion of these indices and set them in the product.
1. At this point the product knows  realizations of the market observables and can calculate its cashflows.

This same method can be used to get realized cashflows for settlement purposes.

## Valuation Formula

<script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>
<div>
In general any cashflow \( CF \) at time \(T\) valued at time \(t_0\) is a function of market obserbles \( S_i \) observed at times \( t_i \leq T \) will have the canonical valuation formula:

$$V(t_0) = \mathbb{E} ^ \mathbb{Q} \left[ \frac{1}{B(t,T)} CF(S_1(t_1), S_2(t_2), ...) \right]$$

When there is some optionality or exercise decision in general the formula can become slightly less well defined.  After the last possible exercise time the cashflows can be valued the same as above.  Meaning we can use the above to find \( V(T_{En}) \) the value at the last exercise date \( T_{En} \)  
</div>




   


