---
title: The project and solution structure
summary: How the QuantSA projects are grouped into solutions and their dependencies.
keywords: 
last_updated: June 11, 2018
tags: developers
sidebar: home_sidebar
permalink: home_projects.html
folder: home
---

There are several solutions contained in the QuantSA repository, these are listed below with short descriptions.  All solutions can be compiled with Visual Studio 2017 community edition.

## QuantSA.sln

The main library, tests and Excel Add-in.  This is a complete solution and is the most common place to develop.  See [here](home_code_intro.html) for details of the projects.

## QuantSA.Main.sln

QuantSA without the Excel layer. 

## QuantSA.Shared.sln

If you wish to write a plug-in then this is the only project that needs to be referenced.

## PrepareRelease.sln

A collection of useful steps that make some consistency checks on the solution.  At the moment this can only run on a developers machine because it needs Excel installed.  This can only be run after QuantSA.sln has been compiled since it references the dlls created by that.

## QuantSAInstaller.sln

A hand written installer with no dependencies on anything extra being installed in visual studio.

## QuantSAPluginDemo.sln

An example plugin.

## QuantSASetup.sln

An InstallShield Setup Project for QuantSA.

### Requirements
* Visual Studio 2015 Professional or higher (The Community Edition will not suffice).
* InstallShield Limited Edition.

The setup project ensures that the client has the required .NET framework and automatically adds the correct version of the add-in to all installed versions of Excel.

