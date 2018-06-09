---
title: Controlling user functions
summary: It is possible to either enable or disable which functions a user can see.
keywords: 
last_updated: September 23, 2016
tags: 
sidebar: home_sidebar
permalink: home_exposed_functions.html
folder: home
---

# Introduction

I have seen other Excel libraries with many hundreds of available functions, this can make it very difficult for a new user to find the function they are looking for.  With this functionality a QuantSA developer or experienced user can point a new user to the functions they need and edit the file `functions_user.csv` in the addin directory to only make that subset of functions visible.

All functions will always be available for use but only the selected set will appear in the Excel function wizard.   

# Architecture

When a new function is written its default visibility is controlled with the `IsHidden` attribute value.  Note that if it is not intended that many users will want access to a new function the developer should consider putting it in a [Plugin](home_plugins.html)

# How to control visibility

The first time Excel is started up with the QuantSA add-in activated a new file: ``functions_user.csv`` will be created in `C:\Users\[username]\AppData\Roaming\QuantSA`.  And it will look something like:

| QSA.ApplyBusinessDayAdjustment | yes |
|    QSA.CovarianceFromCurves    | yes |
|  QSA.CreateBusiness252DayCount | yes |
|        QSA.CreateCashLeg       | yes |
|              ....              | ... |

The values in the second column are the default visibility.  To hide functions change the `yes`s to `no`s.

