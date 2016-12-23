---
title: Clone and Serialize
keywords: 
last_updated: October 31, 2016
tags: [developers]
sidebar: home_sidebar
permalink: home_clone_and_serialize.html
folder: home
---

## Clone

Because the `Product`s and `Simulator`s are slightly stateful they need to be copyied for use in each thread during a valuation.  The `Clone` method is implemented on the base class using in memory binary serialization.  If for some reason it is not possible to mark your implementation of one of these classes as `Serializeable` then you need to overwrite the `Clone` method.

As always cloning can be risky business.  Unless you are absolutely certain that a field in the object is immutable take a deep copy of all data members in the Clone method.

## Serialize

As discussed above all implementations of `Product` and `Simulator` need to be serializable.  Currently this is only used to allow generic cloning of these objects but may be more widely required when the calculations are moved into a grid or cloud.