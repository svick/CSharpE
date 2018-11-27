- CSharpE.MSBuild should run (in a design-time mode?) during a design-time build, if the VS extension is not installed it should also produce a warning

---

- fix moving `NamedTypeReference`s between files (doesn't update `using`s)
- detect reading of References/Symbols, then rerun when those symbols change
- when `Clear()`ing a list of members, or more generally resetting `Parent` to `null`, make sure semantics of detached nodes still work by storing cached versions of necessary Roslyn types
- allow `Where` in smart loops; check there are no sideefects on the node same as `ForEach`
- allow modifying lists while iterating them
  - optional: make it more efficient using copy on write 
- add tests ensuring mutating smart inputs (incl. syntax nodes) throws
- make most classes `sealed`
- look into how InvocationReasons.SemanticChanged works and if it could be used to decide when to rerun the whole transformation
- consider using limited Roslyn formatting instead of `NormalizeWhitespace`
- remove ITransformation.AdditionalReferences: it doesn't seem to be useful

---

- make writing flags enums easier
  - use it for `MemberModifiers`
- consider creating code from "template" lambda or local function  
  - basically automatic code quoter
  - "lambda" parameters for inlining other code blocks
- ensure no Roslyn types are exposed in the public interface
- consider rewriting existing Roslyn-based rewriter project into CSharpE, like LinqAF
- think of how to handle segments that don't change in design time, but do change in build time (see `RecordTransformation`)
- consider smart foreach for user data
  - probably using levenstein distance for diffing
    - no need to use levenstein, if you can be unordered and set-based?
  - and/or maybe some kind of cache/factory?