- figure out better names than `Project`, `Project` and `SourceFile`, `SourceFile`
- fix moving `NamedTypeReference`s between files (doesn't update `using`s)
- make most classes `sealed`
- allow modifying lists while iterating them
  - optional: make it more efficient using copy on write 
- detect reading of References/Symbols, then rerun when those symbols change
- somehow handle multi-level collections like `project.ForEachTypeWithAttribute` (project → file → type)

---

- for IntelliSense, create bodies of user methods, but not of new transformer methods
- make writing flags enums easier
  - use it for `MemberModifiers`
- consider creating code from "template" lambda or local function  
  - basically automatic code quoter
  - "lambda" parameters for inlining other code blocks
- write a tool that ensures no Roslyn types are exposed in the public interface
  - protected abstract methods will probably have to treated as an exception
- consider rewriting existing Roslyn-based rewriter project into CSharpE, like LinqAF
