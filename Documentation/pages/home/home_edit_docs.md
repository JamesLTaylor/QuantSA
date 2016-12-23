---
title: Setup the documentation
keywords: 
last_updated: September 14, 2016
tags: [getting_started]
sidebar: home_sidebar
permalink: home_edit_docs.html
folder: home
---

The documentation for QuantSA can be found [here](https://jamesltaylor.github.io/ "https://jamesltaylor.github.io/")

The source code is in markdown and is based on:  <https://github.com/tomjohnson1492/documentation-theme-jekyll>.  

The source code can be found [here](https://github.com/JamesLTaylor/jamesltaylor.github.io/tree/master/pages/ "https://github.com/JamesLTaylor/jamesltaylor.github.io/tree/master/pages/")

To compile this documentation locally to review your edits, follow any of the many instructions online for building and running a [Jekyll](https://jekyllrb.com/) site.  For example at the site for the very theme that we are using here: <http://idratherbewriting.com/documentation-theme-jekyll/>.  

But in short once everything is setup:

* open a command prompt with ruby
* navigate to the root folder of these the docs
* run `jekyll serve` or for some unknown reason in my setup: `bundle exec jekyll serve`

## Maths

The following example shows how to include maths.

<script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>
<div>
$$a^2 + b^2 = c^2$$
</div>

*Note* this does not work on Github static pages.

