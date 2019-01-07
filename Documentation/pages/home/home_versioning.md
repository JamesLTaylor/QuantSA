---
title: Versioning the Library
summary: Versioning the library and Excel add-in
keywords: 
last_updated: December 26, 2016
tags: developers
sidebar: home_sidebar
permalink: home_versioning.html
folder: home
---

# Mechanics

 * The dlls are versioned by the contents of `./QuantSA/AssemblyInfoAll.cs`.
 * The version displayed alongside the Excel add-in is the string in `./QuantSA/QuantSA.Excel.AddIn/QuantSA.dna`.
 * The version displayed in the help pages is in `./Documentation/_config.yml`

Once a build is released these files will be updated to the next target version in the repo and all development in the master branch will be towards

# Policy

Versions should follow <http://semver.org/>: MAJOR.MINOR.PATCH, which is not quite consistent with the visual studio standard of MAJOR.MINOR.BUILDNO.REVISION  

At the moment we are heading towards a `1.0` release.  That would be a version that provides a reasonable amount of functionality, has had 100s of hours of testing in the wild and has no outstanding issues that could affect the public API in Excel or in QuantSA.Valuation and QuantSA.General.

The plan for now is to run through a few versions of the form `0.1.0-alpha`, `0.2.0-alpha`, each adding new functionality and converging to the 1.0.0 release.  The early ones will all be marked alpha until there are steady users who require a more stable `0.x` release, if that is required we will promote one of the `alpha`s.  Bug fixes to any of the releases will increment the third number, at this early stage most users should be willing to just switch to the next release though.



