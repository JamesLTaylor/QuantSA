---
title: CovarianceFromCurves
keywords:
last_updated: September 29, 2016
tags:
summary: Get the covariance in log returns from a blob of curves.
sidebar: excel_sidebar
permalink: CovarianceFromCurves.html
folder: excel
---

## Description
Get the covariance in log returns from a blob of curves.

<!--HUMAN EDIT START-->

## Details
<script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>

The function treats each row of the blob as a curve.  For the result to be sensible each row must be the same length and each column must relate the same maturities across rows.  

The result is a covariance matrix of the log returns.

More precisely, if the original blob is a matrix <span>\\( M_{rate} \\)</span> where element <span>\\( (i,j) \\)</span> is the rate with maturity <span>\\( (j) \\)</span> in row <span>\\( (i) \\)</span> then we construct a new matrix:

<div>
$$ M_{return}(i,j) =  \ln \left( \frac{M_{rate}(i+1,j)}{M_{rate}(i,j)} \right) $$
</div>

And the result is a matrix where:

<div>
$$ M_{result}(i,j) = Cov\left( M_{return}(,i),M_{return}(,j) \right) $$
</div>

<!--HUMAN EDIT END-->

## Example Sheet

    PCA.xlsx

## Arguments

* **curves** (Double[,])Blob of curves, each row is a curve of the same length.

<!--HUMAN EDIT START-->

<!--## Validation-->

<!--HUMAN EDIT END-->

