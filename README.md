Logical parser
=================
Author: Vojtech Mrazek, student of Faculty of Information Technologies in Brno (Czech Republic)
Date: November 2012

Parser of logical expression ( and, or, >, &lt;, ,,) for Microsoft C#. It's simple LL grammar parser.
Because of generic types you can use this project in lot of cases (simple SQL parser, filter settings etc.).

You can see usage example in project ParserTest. There are 2 additional classes
  - Finder - parsing variables to IDs (comparing of integer is more quick than strings)
  - Elemen - elements which you want to test expression on, there must be only 2 functions
  

Possible conditions
===============================
  - a & number - true in case ''a & number = number''
  - a | number - true in case ''a | number != 0''
  - a [cond] number - cond may be >= > < <= =
  - a = string - a must be string variable
 
Possible conjunctions
====================================
  - and - both cases
  - or - one of cases
  - not - negation

and , or use short evaluation - second subtree may not be evalated

In priority case you its designed, that
  not > and > or

