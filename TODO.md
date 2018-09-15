- clone args in `CollectionTransformer<,,>.Data`

---

- figure out better names than `Project`, `Project` and `SourceFile`, `SourceFile`
- fix moving `NamedTypeReference`s between files (doesn't update `using`s)
- make most classes `sealed`
- allow modifying lists while iterating them
  - optional: make it more efficient using copy on write 
- detect reading of References/Symbols, then rerun when those symbols change
- when `Clear()`ing a list of members, or more generally resetting `Parent` to `null`, make sure semantics of detached nodes still work by storing cached versions of necessary Roslyn types

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
