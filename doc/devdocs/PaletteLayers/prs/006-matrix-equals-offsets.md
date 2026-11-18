## General description
`Matrix` holds its six elements as doubles. `Matrix.Equals` compared the offsets of this matrix with the `OffsetX` and
`OffsetY` of the other matrix, which round the offsets to floats. So a matrix whose offsets a float does not hold, as
a rotation about a point gives, was not equal to its copy. `Equals` now compares the offsets as doubles, as it compares
the other elements and as `GetHashCode` hashes them.

## Changes in detail
* `Graphics2D/Matrix.cs`: `Equals(Matrix)` compares `_offsetX` and `_offsetY` of both matrices.
* `Graphics2DStd/Matrix.cs`, the same class in the namespace `System.Drawing.Drawing2D`, gets the same change.
  `MapModel.sln` lists its project, and `PPen.slnx` does not build it.
* `MatrixTests.EqualsItsCopy` compares a matrix that rotates about a point with its copy, and with a moved copy. It
  fails without the change.
* No code of Purple Pen calls `Matrix.Equals`, so the drawing does not change.

## Issue ticket number
[006-matrix-equals-offsets](../tickets/006-matrix-equals-offsets.md)

## Missing functionality
None.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
