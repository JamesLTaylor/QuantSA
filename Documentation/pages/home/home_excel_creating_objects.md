---
title: Creating Objects
keywords: 
last_updated: September 14, 2016
tags: [getting_started]
sidebar: home_sidebar
permalink: home_excel_creating_objects.html
folder: home
---

Many QuantSA functions create objects.  These can then be used in other functions or data can be extracted directly from them.  Objects are available to use in Excel via the names that is output into the sheet.  The name will look something like:

```
simulationResults.12:29:14-8
```

## Extracting data

If an object stores any data then a call to  `=QSA.GetAvailableResults()` on the output to a previous function call will produce a list of available pieces of data.

These individual pieces can then be obtained with a call to `=QSA.GetResults())` using the reference to the object and the exact name returned above.

##  Excel Array Formulas

Many of the function in QSA return an array of results.  To use the excel array functionality select a range of output cells after typing the function call and press `CTRL+SHIFT+ENTER`.  The output will then fill the selected range, if the selected range is too large the extra cells will be filled with `#N/A` this is not an error.
