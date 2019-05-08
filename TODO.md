- fix moving `NamedTypeReference`s between files (doesn't update `using`s)
- make most classes `sealed`
- consider using limited Roslyn formatting instead of `NormalizeWhitespace`
- identifier should handle escaping keywords (including contextual ones, those can be escaped unnecessarily)

---

- detect reading of References/Symbols, then rerun when those symbols change
- when `Clear()`ing a list of members, or more generally resetting `Parent` to `null`, make sure semantics of detached nodes still work by storing cached versions of necessary Roslyn types
- allow `Where` in smart loops; check there are no sideefects on the node same as `ForEach`
- allow modifying lists while iterating them
  - optional: make it more efficient using copy on write 
- add tests ensuring mutating smart inputs (incl. syntax nodes) throws
- look into how InvocationReasons.SemanticChanged works and if it could be used to decide when to rerun the whole transformation
- consistent treatment of verbatim strings and verbatim interpolated strings

---

- make writing flags enums easier
  - use it for `MemberModifiers`
- consider creating code from "template" lambda or local function  
  - basically automatic code quoter
  - "lambda" parameters for inlining other code blocks
- ensure no unwated Roslyn types are exposed in the public interface
- consider rewriting existing Roslyn-based rewriter project into CSharpE, like LinqAF
- think of how to handle segments that don't change in design time, but do change in build time (see `RecordTransformation`)
- consider smart foreach for user data
  - probably using levenstein distance for diffing
    - no need to use levenstein, if you can be unordered and set-based?
  - and/or maybe some kind of cache/factory?
- CSharpE.MSBuild should run (in a design-time mode?) during a design-time build, if the VS extension is not installed it should also produce a warning
- design-time transformations shouldn't add `using`s, because those are observable
- add `Smart.ForEachWithSegment` (but with better name), which allows adding members to a type based on a collection of other members
  - might require adding an abstraction for dependencies? (node/limited node/data)
- improve debugging of extensions by using launchSettings.json
- improve `CollectionTransformer` reuse by allowing different `Data`
