---
title: Setting up and running test
keywords: 
last_updated: September 14, 2016
tags: [getting_started]
sidebar: home_sidebar
permalink: home_tests.html
folder: home
---

## Writing tests

A useful function for transferring your Excel use case into a CSharp test is:

`=QSA.GetCSArray(data)`

Which turns a block of numbers into something like:


{% raw %}
```cs
{{3578,0,0,0,0,0,0,0,0,3578},
{0,0,0,272,0,106,0,0,0,378},
{0,100,33,33,0,75,1242,750,0,2234},
{131,791,668,1465,6059,4787,12302,18641,4349,49194},
{11,476,0,16,37,0,0,0,0,539}}
```
{% endraw %}

