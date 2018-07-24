---
title: Clone and Serialize
keywords: 
last_updated: June 28, 2018
tags: [developers]
sidebar: home_sidebar
permalink: home_clone_and_serialize.html
folder: home
---

## Clone

Because the `Product`s and `Simulator`s are slightly stateful they need to be copied for use in each thread during a valuation.  The `Clone` method is implemented using serialization and deserialization with [Json.NET](https://www.newtonsoft.com/json).  This has some consequences for how classes are written and constructed, see below.

## Serialize

Any object that might need to be serialized either for cloning or because it will be included in a message to perform a calculation will be serialized with [Json.NET](https://www.newtonsoft.com/json) and needs to have the following features:

* The constructor must not rely on any non null inputs, or it must provide an empty private constructor
* The constructor must not perform in initialization or calculation, only assignment
* If you have a property that needs to be initialized from other class data then have a backing field and a property that implements the initialization in the getter.  Mark both the field and the property with [JsonIgnore](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonIgnoreAttribute.htm)
* Dictionaries can only have primitive keys.

## Example

<script src="https://cdn.rawgit.com/google/code-prettify/master/loader/run_prettify.js"></script>
<pre class="prettyprint">
        private readonly Date[] _dates;
        private readonly double[] _rates;

        [JsonIgnore] private LinearSpline _spline;
        
        [JsonIgnore]
        private LinearSpline Spline
        {
            get
            {
                if (_spline != null) return _spline;
                _spline = LinearSpline.InterpolateSorted(_dates.Select(d => (double) d.value).ToArray(), _rates);
                return _spline;
            }
        }        
</pre>
     