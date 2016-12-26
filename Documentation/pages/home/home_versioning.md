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

The dlls are versioned by the contents of `AssemblyInfoAll.cs`.

The version displayed alongside the Excel add-in is the string in `QuantSA.dna`.

# Policy

Versions should follow <http://semver.org/>.

At the moment we are just heading towards a 1.0 release.  That would be a version that provides a reasonable amount of functionality and has had 100s of hours of testing in the wild.

