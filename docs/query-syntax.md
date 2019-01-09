---
title: Query Syntax
nav_order: 3
---

# Syntax Diagram

<pre>
(f|v)<i>Subject</i> [<i>Alias-List</i>|<i>Aggregation</i>] (<i>Condition</i>)
</pre>

### _Subject_

A request must have a _Subject_, which is a table or view name starting with either `f` or `v`.  

#### Examples

Address Book Table.
```
f0101
```

Open Purchase Order by Item.
```
v4311jo
```

Submitting a _Subject_ only request will fetch all fields of up to _Maximum Rows_.

## _Alias-List_

An _Alias-List_ is a comma separated list of table's or view's aliases to list.  The table prefix can be ignored if the alias is unique, like in tables for example.

A query can have either _Alias-List_ or _Aggregation_.

### Examples

List Address Book's name and number.
```
f0101 (an8,alph)
```

List branch, document number and type and open quantity on F4311 and F4316.
```
v4311jo (mcu,f4311.doco,f4311.dcto,f4311.uopn,f4316.uopn)
```

__Note:__ The _mcu_ alias does not need to be prefixed with table since it's only in F4311.

## _Aggregation_

The following aggregate functions are available:

- sum
- min
- max
- avg
- count
- count_distinct
- avg_distinct
- group

In addition there are two sequence functions:

- desc
- asc

The form of an aggregate is:

<pre>
<i>Aggreate-Function</i>(<i>Alias-List</i>)
</pre>

__Note:__ Items in the _Alias-List_ __must__ be prefixed with the table.

### Examples

Show highest, lowest, average and total order line amounts.

```
f4311 [max(f4311.aexp) min(f4311.aexp) avg(f4311.aexp) sum(f4311.aexp)]
```

Repeat the above, but group and sort the result by supplier and count orders and lines.

```
f4311 [
	max(f4311.aexp)
	min(f4311.aexp)
	avg(f4311.aexp)
	sum(f4311.aexp)
	count_distinct(f4311.doco,F4311.dcto)
	count(f4311.an8)
	group(f4311.an8)
	asc(f4311.an8)]
```

__Note:__ The _count_distinct_ function counts rows with unique _doco_ and _dcto_.

## _Condition_

The form of a condition is:

<pre>
(all|any)(<i>Alias</i> <i>Operator</i> <i>Literal(s,)...</i> ...)
</pre>

The `all` prefix requires that all condidtions must be met and `any` requires that one of the condition must met (`AND`/`OR` equivalent).

The available _Operators_ are:

- Equal `=`
- Greater Than `>`
- Less Than `<`
- Greater or Equal `>=`
- Less or Equal `<=`
- Not Equal `<>`
- Between `bw`
- In List `in`
- String Contains `?`
- String is Blank `_`
- String is Not Blank `!`
- String Starts With `^`
- String Ends With `$`

### Examples

List Sum of Open OP Orders.

```
f4311 [
	group(f4311.doco,f4311.dcto) sum(f4311.aexp)]
	all(f4311.nxtr = 400 f4311.dcto = OP f4311.uopn <> 0)
```

List items with non-blank cat code 2 and contains "Bike" in the description.

```
f4101 (itm,litm,dsc1) all(f4101.srp2 ! blank f4101.srtx ? Bike)
```

__Note:__ The `!` operator only require a literal to keep up with the syntax.

List Work Orders with Status between 10 and 40.

```
f4801 (doco,dl01,srst) all(f4801.srst bw "10","45")
```

__Note:__ Literals can optionally be enclosed in quotation marks which is useful when it contains special characters.
