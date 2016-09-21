---
title: Writing Plugins
keywords: 
last_updated: September 14, 2016
tags: [getting_started]
sidebar: home_sidebar
permalink: home_plugins.html
folder: home
---

## Introduction

QuantSA allows plugins that may be required for several reasons such as:

* a user may require bespoke functionality that does not fit naturally into QuantSA.  
* a quant may have some proprietary models that they would like to sell to a particualr party and not make available to everyone
* a regular user of QuantSA may write for themselves, or commission a quant to write, something specific to their needs 

## Architecture

See the example project: <https://github.com/JamesLTaylor/QuantSA/tree/master/PluginDemo> for an easy prototype of how to write Plugins

### Custom button icons

In your custom ribbon you can add button tags like:

    image='MainLogo'

You will then need to make sure that your assembly contains a public static method like:

    public static System.Drawing.Bitmap get_MainLogo()

The QuantSA plugin will then find this resource when the plugin is loaded and use it in the Ribbon. 

