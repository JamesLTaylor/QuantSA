---
title: Automated builds
summary: The QuantSA automated builds
keywords: 
last_updated: December 26, 2016
tags: developers
sidebar: home_sidebar
permalink: home_builds.html
folder: home
---

# Overview

QuantSA has some level of continuous integration.  This currently run in two different places:

1.   <https://travis-ci.org/JamesLTaylor/QuantSA>
2.   <https://jameslatimertaylor.visualstudio.com/QuantSA/_build>

Both of which need private log on details and can't be changed by other developers.  Please contact [me](mailto:James@cogn.co.za) if you would like to be involved in the building process.

## Travis

Builds the markdown docs using Jekyll and checks that there are no dead links.  This was easier to set up on Travis than installing Ruby into Visual Studio Team Servives.

## Visual Studio

The C# solutionns are all built here and the unit tests are run.


## Further work

None of the excel functionality is currently tested.  This needs to be added somewhere rather than relying on developer's good behaviour.
