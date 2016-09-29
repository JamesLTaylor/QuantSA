---
title: Getting started with QuantSA
keywords: 
tags: [getting_started]
sidebar: home_sidebar
permalink: index.html
summary: "A free, easy to use quant library for South Africa"
---


# QuantSA

QuantSA is a library of quantitative finance tools, it is also an Excel addin which is the most common way to interact with the library.

# Why?

In 2016 I found myself doing a few hours of consulting in Quantititive Finance.  I wrote one or two VBA functions and then the prospect of ever doing that again made me feel ill.  I decided I would save myself and as many others as possible from ever again writing a curve stripper, a Monte Carlo simulation or anything like that (especially in VBA).

Why not [QuantLib](http://quantlib.org/index.shtml) or simular?  QuantLib is extensive in its modelling and I intend to draw on its ideas and use it as a reference as much as possible.  QuantSA attempts to significantly lower the barriers to entry for usage and contribution by using C# rather than C++, often choosing just Monte Carlo implementations and having a smaller scope.  The South African quant community is small which means that if we control the library we can quickly reach consensus on its applicability in the local market.

Another key advantage that QuantSA offers over all other Quant libraries is its customizability, including:

* **[plugins](home_plugins.html)** - Allow developers to create user specific functins using the full power of QuantSA but not clutter the function space of the main library
* **scripts** - At run time new products can be created and valued.
* **control of function visibility** - Developers can either enable or disable sets of functions for their users, making the user experience more efficient less overwhelming.

# Features

* Free.
* 1 (or slightly more) click installation of the add-in into Excel.
* The most commonly used functions are quick and easy to use.
* User experience can be customized so that a subset of regularly used funcions can be shown and the rest hidden.
* Extra functionality can be added via plugins and not shared with the whole world.


# Objectives
* Make it as easy as possible to perform common valuations, including:
    * Derivative fair values.
    * CVA, DVA, etc.
    * Employee share scheme valuations.
    * BEE scheme valuations.
*  Have code that is well documented and easy to understand.
    * Will be used in teaching at Universities.    
    * Allows users to quickly gain confidence in the implementation if they wish.
* Have an architecture that prefers scalability across extra hardware in preference to complex model implementations.
* Provide a backend with market data that is in the public domain
    * This is to enable a new user to perform a first valuation as soon as a product is captured.
* Have models that are standard and whose validation is done in the open by academics and practitioners.
* Allow plugins where sophisticated users can expose more advanced features alongside standard features.


# Getting started - Excel users

* Download the add-in
* select the add-in from Excel
* start using the functions that start with `QSA.`

More details can be found in the [Installation page](home_installation.html) in the menu.


# Getting started - developers

* Download the source
* Compile and run the tests
* Look around the code
* Check the issue list
* Start contributing

More details can be found under the [Developers section](home_setup.html) in the menu. 



