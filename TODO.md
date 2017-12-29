- when fixing equivalence, probably use `IsEquivalentTo(node, false)`
- fix moving `NamedTypeReference`s between files (doesn't update `using`s)
- make most classes `sealed`

---

- for IntelliSense, create bodies of user methods, but not of new transformer methods
- make writing flags enums easier
  - use it for `MemberModifiers`
- consider creating code from "template" lambda or local function  
  - basically automatic code quoter
  - "lambda" parameters for inlining other code blocks
- write a tool that ensures no Roslyn types are exposed in the public interface
  - protected abstract methods will probably have to treated as an exception
