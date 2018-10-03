1. try changing the language of *.cs files in VS to `CSharpE` by using `IContentTypeRegistryService`
2. if that doesn't work, use the hack of setting `ServiceLayer` to `Host`

---

- remove the "persistent" term from the codebase (and maybe also "cloneable"?)

---

- fix moving `NamedTypeReference`s between files (doesn't update `using`s)
- detect reading of References/Symbols, then rerun when those symbols change
- when `Clear()`ing a list of members, or more generally resetting `Parent` to `null`, make sure semantics of detached nodes still work by storing cached versions of necessary Roslyn types
- allow `Where` in smart loops; check there are no sideefects on the node same as `ForEach`
- allow modifying lists while iterating them
  - optional: make it more efficient using copy on write 
- add tests ensuring mutating smart inputs (incl. syntax nodes) throws
- make most classes `sealed`

---

- for IntelliSense, create bodies of user methods, but not of new transformer methods
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
