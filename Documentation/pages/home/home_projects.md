---
title: The project and solution structure
summary: How the QuantSA projects are grouped into solutions and their depdencies.
keywords: 
last_updated: December 26, 2016
tags: developers
sidebar: home_sidebar
permalink: home_projects.html
folder: home
---

The sections below are ordered according to the order in which you would compile them.

## QuantSACore.sln

The main library and tests.  No depedencies to the rest of QuantSA.

## QuantSAExcelCommon.sln

The interfaces and utilities used by the excel addin and plugins.  Also needed for generating Excel code.

## GenerateXLCode.sln

This solution/project produces an executable called `GenerateXLCode.exe`.  This is used in the build steps of `QuantSAExcelAddIn.sln`.

## QuantSAExcelAddIn.sln

Produces the Excel addin.  

## PrepareRelease.sln

A collection of useful steps that make some consistency checks on the solution.  At the moment this can only run on a developers machine because it needs Excel installed.

## QuantSASetup.sln

An InstallShield Setup Project for QuantSA ().

### Requirements
* Visual Studio 2015 Professional or higher (The Community Edition will not suffice).
* InstallShield Limited Edition.

The setup project ensures that the client has the required .NET framework and automatically adds the correct verison of the add-in to all installed versions of Excel.

