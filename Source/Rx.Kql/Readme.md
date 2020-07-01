# [Rx.Kql] Nuget package

## Package Source
http://wanuget/Official/nuget/ 

## Standing query in Kusto language

This document and code assumes the reader is familiar with **L**anguage **IN**tegrated **Q**uery (LINQ) and Reactive Extensions (**Rx.Net**). 

LINQ and Rx are old and stable .Net framework pieces described in chapters 10-11 in the book  [Programming C# 5.0](http://shop.oreilly.com/product/0636920024064.do). Also good books are [LINQ Pocket Reference](http://shop.oreilly.com/product/9780596519254.do) and [Programming Reactive Extensions and LINQ](http://www.apress.com/us/book/9781430237471) . Online book is [http://introtorx.com](http://introtorx.com)

Specifically terms like "real-time" here mean **milliseconds** of latency and implementation based on push-callbacks (IObservable). 


## About the Rx.Kql class library

This is class library that implements subset of the Kusto language on real-time (IObservable) pipelines of callbacks. This is using just the language syntax 

There are no dependencies on the Kusto API or the "storage-then-query infrastructure". Instead the query can be Rx.Kql driven.

### There are two parts of the code:
#### Abstract Syntax Tree (AST) implementation 
This is implemented in [KustoParser.cs](KustoParser.cs), as a class for each of the Kusto language operators. There are three usage scenarios for the AST:
- Explicitly building the AST, and then serializing it to Kusto query
- Parsing Kusto query from string to build AST
- Evaluating operators and expressions on "dynamic" C# instance

#### Operators in System.Reactive.Linq
These are normal Rx operators that can be used on IObservable pipelines as follows:

```csharp
var points = Observable.FromEventPattern<MouseEventArgs>(panel1, "MouseMove")
                    .ToDynamic(m => m.EventArgs)
                    .KustoQuery("where Button == Left | project X, Y");
```

##### Here:
- ToDynamic converts instances of static types to isomorphic "dynamic" representation. 
- KustoQuery evaluates a query on the live-stream of dynamic instances

See the WinForms [sample on mouse move events](\src\Samples\Rx.Kql.MouseMove\Readme.md).