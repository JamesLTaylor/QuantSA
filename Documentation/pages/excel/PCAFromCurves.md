---
title: PCAFromCurves
keywords:
last_updated: September 29, 2016
tags:
summary: Perform a PCA on the log returns of a blob of curves.
sidebar: excel_sidebar
permalink: PCAFromCurves.html
folder: excel
---

## Description
Perform a PCA on the log returns of a blob of curves.

<!--HUMAN EDIT START-->

## Details

This function performs a completely standard [Principle Component Analysis](https://en.wikipedia.org/wiki/Principal_component_analysis) on the returns of a set of curves.

<script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>

The function treats each row of the blob as a curve.  For the result to be sensible each row must be the same length and each column must relate the same maturities across rows.  The time spacing between the observations in each row should also be approxiamtely constant, for example daily or monthly.  

Based on whether absolute or relative returns are selected, the returns are calculated either as:

<div>
$$ M_{return}(i,j) =  M_{rate}(i+1,j) - M_{rate}(i,j) $$
</div>

or 

<div>
$$ M_{return}(i,j) =  \ln \left( \frac{M_{rate}(i+1,j)}{M_{rate}(i,j)} \right) $$
</div>

Where the original blob is a matrix <span>\\( M_{rate} \\)</span> where element <span>\\( (i,j) \\)</span> is the rate with maturity <span>\\( (j) \\)</span> in row <span>\\( (i) \\).

<!--HUMAN EDIT END-->

## Example Sheet

    PCA.xlsx

## Arguments

* **curves** Blob of curves, each row is a curve of the same length.
* **useRelative** Indicates if the PCA is to be done on relative moves.  If not then it will be done on absolute moves.([Boolean](Boolean.html))

<!--HUMAN EDIT START-->

## Validation

The Principal Component analysis is performed using the singular value decomposition in [Accord.NET](http://accord-framework.net/).  Details on how to use Accord.NET and how the singular value decomposition can be used to perform a PCA can be found in the paper [A Tutorial on Principal Component Analysis with the Accord.NET Framework](https://arxiv.org/abs/1210.7463).

We further compared the results of the PCA to those produced by Matlab.  This can be seen in:

[PCATest.m](https://github.com/JamesLTaylor/QuantSA/blob/master/Validation/PCATest.m) and [PCATest.xlsx](https://github.com/JamesLTaylor/QuantSA/blob/master/Validation/PCATest.xlsx) 

<!--HUMAN EDIT END-->

