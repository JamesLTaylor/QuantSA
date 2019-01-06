---
title: Curve stripping
summary: The maths of multi curve stripping
keywords: 
last_updated: December 31, 2018
tags: 
sidebar: home_sidebar
permalink: home_curve_stripping.html
folder: home
---

<script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>

# Overview


# Multi Dimensional Newton

<div>
we are trying to solve 

$$ f(x) = 0 $$

Where \( f(x) \) is a vector valued function with each element the value of one of the benchmark instruments, and \( x \) is the column vector of curve pillar point values (either discount factors or rates)
</div>

This can be solved using [Broyden's method in MathNet](https://numerics.mathdotnet.com/api/MathNet.Numerics.RootFinding/Broyden.htm) or with the QuantSA implementation of a multi dimensional Newton.

<div>
The QuantSA version proceeds by approximating \(f(x)\) as linear:

$$ f(x+\delta) \approx f(x) + J(x)\delta $$

Where \(J\) is the Jacobian matrix with 

$$J_{ij} = \frac{\partial f_i}{\partial x_j}$$

since we require 

$$f(x+\delta)=0$$

we estimate

$$\delta  = -J(x)^{-1}f(x)$$

</div>

   


