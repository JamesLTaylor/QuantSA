---
title: Generate markdown
summary: Generate markdown from Excel function attributes
keywords: 
last_updated: June 23, 2018
tags: [developers]
sidebar: home_sidebar
permalink: home_generate_markdown.html
folder: home
---

# Introduction

The skeleton of the help markdown files can be generated from the attributes set when a function is exposed to excel.  Use the solution:

<https://github.com/JamesLTaylor/QuantSA/tree/master/PrepareRelease> 

 * Update the output folder to the location where  you have the documentation checked out.
 * Compile and run the solution.

Note that the docs are built officially by the Travis build described [here](home_builds.html).

## Links for Excel input types

Types that are listed in `\_data\sidebars\excel_sidebar.yml` before the line `# AUTO GENERATED BEYOND THIS POINT` are all used by PrepareRelease to add links in the generated documentation when that type appears.  When the type is an interface it will start with an I in its C# name but the type listed in the sidebar should not have the I - this makes the documentation easier to read by non C# developers.

